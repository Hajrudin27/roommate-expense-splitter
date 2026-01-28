using Microsoft.AspNetCore.Mvc;
using RoomateSplitter.Api.Contracts.Groups;
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
       return CreatedAtAction(nameof(GetById), new { id = group.Id }, new CreateGroupResponse(group.Id));
    }

    // small helper endpoint
    [HttpGet]
    public IActionResult List()
    {
        var result = Groups.Select(g => new GroupResponse(g.Id, g.Name, g.Currency, g.CreatedAt));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
        {
      
            var group = Groups.SingleOrDefault(g => g.Id == id);
            if (group is null) return NotFound();

            return Ok(new GroupResponse(group.Id, group.Name, group.Currency, group.CreatedAt));
        }

}