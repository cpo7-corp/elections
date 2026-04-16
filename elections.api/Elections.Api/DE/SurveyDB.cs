using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Elections.Api.DE;

public class SurveyDB
{
    [BsonId]
    public ObjectId Id { get; set; }

    public Guid UserId { get; set; }

    public required List<PartyVoteDB> Votes { get; set; }

    public List<Guid> LikedBy { get; set; } = new();
    public List<Guid> DislikedBy { get; set; } = new();

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public string? Ip { get; set; }
}

public class PartyVoteDB
{
    public required int PartyId { get; set; }
    public required int Mandates { get; set; }
}
