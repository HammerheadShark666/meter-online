using Azure.Messaging.ServiceBus;
using MeterReading.Domain;
using MeterReading.Helper;
using MeterReading.Service.Interface;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeterReading.Function;

public class ReadVodafoneMeter(ILogger<ReadVodafoneMeter> logger, IReadMeterVodafoneService readMeterVodafoneService)
{
    private readonly ILogger<ReadVodafoneMeter> _logger = logger;

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
        using (_logger.BeginScope("MessageId: {MessageId}", message.MessageId))
        {
            Meter? meter;

            try
            {
                meter = JsonSerializer.Deserialize<Meter>(message.Body, JsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize Meter message");
                await messageActions.DeadLetterMessageAsync(
                    message,
                    deadLetterReason: "Deserialization Failed",
                    deadLetterErrorDescription: ex.Message);
                return;
            }

            if (meter is null)
            {
                _logger.LogError("Meter payload was null after deserialization");
                await messageActions.DeadLetterMessageAsync(
                    message,
                    deadLetterReason: "Invalid Meter",
                    deadLetterErrorDescription: "Meter was null");
                return;
            }

            try
            {
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

                // TODO: send updated meter to next queue

                await messageActions.CompleteMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed while processing meter {MeterNumber}", meter.MeterNumber);
                throw;
            }


        }


        //    Meter? meter = JsonSerializer.Deserialize<Meter>(message.Body);
        //if (meter is null)
        //{
        //    _logger.LogError("Failed to deserialize Meter from message body. MessageId: {id}", message.MessageId);
        //    return;
        //}

        //var latestReading = readMeterVodafoneService.ReadMeter(meter.MeterNumber, meter.ConnectionNumber, meter.lastReading);

        //meter.lastReading = latestReading;
        //meter.lastReadOn = DateTime.UtcNow;

        ////serialize updated meter and send to updating queue    


        //await messageActions.CompleteMessageAsync(message);
    }
}