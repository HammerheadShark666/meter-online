using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StageMetersForReading.Domain;

public class Meter
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("meterNumber")]
    public string MeterNumber { get; set; }

    [BsonElement("telecomProvider")]
    public string TelecomProvider { get; set; }

    [BsonElement("meterType")]
    public string MeterType { get; set; }

    [BsonElement("connectionNumber")]
    public string ConnectionNumber { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; }
}
