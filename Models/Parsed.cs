using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace vki_schedule_telegram.Models;

public class Parsed
{
    [BsonId] 
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string? Name { get; set; }
    public List<Pdf>? Data { get; set; }
}