namespace MeterReading.Helper;

internal static class Constants
{
    public const string TelecomsVodafone = "vodafone";
    public const string TelecomsO2 = "O2";
    public const string TelecomsEE = "EE";
}

internal static class MeterType
{
    public const string Solar = "solar";
}

internal static class MongoSettings
{
    public const string ConnectionString = "MongoDbConnection";
    public const string Database = "meter-online-db";
    public const string Meters = "meters";
}

internal static class MongoCollections
{
    public const string Meters = "meters";
}

internal static class ServiceBus
{
    public const string AzureServiceBusConnection = "AZURE_SERVICE_BUS_CONNECTION";
}

internal static class ServiceBusQueues
{
    public const string StagingMeterVodafoneQueue = "sbq-staging-read-vodafone";
    public const string SuccessfullyReadMeterVodafoneQueue = "sbq-successfully-read-vodafone";
}

internal static class ReadMeterValue
{
    public const decimal MinValue = (decimal)0.0;
    public const decimal MaxValue = (decimal)150.0;
}