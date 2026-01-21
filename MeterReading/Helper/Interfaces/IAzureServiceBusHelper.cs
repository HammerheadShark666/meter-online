namespace MeterReading.Function.Helpers.Interfaces;

public interface IAzureServiceBusHelper
{
    Task SendMessagesAsync(string queue, IEnumerable<string> meters);
}