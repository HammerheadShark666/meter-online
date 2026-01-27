using Azure.Messaging.ServiceBus;
using MeterReading.Helper;
using MeterReading.Service.Interface;
using Microsoft.Azure.Functions.Worker;

namespace MeterReading.Function;

public class ReadMeterVodafone(IReadMeterVodafoneService readMeterVodafoneService)
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