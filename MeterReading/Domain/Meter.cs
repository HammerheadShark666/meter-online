using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MeterReading.Domain;

public class Meter
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    [BsonElement("meterNumber")]
    public required string MeterNumber { get; set; }

    [BsonElement("telecomsProvider")]
    public required string TelecomsProvider { get; set; }

    [BsonElement("meterType")]
    public required string MeterType { get; set; }

    [BsonElement("connectionNumber")]
    public required string ConnectionNumber { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string UserId { get; set; }

    [BsonElement("lastReading")]
    public decimal LastReading { get; set; }

    [BsonElement("lastReadOn")]
    public DateTime LastReadOn { get; set; }
}