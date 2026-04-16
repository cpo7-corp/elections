using Elections.Api.Models;
using Elections.Api.Logic;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Elections.Api.Controllers;

[ApiController]
public class SurveyController(SurveyLogic surveyLogic, IHttpContextAccessor ctx) : ControllerBase
{
    [HttpGet("api/parties")]
    [ResponseCache(Duration = 3600)]
    public async Task<List<Party>> GetParties()
    {
        return await Task.FromResult(SurveyLogic.GetParties());
    }

    [HttpPost("api/surveys")]
    public async Task<ApiResponse> SaveSurvey([FromBody] SurveyReq req)
    {
        return await surveyLogic.SaveSurvey(req, GetIp());
    }

    [HttpGet("api/surveys")]
    public async Task<List<SurveyResponse>> GetSurveys([FromQuery] int page = 0, [FromQuery] int pageSize = 10)
    {
        return await surveyLogic.GetSurveys(page, pageSize);
    }

    [HttpGet("api/surveys/{id}")]
    public async Task<SurveyResponse?> GetSurvey(ObjectId id)
    {
        return await surveyLogic.GetSurveyById(id);
    }

    [HttpGet("api/surveys/aggregated")]
    public async Task<List<PartyAggregate>> GetAggregatedResults()
    {
        return await surveyLogic.GetAggregatedResults();
    }

    [HttpPost("api/surveys/{id}/like")]
    public async Task<ApiResponse> LikeSurvey(ObjectId id, [FromQuery] Guid userId)
    {
        return await surveyLogic.ToggleReaction(id, userId, true);
    }

    [HttpPost("api/surveys/{id}/dislike")]
    public async Task<ApiResponse> DislikeSurvey(ObjectId id, [FromQuery] Guid userId)
    {
        return await surveyLogic.ToggleReaction(id, userId, false);
    }

    private string GetIp()
    {
        var httpContext = ctx.HttpContext;
        if (httpContext == null) return "unknown";
        var cfIp = httpContext.Request.Headers["cf-connecting-ip"].FirstOrDefault();
        if (!string.IsNullOrEmpty(cfIp)) return cfIp.Trim();
        return httpContext.Connection.RemoteIpAddress?.ToString()?.Trim() ?? "unknown";
    }
}
