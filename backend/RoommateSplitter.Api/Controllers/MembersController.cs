using Microsoft.AspNetCore.Mvc;
using RoommateSplitter.Api.Contracts.Members;
using RoommateSplitter.Domain.Groups;
using RoommateSplitter.Domain.Repositories;
using RoommateSplitter.Domain.Users;

namespace RoommateSplitter.Api.Controllers;

[ApiController]
[Route("api/groups/{groupId:guid}/members")]
public sealed class MembersController : ControllerBase
{
    private readonly IGroupsRepository _groups;
    private readonly IUsersRepository _users;
    private readonly IGroupMembersRepository _members;

    public MembersController(
        IGroupsRepository groups,
        IUsersRepository users,
        IGroupMembersRepository members)
    {
        _groups = groups;
        _users = users;
        _members = members;
    }

    [HttpGet]
    public IActionResult List(Guid groupId)
    {
        if (_groups.GetById(groupId) is null)
            return NotFound(new { error = "Group not found." });

        var memberships = _members.ListByGroupId(groupId);

        // For now, fetch user info per member (fine for small groups).
        // Later you can optimize with a join query in infrastructure.
        var result = memberships.Select(m =>
        {
            var user = _users.GetById(m.UserID);
            return new MemberResponse(
                GroupId: groupId,
                UserId: m.UserID,
                Email: user?.Email ?? "",
                Name: user?.Name ?? "",
                Role: m.Role.ToString(),
                JoinedAt: m.JoinedAt
            );
        }).ToList();

        return Ok(result);
    }

    [HttpPost]
    public IActionResult Create(Guid groupId, [FromBody] CreateMemberRequest request)
    {
        if (_groups.GetById(groupId) is null)
            return NotFound(new { error = "Group not found." });

        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { error = "Email is required." });

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Name is required." });

        var email = request.Email.Trim();
        var name = request.Name.Trim();

        var role = GroupRole.member;
        if (!string.IsNullOrWhiteSpace(request.Role) &&
            Enum.TryParse<GroupRole>(request.Role, ignoreCase: true, out var parsed))
        {
            role = parsed;
        }

        // Reuse existing user by email
        var user = _users.GetByEmail(email);
        if (user is null)
        {
            user = new User(email, name);
            _users.Add(user);
        }

        if (_members.Exists(groupId, user.Id))
            return Conflict(new { error = "User is already a member of this group." });

        var member = new GroupMember(groupId, user.Id, role);
        _members.Add(member);

        return CreatedAtAction(nameof(List), new { groupId }, new CreateMemberResponse(user.Id));
    }
}
