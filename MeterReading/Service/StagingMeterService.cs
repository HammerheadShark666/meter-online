using Azure.Messaging.ServiceBus;
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

    //public async Task StageVodafoneMetersForReading()
    //{
    //    var database = mongoDbHelper.GetDatabase();

    //    var metersCollection = database.GetCollection<Meter>(MongoCollections.Meters);
    //    var filter = Builders<Meter>.Filter.Regex(m => m.TelecomsProvider, MongoBsonHelper.ExactMatch(Constants.TelecomsVodafone));
    //    var documents = await metersCollection.Find(filter).ToListAsync();

    //    if (documents.Count > 0)
    //    {
    //        foreach (var doc in documents)
    //        {
    //            var meterToRead = JsonSerializer.Serialize(doc);
    //            await azureServiceBusHelper.SendMessageAsync(ServiceBusQueues.StagingMeterVodafoneQueue, meterToRead);
    //        }
    //    }
    //    else
    //    {
    //        _logger.LogInformation($"No meters were found to be read for Vodafone.");

    //    }
    //}

    public async Task StageVodafoneMetersForReading()
    {
        try
        {

            //var database = mongoDbHelper.GetDatabase();
            //var metersCollection = database.GetCollection<BsonDocument>(MongoCollections.Meters);

            //var allDocs = await metersCollection.Find(_ => true).ToListAsync();

            //foreach (var doc in allDocs)
            //{
            //    if (doc.Contains("_id") && doc["_id"].BsonType == BsonType.String)
            //    {
            //        var idString = doc["_id"].AsString;

            //        if (ObjectId.TryParse(idString, out ObjectId objectId))
            //        {
            //            var filter = Builders<BsonDocument>.Filter.Eq("_id", idString);
            //            var update = Builders<BsonDocument>.Update.Set("_id", objectId);

            //            await metersCollection.UpdateOneAsync(filter, update);
            //        }
            //    }
            //}




















            //var database = mongoDbHelper.GetDatabase();
            //var metersCollection = database.GetCollection<Meter>(MongoCollections.Meters);

            //var first = await metersCollection.Find(_ => true).FirstOrDefaultAsync();
            //Console.WriteLine(first.Id.GetType());

            //Random rnd = new();

            //// Get all meters from the collection
            //var meters = await metersCollection.Find(_ => true).ToListAsync();

            //foreach (var meter in meters)
            //{
            //    var update = Builders<Meter>.Update
            //        .Set(m => m.lastReading, Math.Round(rnd.NextDouble() * 25000, 2))
            //        .Set(m => m.lastReadOn, DateTime.UtcNow.Date.AddDays(-1).AddHours(2));

            //    var result = await metersCollection.UpdateOneAsync(
            //        m => m.Id == meter.Id,
            //        update
            //    );

            //    Console.WriteLine($"Matched: {result.MatchedCount}, Modified: {result.ModifiedCount}");


            //}






            var database = mongoDbHelper.GetDatabase();
            var metersCollection = database.GetCollection<Meter>(MongoCollections.Meters);

            var filter = Builders<Meter>.Filter.Regex(
                m => m.TelecomsProvider,
                MongoBsonHelper.ExactMatch(Constants.TelecomsVodafone));

            var documents = await metersCollection.Find(filter).ToListAsync();

            if (!documents.Any())
            {
                _logger.LogInformation("No Vodafone meters found to stage.");
                return;
            }

            var metersToRead = new List<string>();
            int batchCount = 0;
            const int batchSize = 200;

            foreach (var doc in documents)
            {
                metersToRead.Add(JsonSerializer.Serialize(doc));
                batchCount++;

                if (batchCount == batchSize)
                {
                    await azureServiceBusHelper.SendMessagesAsync(ServiceBusQueues.StagingMeterVodafoneQueue, metersToRead);
                    batchCount = 0;
                    metersToRead.Clear();
                }
            }

            if (metersToRead.Any())
            {
                await azureServiceBusHelper.SendMessagesAsync(ServiceBusQueues.StagingMeterVodafoneQueue, metersToRead);
            }

            _logger.LogInformation("Successfully staged {Count} Vodafone meters", documents.Count);
        }
        catch (MongoException ex)
        {
            _logger.LogError(ex, "MongoDB failure while staging Vodafone meters");
            throw;
        }
        catch (ServiceBusException ex)
        {
            _logger.LogError(ex, "Service Bus failure while staging Vodafone meters");
            throw;
        }
    }
}