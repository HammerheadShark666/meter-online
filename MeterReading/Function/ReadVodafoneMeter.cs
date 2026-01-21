using Azure.Messaging.ServiceBus;
using MeterReading.Domain;
using MeterReading.Helper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeterReading.Function;

public class ReadVodafoneMeter(ILogger<ReadVodafoneMeter> logger)
{
    private readonly ILogger<ReadVodafoneMeter> _logger = logger;

    [Function(nameof(ReadVodafoneMeter))]
    public async Task Run(
        [ServiceBusTrigger(ServiceBusQueues.StagingMeterVodafoneQueue, Connection = ServiceBus.AzureServiceBusConnection)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);


        Meter? meter = JsonSerializer.Deserialize<Meter>(message.Body);
        if (meter is null)
        {
            _logger.LogError("Failed to deserialize Meter from message body. MessageId: {id}", message.MessageId);
            return;
        }

        await messageActions.CompleteMessageAsync(message);
    }
}