namespace MeterReading.Helper.Exceptions;

public class EnvironmentVariableNotFoundException(string message) : Exception(message)
{
}