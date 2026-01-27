using Azure.Messaging.ServiceBus;
using MeterReading.Domain;
using MeterReading.Helper;
using MeterReading.Helper.Interfaces;
using MeterReading.Service.Interface;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Text.Json;

namespace MeterReading.Service;

internal sealed class StagingMeterService(
    IAzureServiceBusHelper azureServiceBusHelper,
    IMongoDbHelper mongoDbHelper,
    ILoggerFactory loggerFactory) : IStagingMeterService
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<StagingMeterService>();

    public async Task StageVodafoneMetersForReading()
    {
        try
        {
            var meters = await GetMeters();

            if (!meters.Any())
            {
                _logger.LogInformation("No Vodafone meters found to stage.");
                return;
            }

            const int batchSize = 200;

            foreach (var batch in meters.Chunk(batchSize))
            {
                var payloads = batch
                    .Select(m => JsonSerializer.Serialize(m))
                    .ToList();

                await azureServiceBusHelper.SendMessagesInBatchAsync(
                    ServiceBusQueues.StagingMeterVodafoneQueue,
                    payloads);
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

        var filter = Builders<Meter>.Filter.And(
            Builders<Meter>.Filter.Regex(m => m.TelecomsProvider, MongoBsonHelper.ExactMatch(Constants.TelecomsVodafone)),
            Builders<Meter>.Filter.Eq(m => m.MeterType, MeterType.Solar));

        return await metersCollection.Find(filter).ToListAsync();
    }
}