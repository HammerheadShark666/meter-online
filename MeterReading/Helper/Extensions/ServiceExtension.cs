using MeterReading.Function.Helpers;
using MeterReading.Function.Helpers.Interfaces;
using MeterReading.Helper.Interfaces;
using MeterReading.Service;
using MeterReading.Service.Interface;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;

namespace MeterReading.Helper.Extensions;
public static class ServiceExtensions
{
    public static void ConfigureDependencyInjection(IServiceCollection services)
    {
        services.AddScoped<IStagingMeterService, StagingMeterService>();
        services.AddScoped<IAzureServiceBusHelper, AzureServiceBusHelper>();
        services.AddScoped<IMongoDbHelper, MeterReading.Helper.MongoDbHelper>();
    }

    public static void ConfigureApplicationInsightsTelemetryWorkerService(this IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetryWorkerService();
    }

    public static void ConfigureFunctionsApplicationInsights(IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetryWorkerService();
    }

    public static void ConfigureAzureServiceBusClient(this IServiceCollection services)
    {
        var azureServiceBusConnection = EnvironmentVariables.GetEnvironmentVariable(ServiceBus.AzureServiceBusConnection);

        services.AddAzureClients(builder =>
        {
            builder.AddServiceBusClient(EnvironmentVariables.GetEnvironmentVariable(azureServiceBusConnection));
        });
    }
}