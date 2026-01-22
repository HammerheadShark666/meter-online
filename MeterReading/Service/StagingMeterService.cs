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

    public async Task StageVodafoneMetersForReading()
    {
        try
        {
            //var database = mongoDbHelper.GetDatabase();
            //var metersCollection = database.GetCollection<Meter>(MongoCollections.Meters);

            ////var filter = Builders<Meter>.Filter.Regex(
            ////    m => m.TelecomsProvider,
            ////    MongoBsonHelper.ExactMatch(Constants.TelecomsVodafone));


            //var builder = Builders<Meter>.Filter;

            //var filter =
            //    builder.Regex(m => m.TelecomsProvider, MongoBsonHelper.ExactMatch(Constants.TelecomsVodafone)) &
            //    builder.Eq(m => m.MeterType, MeterType.Solar);

            //var documents = await metersCollection.Find(filter).ToListAsync();

            var meters = await GetMeters();

            if (meters.Count == 0)
            {
                _logger.LogInformation("No Vodafone meters found to stage.");
                return;
            }

            var metersToRead = new List<string>();
            int batchCount = 0;
            const int batchSize = 200;

            foreach (var meter in meters)
            {
                metersToRead.Add(JsonSerializer.Serialize(meter));
                batchCount++;

                if (batchCount == batchSize)
                {
                    await azureServiceBusHelper.SendMessagesAsync(ServiceBusQueues.StagingMeterVodafoneQueue, metersToRead);
                    batchCount = 0;
                    metersToRead.Clear();
                }
            }

            if (metersToRead.Count > 0)
            {
                await azureServiceBusHelper.SendMessagesAsync(ServiceBusQueues.StagingMeterVodafoneQueue, metersToRead);
            }

            _logger.LogInformation("Successfully staged {Count} Vodafone meters", meters.Count);
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

    private async Task<List<Meter>> GetMeters()
    {
        var database = mongoDbHelper.GetDatabase();
        var metersCollection = database.GetCollection<Meter>(MongoCollections.Meters);

        //var filter = Builders<Meter>.Filter.Regex(
        //    m => m.TelecomsProvider,
        //    MongoBsonHelper.ExactMatch(Constants.TelecomsVodafone));


        var builder = Builders<Meter>.Filter;

        var filter =
            builder.Regex(m => m.TelecomsProvider, MongoBsonHelper.ExactMatch(Constants.TelecomsVodafone)) &
            builder.Eq(m => m.MeterType, MeterType.Solar);

        var meters = await metersCollection.Find(filter).ToListAsync();

        return meters;
    }
}