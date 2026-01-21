using Azure.Messaging.ServiceBus;
using MeterReading.Function.Helpers.Interfaces;
using System.Collections.Concurrent;

namespace MeterReading.Helper;

internal class AzureServiceBusHelper(ServiceBusClient client) : IAzureServiceBusHelper, IAsyncDisposable
{
    private readonly ServiceBusClient _client = client;
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

    public async Task SendMessagesAsync(string queue, IEnumerable<string> meters)
    {
        var sender = _senders.GetOrAdd(queue, q => _client.CreateSender(q));
        ServiceBusMessageBatch batch = await sender.CreateMessageBatchAsync();

        foreach (var meter in meters)
        {
            if (!batch.TryAddMessage(new ServiceBusMessage(meter)))
            {
                await sender.SendMessagesAsync(batch);
                batch.Dispose();

                batch = await sender.CreateMessageBatchAsync();

                if (!batch.TryAddMessage(new ServiceBusMessage(meter)))
                {
                    throw new InvalidOperationException("Message too large for Service Bus");
                }
            }
        }

        if (batch.Count > 0)
        {
            await sender.SendMessagesAsync(batch);
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
        {
            await sender.DisposeAsync();
        }
    }
}