using MeterReading.Domain;
using MeterReading.Function;
using MeterReading.Function.Helpers.Interfaces;
using MeterReading.Helper;
using MeterReading.Helper.Interfaces;
using MeterReading.Service.Interface;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Text.Json;

namespace MeterReading.Service;
internal sealed class StagingMeterService(IAzureServiceBusHelper azureServiceBusHelper, IMongoDbHelper mongoDbHelper, ILoggerFactory loggerFactory) : IStagingMeterService
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<StageVodafoneMeters>();

    public async Task StageVodafoneMetersForReading()
    {
        var database = mongoDbHelper.GetDatabase();

        var metersCollection = database.GetCollection<Meter>(MongoCollections.Meters);
        var filter = Builders<Meter>.Filter.Regex(m => m.TelecomsProvider, MongoBsonHelper.ExactMatch(Constants.TelecomsVodafone));
        var documents = await metersCollection.Find(filter).ToListAsync();

        if (documents.Count > 0)
        {
            foreach (var doc in documents)
            {
                var meterToRead = JsonSerializer.Serialize(doc);
                await azureServiceBusHelper.SendMessageAsync(ServiceBusQueues.StagingMeterVodafoneQueue, meterToRead);
            }
        }
        else
        {
            _logger.LogInformation($"No documents found for Vodafone.");

        }
    }
}