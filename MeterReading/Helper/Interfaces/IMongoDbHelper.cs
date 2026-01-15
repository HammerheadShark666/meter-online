using MongoDB.Driver;

namespace MeterReading.Helper.Interfaces;
public interface IMongoDbHelper
{
    IMongoDatabase GetDatabase();
}