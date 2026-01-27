using Azure.Messaging.ServiceBus;
using MeterReading.Domain;
using MeterReading.Helper;
using MeterReading.Helper.Exceptions;
using MeterReading.Helper.Interfaces;
using MeterReading.Service.Interface;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Text.Json;

namespace MeterReading.Service;
public class UpdateMeterReadingService(ILogger<UpdateMeterReadingService> logger, IMongoDbHelper mongoDbHelper) : IUpdateMeterReadingService
{
    public async Task UpdateMeterReading(ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        using var scope = logger.BeginScope("MessageId: {MessageId}", message.MessageId);

        MeterReadingToSave meterReading;

        try
        {
            meterReading = Deserialize(message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize meter reading message");

            await messageActions.DeadLetterMessageAsync(
                message,
                deadLetterReason: "Failed to deserialize meter reading message",
                deadLetterErrorDescription: ex.Message);

            return;
        }

        logger.LogInformation("Processing meter reading {@MeterReading}", meterReading);

        if (!await UpdateMeterReadingInDatabase(meterReading))
        {
            logger.LogWarning(
                "Meter not found or reading is not newer. Id: {Id}, MeterNumber: {MeterNumber}",
                meterReading.Id,
                meterReading.MeterNumber);

            await messageActions.DeadLetterMessageAsync(
                message,
                deadLetterReason: "Meter Not Found or Stale Reading",
                deadLetterErrorDescription: "No matching meter found or reading was older than existing value");
        }

        logger.LogInformation("Meter reading updated successfully");

        await messageActions.CompleteMessageAsync(message);
    }

    private async Task<bool> UpdateMeterReadingInDatabase(MeterReadingToSave meterReading)
    {
        var database = mongoDbHelper.GetDatabase();
        var metersCollection = database.GetCollection<Meter>(MongoCollections.Meters);

        var filter = Builders<Meter>.Filter.And(
            Builders<Meter>.Filter.Eq(m => m.Id, meterReading.Id),
            Builders<Meter>.Filter.Eq(m => m.MeterNumber, meterReading.MeterNumber),
            Builders<Meter>.Filter.Lt(m => m.LastReadOn, meterReading.LastReadOn)
        );

        var update = Builders<Meter>.Update.Combine(
            Builders<Meter>.Update.Set(m => m.LastReading, meterReading.LastReading),
            Builders<Meter>.Update.Set(m => m.LastReadOn, meterReading.LastReadOn)
        );

        var result = await metersCollection.UpdateOneAsync(filter, update);

        if (result.MatchedCount == 0)
            return false;

        return true;
    }

    private static MeterReadingToSave Deserialize(ServiceBusReceivedMessage message)
    {
        if (message.Body == null)
            throw new MeterNotDeserialisedException("Message body was null");

        return JsonSerializer.Deserialize<MeterReadingToSave>(
            message.Body,
            JsonHelper.JsonOptions
        ) ?? throw new MeterNotDeserialisedException(
            "Meter payload was null after deserialization");
    }
}