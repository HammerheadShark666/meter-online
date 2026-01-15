using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using StageMetersForReading.Domain;

namespace StageMetersForReading;

public class StageVodaFoneMeters
{
    private readonly ILogger _logger;
    private readonly string _connectionString;

    public StageVodaFoneMeters(ILoggerFactory loggerFactory, IConfiguration config)
    {
        _logger = loggerFactory.CreateLogger<StageVodaFoneMeters>();

        _connectionString = config.GetValue<string>("MongoDbConnection")
                                ?? throw new InvalidOperationException("Connection string 'MongoDbConnection' is missing.");
    }

    [Function("StageVodaFoneMeters")]
    public async Task Run([TimerTrigger("0 6 17 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
        }


        var client = new MongoClient(_connectionString);

        var database = client.GetDatabase("meter-online-db");
        var meters = database.GetCollection<Meter>("meters");

        using var cursor = await meters.FindAsync(FilterDefinition<Meter>.Empty);

        while (await cursor.MoveNextAsync())
        {
            foreach (var meter in cursor.Current)
            {
                Console.WriteLine(meter.MeterNumber);
            }
        }

        //< AzureFunctionsVersion > v4 </ AzureFunctionsVersion >


        //foreach (var meter in meters)
        //{
        //    if (meter.TelecomProvider == "VodaFone")
        //    {
        //        _logger.LogInformation($"Staging meter with MeterNumber: {meter.MeterNumber}, ConnectionNumber: {meter.ConnectionNumber}");
        //        // Add your staging logic here
        //    }
        //}

    }
}
