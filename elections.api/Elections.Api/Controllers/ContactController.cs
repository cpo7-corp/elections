using Elections.Api.Logic;
using Elections.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Elections.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController(UserLogic userLogic, IHttpContextAccessor ctx) : ControllerBase
{
    [HttpPost]
    public async Task<ApiResponse> Submit([FromBody] ContactReq req)
    {
        return await userLogic.Contact(req, GetIp());
    }

    private string GetIp()
    {
        var httpContext = ctx.HttpContext;
        if (httpContext == null) return "unknown";

        var cfIp = httpContext.Request.Headers["cf-connecting-ip"].FirstOrDefault();
        if (!string.IsNullOrEmpty(cfIp))
        {
            return cfIp.Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString()?.Trim() ?? "unknown";
    }
}
