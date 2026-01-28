using Microsoft.AspNetCore.Mvc;
using RoommateSplitter.Api.Contracts.Groups;
using RoommateSplitter.Domain.Groups;

namespace RoommateSplitter.Api.Controllers;

[ApiController]
[Route("api/groups")]
public class GroupsController : ControllerBase
{
    // Temporary in-memory store for demonstration purposes
    private static readonly List<Group> Groups = new();

    [HttpPost]
    public IActionResult Create([FromBody] CreateGroupRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Error = "Group name is required." });
        }
        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "DKK" : request.Currency.Trim().ToUpperInvariant();

        var group = new Group(request.Name.Trim(), currency);
        Groups.Add(group);

        // 201 Created plus location header
        return Created($"/api/groups/{group.Id}", new CreateGroupResponse(group.Id));
    }

    // small helper endpoint
    [HttpGet]
    public IActionResult List()
    {
        var result = Groups.Select(g => new
        {
            g.Id,
            g.Name,
            g.Currency,
            g.CreatedAt
        });
        return Ok(result);
    }
}