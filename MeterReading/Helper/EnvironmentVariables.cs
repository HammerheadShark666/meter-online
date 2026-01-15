using MeterReading.Helper.Exceptions;

namespace MeterReading.Helper;
public class EnvironmentVariables
{
    public static string AzureServiceBusConnection => GetEnvironmentVariable(ServiceBus.AzureServiceBusConnection);

    public static string GetEnvironmentVariable(string name)
    {
        var variable = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrEmpty(variable))
            throw new EnvironmentVariableNotFoundException($"Environment Variable Not Found: {name}.");

        return variable;
    }
}