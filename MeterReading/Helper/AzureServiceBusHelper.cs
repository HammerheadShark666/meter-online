using Azure.Messaging.ServiceBus;
using MeterReading.Function.Helpers.Interfaces;

namespace MeterReading.Helper;

internal class AzureServiceBusHelper(ServiceBusClient serviceBusClient) : IAzureServiceBusHelper
{
    public async Task SendMessageAsync(string queue, string data)
    {
        var sender = serviceBusClient.CreateSender(queue);
        await sender.SendMessageAsync(new ServiceBusMessage(data));
    }
}