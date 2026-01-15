using MeterReading.Helper.Extensions;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        // services.AddApplicationInsightsTelemetryWorkerService();
        // services.ConfigureFunctionsApplicationInsights();
        //services.AddScoped<IStagingMeterService, StagingMeterService>();
        //services.AddScoped<IAzureServiceBusHelper, AzureServiceBusHelper>();
        //services.AddScoped<IMongoDbHelper, MeterReading.Helper.MongoDbHelper>();

        ServiceExtensions.ConfigureApplicationInsightsTelemetryWorkerService(services);
        ServiceExtensions.ConfigureFunctionsApplicationInsights(services);
        ServiceExtensions.ConfigureDependencyInjection(services);
        ServiceExtensions.ConfigureAzureServiceBusClient(services);

        //services.AddAzureClients(builder =>
        //{
        //    builder.AddServiceBusClient(EnvironmentVariables.GetEnvironmentVariable(ServiceBus.AzureServiceBusConnection));
        //});
    })
    .Build();

host.Run();
