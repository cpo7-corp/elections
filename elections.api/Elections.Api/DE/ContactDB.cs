using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Elections.Api.DE;

public class ContactDB
{
    [BsonId]
    public ObjectId Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Phone { get; set; }
    public required string Email { get; set; }
    public required string Message { get; set; }
    public string? Ip { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
}
