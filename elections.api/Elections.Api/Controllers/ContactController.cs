using Elections.Api.Logic;
using Elections.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Elections.Api.Core;

namespace Elections.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController(UserLogic userLogic) : ControllerBase
{
    [HttpPost]
    public async Task<ApiResponse> Submit([FromBody] ContactReq req)
    {
        return await userLogic.Contact(req, HttpContext.GetClientIp());
    }

}
