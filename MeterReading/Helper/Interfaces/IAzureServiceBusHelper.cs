using MeterReading.Domain;

namespace MeterReading.Helper.Interfaces;

public interface IAzureServiceBusHelper
{
    Task SendMessagesInBatchAsync(string queue, IEnumerable<string> meters);
    Task SendMessageAsync(string queue, MeterReadingToSave meterReadingToSave);
}