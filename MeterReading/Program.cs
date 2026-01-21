using MeterReading.Helper.Extensions;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        ServiceExtensions.ConfigureApplicationInsightsTelemetryWorkerService(services);
        ServiceExtensions.ConfigureFunctionsApplicationInsights(services);
        ServiceExtensions.ConfigureDependencyInjection(services);
        ServiceExtensions.ConfigureAzureServiceBusClient(services);
    })
    .Build();

host.Run();