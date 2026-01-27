using Azure.Messaging.ServiceBus;
using MeterReading.Domain;
using MeterReading.Helper.Interfaces;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MeterReading.Helper;

internal class AzureServiceBusHelper(ServiceBusClient client) : IAzureServiceBusHelper, IAsyncDisposable
{
    private readonly ServiceBusClient _client = client;
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

    public async Task SendMessagesInBatchAsync(string queue, IEnumerable<string> meters)
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

    public async Task SendMessageAsync(string queue, MeterReadingToSave meterReadingToSave)
    {
        var sender = _senders.GetOrAdd(queue, q => _client.CreateSender(q));

        var message = ToServiceBusMessage(meterReadingToSave, "Meter read successfully");

        await sender.SendMessageAsync(message);
    }

    public static ServiceBusMessage ToServiceBusMessage<T>(T value, string? subject = null)
    {
        return new ServiceBusMessage(JsonSerializer.Serialize(value))
        {
            ContentType = "application/json",
            Subject = subject,
            MessageId = Guid.NewGuid().ToString()
        };
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
        {
            await sender.DisposeAsync();
        }
    }
}