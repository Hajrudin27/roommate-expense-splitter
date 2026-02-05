using Microsoft.AspNetCore.Mvc;
using RoommateSplitter.Api.Contracts.Groups;
using RoommateSplitter.Domain.Repositories;
using RoommateSplitter.Domain.Groups;

namespace RoommateSplitter.Api.Controllers;

[ApiController]
[Route("api/groups")]
public class GroupsController : ControllerBase
{
    private readonly IGroupsRepository _groups;

    public GroupsController(IGroupsRepository groups)
    {
        _groups = groups;
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateGroupRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Group name is required." });

        var currency = string.IsNullOrWhiteSpace(request.Currency)
            ? "DKK"
            : request.Currency.Trim().ToUpperInvariant();

        var group = new Group(request.Name.Trim(), currency);
        _groups.Add(group);

        return CreatedAtAction(nameof(GetById), new { id = group.Id }, new CreateGroupResponse(group.Id));
    }

    [HttpGet]
    public IActionResult List()
    {
        var result = _groups.List()
            .Select(g => new GroupResponse(g.Id, g.Name, g.Currency, g.CreatedAt));

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var group = _groups.GetById(id);
        if (group is null) return NotFound();

        return Ok(new GroupResponse(group.Id, group.Name, group.Currency, group.CreatedAt));
    }
}
