using Azure.Messaging.ServiceBus;
using MeterReading.Helper;
using MeterReading.Service.Interface;
using Microsoft.Azure.Functions.Worker;

namespace MeterReading.Function;

public class UpdateMeterReading(IUpdateMeterReadingService updateMeterReadingService)
{
    [Function(nameof(UpdateMeterReading))]
    public async Task Run([ServiceBusTrigger(ServiceBusQueues.SuccessfullyReadMeterVodafoneQueue, Connection = ServiceBus.AzureServiceBusConnection)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        await updateMeterReadingService.UpdateMeterReading(message, messageActions);
    }
}
