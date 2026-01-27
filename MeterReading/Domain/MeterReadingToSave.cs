using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MeterReading.Domain;
public class MeterReadingToSave
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    [BsonElement("meterNumber")]
    public required string MeterNumber { get; set; }

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string UserId { get; set; }

    [BsonElement("lastReading")]
    public decimal LastReading { get; set; }

    [BsonElement("lastReadOn")]
    public DateTime LastReadOn { get; set; }
}