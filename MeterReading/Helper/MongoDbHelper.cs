using MeterReading.Helper.Interfaces;
using MongoDB.Driver;

namespace MeterReading.Helper;

public class MongoDbHelper() : IMongoDbHelper
{
    private readonly string _connectionString = EnvironmentVariables.GetEnvironmentVariable(MongoSettings.ConnectionString);

    public IMongoDatabase GetDatabase()
    {
        var client = new MongoClient(_connectionString);
        return client.GetDatabase(MongoSettings.Database);
    }
}