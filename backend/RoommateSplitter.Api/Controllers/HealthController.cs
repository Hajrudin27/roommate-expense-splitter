using Microsoft.AspNetCore.Mvc;
using RoommateSplitter.Domain.Repositories;


namespace RoomateSplitter.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "ok" });
    }
}
