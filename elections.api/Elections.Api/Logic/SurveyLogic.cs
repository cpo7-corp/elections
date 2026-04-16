using Elections.Api.DE;
using Elections.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Elections.Api.Logic;

public class SurveyLogic(IMongoDatabase database)
{
    private readonly IMongoCollection<SurveyDB> surveyCollection = database.GetCollection<SurveyDB>("surveys");

    private static readonly List<Party> parties = new()
    {
        new() { Id = 1, Order = 1, Name = "ליכוד", Leader = "בנימין נתניהו", ImageUrl = "/assets/images/likud.jpg" },
        new() { Id = 2, Order = 2, Name = "נפתלי בנט", Leader = "נפתלי בנט", ImageUrl = "/assets/images/בנט26.jpg" },
        new() { Id = 3, Order = 3, Name = "המחנה הממלכתי", Leader = "בני גנץ", ImageUrl = "/assets/images/המחנה-הממלכתי-copy.png" },
        new() { Id = 4, Order = 4, Name = "שס", Leader = "אריה דרעי", ImageUrl = "/assets/images/shas.jpg" },
        new() { Id = 5, Order = 5, Name = "יהדות התורה", Leader = "יצחק גולדקנופף", ImageUrl = "/assets/images/yahdut htora.png" },
        new() { Id = 6, Order = 6, Name = "ישראל ביתנו", Leader = "אביגדור ליברמן", ImageUrl = "/assets/images/beytenu.jpg" },
        new() { Id = 7, Order = 7, Name = "הדמוקרטים", Leader = "יאיר גולן", ImageUrl = "/assets/images/democrats.png" },
        new() { Id = 8, Order = 8, Name = "עוצמה יהודית", Leader = "איתמר בן גביר", ImageUrl = "/assets/images/otzma-yehudit.jpg" },
        new() { Id = 9, Order = 9, Name = "יש עתיד", Leader = "יאיר לפיד", ImageUrl = "/assets/images/yesh_atid.jpg" },
        new() { Id = 10, Order = 10, Name = "הרשימה המשותפת", Leader = "נציגים ערבים", ImageUrl = "/assets/images/mesutefet.png" },
        new() { Id = 11, Order = 11, Name = "הציונות הדתית", Leader = "בצלאל סמוטריץ", ImageUrl = "/assets/images/הציונות-הדתית.png" },
        new() { Id = 12, Order = 12, Name = "חופש כלכלי", Leader = "אביר קארה", ImageUrl = "/assets/images/abir kara.jfif" },
        new() { Id = 13, Order = 13, Name = "הכלכלית", Leader = "ירון זליכה", ImageUrl = "/assets/images/calcalit.png" },
        new() { Id = 14, Order = 14, Name = "החרדים העובדים", Leader = "נציג עובדים", ImageUrl = "/assets/images/chardi work.png" },
        new() { Id = 15, Order = 15, Name = "הצעירים", Leader = "נציג דור העתיד", ImageUrl = "/assets/images/young.jpg" },
        new() { Id = 16, Order = 16, Name = "המילואימניקים", Leader = "לוחמי המילואים", ImageUrl = "/assets/images/המילואימניקים.jpg" },
        new() { Id = 17, Order = 17, Name = "הקשישים", Leader = "נציג הגיל השלישי", ImageUrl = "/assets/images/old.jpg" }
    };

    public static List<Party> GetParties()
    {
        return parties;
    }

    public async Task<ApiResponse> SaveSurvey(SurveyReq req, string ip)
    {
        try
        {
            if (req.Votes.Sum(v => v.Mandates) != 120)
            {
                return ApiResponse.FromError("ERROR_INVALID_TOTAL_MANDATES");
            }

            if (req.Votes.Any(v => v.Mandates < 0 || v.Mandates > 120))
            {
                return ApiResponse.FromError("ERROR_MANDATE_OUT_OF_RANGE");
            }

            var partyIds = GetParties().Select(p => p.Id).ToList();
            if (req.Votes.Any(v => !partyIds.Contains(v.PartyId)))
            {
                return ApiResponse.FromError("ERROR_INVALID_PARTY_ID");
            }

            var userId = req.UserId ?? Guid.CreateVersion7();
            var votesDB = req.Votes.Select(v => new PartyVoteDB { PartyId = v.PartyId, Mandates = v.Mandates }).ToList();

            var filter = Builders<SurveyDB>.Filter.Eq(x => x.UserId, userId);
            var existing = await surveyCollection.Find(filter).FirstOrDefaultAsync();

            if (existing != null && (DateTime.UtcNow - existing.Created).TotalMinutes > 15)
            {
                return ApiResponse.FromError("ERROR_EDIT_TIME_EXCEEDED");
            }

            var update = Builders<SurveyDB>.Update
                .Set(x => x.Votes, votesDB)
                .Set(x => x.Ip, ip)
                .Set(x => x.Created, DateTime.UtcNow)
                .SetOnInsert(x => x.UserId, userId)
                .SetOnInsert(x => x.LikedBy, new List<Guid>())
                .SetOnInsert(x => x.DislikedBy, new List<Guid>());

            await surveyCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });

            return ApiResponse.FromSuccess(new { userId });
        }
        catch (Exception ex)
        {
            return ApiResponse.FromError(ex.Message);
        }
    }

    public async Task<List<SurveyResponse>> GetSurveys(int page, int pageSize)
    {
        return await surveyCollection.Find(_ => true)
            .SortByDescending(x => x.Created)
            .Skip(page * pageSize)
            .Limit(pageSize)
            .Project(x => new SurveyResponse
            {
                Id = x.Id.ToString(),
                UserId = x.UserId,
                Created = x.Created,
                LikesCount = x.LikedBy.Count,
                DislikesCount = x.DislikedBy.Count,
                Votes = x.Votes.Select(v => new PartyVote { PartyId = v.PartyId, Mandates = v.Mandates }).ToList()
            })
            .ToListAsync();
    }

    public async Task<SurveyResponse?> GetSurveyById(ObjectId id)
    {
        return await surveyCollection.Find(x => x.Id == id)
            .Project(x => new SurveyResponse
            {
                Id = x.Id.ToString(),
                UserId = x.UserId,
                Created = x.Created,
                LikesCount = x.LikedBy.Count,
                DislikesCount = x.DislikedBy.Count,
                Votes = x.Votes.Select(v => new PartyVote { PartyId = v.PartyId, Mandates = v.Mandates }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<PartyAggregate>> GetAggregatedResults()
    {
        // Simple average of all surveys
        // 1. Unwind votes
        // 2. Group by PartyId, average mandates
        // 3. Project to DTO

        var pipeline = new[]
        {
            new BsonDocument("$unwind", "$Votes"),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$Votes.PartyId" },
                { "AverageMandates", new BsonDocument("$avg", "$Votes.Mandates") }
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "PartyId", "$_id" },
                { "AverageMandates", 1 },
                { "_id", 0 }
            }),
            new BsonDocument("$sort", new BsonDocument("AverageMandates", -1))
        };

        var results = await surveyCollection.Aggregate<PartyAggregate>(pipeline).ToListAsync();
        return results;
    }

    public async Task<ApiResponse> ToggleReaction(ObjectId surveyId, Guid userId, bool isLike)
    {
        try
        {
            var survey = await surveyCollection.Find(x => x.Id == surveyId)
                .Project(x => new { IsLiked = x.LikedBy.Contains(userId), IsDisliked = x.DislikedBy.Contains(userId) })
                .FirstOrDefaultAsync();

            if (survey == null) return ApiResponse.FromError("ERROR_SURVEY_NOT_FOUND");

            var filter = Builders<SurveyDB>.Filter.Eq(x => x.Id, surveyId);
            UpdateDefinition<SurveyDB> update;

            if (isLike)
            {
                if (survey.IsLiked)
                {
                    update = Builders<SurveyDB>.Update.Pull(x => x.LikedBy, userId);
                }
                else
                {
                    update = Builders<SurveyDB>.Update.AddToSet(x => x.LikedBy, userId).Pull(x => x.DislikedBy, userId);
                }
            }
            else
            {
                if (survey.IsDisliked)
                {
                    update = Builders<SurveyDB>.Update.Pull(x => x.DislikedBy, userId);
                }
                else
                {
                    update = Builders<SurveyDB>.Update.AddToSet(x => x.DislikedBy, userId).Pull(x => x.LikedBy, userId);
                }
            }

            await surveyCollection.UpdateOneAsync(filter, update);
            return ApiResponse.FromSuccess();
        }
        catch (Exception ex)
        {
            return ApiResponse.FromError(ex.Message);
        }
    }
}
