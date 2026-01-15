namespace MeterReading.Function.Helpers.Interfaces;

public interface IAzureServiceBusHelper
{
    Task SendMessageAsync(string queue, string data);
}