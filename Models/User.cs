using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace vki_schedule_telegram.Models;
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public long? TgId { get; set; }
    public string? Name { get; set; }
    public string? Group { get; set; }
}