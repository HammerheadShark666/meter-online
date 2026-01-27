using Azure.Messaging.ServiceBus;
using MeterReading.Domain;
using MeterReading.Helper;
using MeterReading.Helper.Exceptions;
using MeterReading.Helper.Interfaces;
using MeterReading.Service.Interface;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeterReading.Service;
public sealed class ReadMeterVodafoneService(ILogger<ReadMeterVodafoneService> logger, IAzureServiceBusHelper azureServiceBusHelper) : IReadMeterVodafoneService
{
    private static readonly Random _random = new();

    public async Task ReadMeter(ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        using var scope = logger.BeginScope("MessageId: {MessageId}", message.MessageId);

        var meter = await GetMeterAsync(message, messageActions)
             ?? throw new MeterNotDeserialisedException($"Meter failed to deserialise MessageId: {message.MessageId}");

        try
        {
            if (meter.MeterNumber.Equals("8515111350"))
                throw new MeterNotReadException("Failed meter reading for testing Dead Letter Queue");

            var latestReading = UpdateMeterReading(meter.LastReading);

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
            var meter = JsonSerializer.Deserialize<Meter>(message.Body, JsonHelper.JsonOptions);
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

    private static decimal UpdateMeterReading(decimal lastReadingValue)
    {
        //In real world this code would connect to the meter and get the reading.
        //Here we will simulate this by generating a random number between MinValue and MaxValue.

        decimal min = ReadMeterValue.MinValue;
        decimal max = ReadMeterValue.MaxValue;

        decimal meterIncrement =
            min + (decimal)_random.NextDouble() * (max - min);

        return Math.Round(lastReadingValue + meterIncrement, 2, MidpointRounding.AwayFromZero);
    }
}