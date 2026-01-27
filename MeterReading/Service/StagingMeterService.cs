using Azure.Messaging.ServiceBus;
using MeterReading.Domain;
using MeterReading.Helper;
using MeterReading.Helper.Interfaces;
using MeterReading.Service.Interface;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Text.Json;

namespace MeterReading.Service;

internal sealed class StagingMeterService(ILogger<StagingMeterService> logger, IAzureServiceBusHelper azureServiceBusHelper, IMongoDbHelper mongoDbHelper) : IStagingMeterService
{
    public async Task StageVodafoneMetersForReading()
    {
        try
        {
            var meters = await GetMeters();
            if (meters.Count == 0)
            {
                logger.LogInformation("No Vodafone meters found to stage.");
                return;
            }

            foreach (var batch in meters.Chunk(AzureBusService.BatchMaxMessageCount))
            {
                var payloads = batch
                    .Select(m => JsonSerializer.Serialize(m))
                    .ToList();

                await azureServiceBusHelper.SendMessagesInBatchAsync(
                    ServiceBusQueues.StagingMeterVodafoneQueue,
                    payloads);
            }

            logger.LogInformation("Successfully staged {Count} Vodafone meters", meters.Count);
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "MongoDB failure while staging Vodafone meters");
            throw;
        }
        catch (ServiceBusException ex)
        {
            logger.LogError(ex, "Service Bus failure while staging Vodafone meters");
            throw;
        }
    }

    private async Task<List<Meter>> GetMeters()
    {
        var database = mongoDbHelper.GetDatabase();
        var metersCollection = database.GetCollection<Meter>(MongoCollections.Meters);

        var filter = Builders<Meter>.Filter.And(
            Builders<Meter>.Filter.Regex(m => m.TelecomsProvider, MongoBsonHelper.ExactMatch(Constants.TelecomsVodafone)),
            Builders<Meter>.Filter.Eq(m => m.MeterType, MeterType.Solar));

        return await metersCollection.Find(filter).Limit(10).ToListAsync();
    }
}