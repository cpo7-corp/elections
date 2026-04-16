namespace Elections.Api.Models;

public class SurveyResponse
{
    public string? Id { get; set; }
    public Guid UserId { get; set; }
    public List<PartyVote>? Votes { get; set; }
    public int LikesCount { get; set; }
    public int DislikesCount { get; set; }
    public DateTime Created { get; set; }
}

public class SurveyReq
{
    public Guid? UserId { get; set; }
    public required List<PartyVote> Votes { get; set; }
}

public class PartyVote
{
    public required int PartyId { get; set; }
    public required int Mandates { get; set; }
}
