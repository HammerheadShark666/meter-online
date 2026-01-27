using Azure.Messaging.ServiceBus;
using MeterReading.Domain;
using MeterReading.Helper;
using MeterReading.Helper.Exceptions;
using MeterReading.Helper.Interfaces;
using MeterReading.Service.Interface;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeterReading.Function;

public class ReadVodafoneMeter(ILogger<ReadVodafoneMeter> logger, IReadMeterVodafoneService readMeterVodafoneService, IAzureServiceBusHelper azureServiceBusHelper)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Function(nameof(ReadVodafoneMeter))]
    public async Task Run(
        [ServiceBusTrigger(ServiceBusQueues.StagingMeterVodafoneQueue, Connection = ServiceBus.AzureServiceBusConnection)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        using var scope = logger.BeginScope("MessageId: {MessageId}", message.MessageId);

        var meter = await GetMeterAsync(message, messageActions)
            ?? throw new MeterNotDeserialisedException($"Meter failed to deserialise MessageId: {message.MessageId}");

        try
        {
            if (meter.MeterNumber.Equals("8515111350"))
                throw new MeterNotReadException("Failed meter reading for testing Dead Letter Queue");


            var latestReading = readMeterVodafoneService.ReadMeter(
                meter.MeterNumber,
                meter.ConnectionNumber,
                meter.LastReading);

            var meterReadingToSave = new MeterReadingToSave()
            {
                Id = meter.Id,
                MeterNumber = meter.MeterNumber,
                UserId = meter.UserId,
                LastReading = latestReading,
                LastReadOn = DateTime.UtcNow
            };

            await azureServiceBusHelper.SendMessageAsync(ServiceBusQueues.SuccessfullyReadMeterVodafoneQueue, meterReadingToSave);

            await messageActions.CompleteMessageAsync(message);
        }
        catch (MeterNotReadException mrex)
        {
            logger.LogError("Meter failed to read - {meterNumber}", meter.MeterNumber);
            await messageActions.DeadLetterMessageAsync(
                message,
                deadLetterReason: "Meter failed to read",
                deadLetterErrorDescription: mrex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed while processing meter {MeterNumber}", meter.MeterNumber);
            throw;
        }
    }

    private async Task<Meter?> GetMeterAsync(ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        try
        {
            var meter = JsonSerializer.Deserialize<Meter>(message.Body, JsonOptions);
            if (meter == null)
            {
                logger.LogError("Meter payload was null after deserialization");

                await messageActions.DeadLetterMessageAsync(
                    message,
                    deadLetterReason: "Invalid Meter",
                    deadLetterErrorDescription: "Meter was null");

                return null;
            }

            return meter;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize Meter message");

            await messageActions.DeadLetterMessageAsync(
                message,
                deadLetterReason: "Deserialization Failed",
                deadLetterErrorDescription: ex.Message);

            return null;
        }
    }
}