using Azure.Messaging.ServiceBus;
using MeterReading.Helper;
using MeterReading.Service.Interface;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MeterReading.Function;

public class ReadMeterVodafone(ILogger<ReadMeterVodafone> logger, IReadMeterVodafoneService readMeterVodafoneService)
{
    [Function(nameof(ReadMeterVodafone))]
    public async Task Run(
        [ServiceBusTrigger(ServiceBusQueues.StagingMeterVodafoneQueue, Connection = ServiceBus.AzureServiceBusConnection)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        await readMeterVodafoneService.ReadMeter(message, messageActions);
    }
}