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
    private readonly IUsersRepository _usersRepository;
    private readonly IGroupMembersRepository _groupMembersRepository;

    public MembersController(
        IGroupsRepository groups,
        IUsersRepository usersRepository,
        IGroupMembersRepository groupMembersRepository)
    {
        _groups = groups;
        _usersRepository = usersRepository;
        _groupMembersRepository = groupMembersRepository;
    }

    [HttpGet]
    public IActionResult List(Guid groupId)
    {
        if (_groups.GetById(groupId) is null)
            return NotFound(new { error = "Group not found." });

        var members = _groupMembersRepository.ListWithUsersByGroupId(groupId);

        var result = members.Select(m => new MemberResponse(
            GroupId: m.GroupId,
            UserId: m.UserId,
            Email: m.Email,
            Name: m.Name,
            Role: m.Role,
            JoinedAt: m.JoinedAt
        )).ToList();

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

        var user = _usersRepository.GetByEmail(email);
        if (user is null)
        {
            user = new User(email, name);
            _usersRepository.Add(user);
        }

        if (_groupMembersRepository.Exists(groupId, user.Id))
            return Conflict(new { error = "User is already a member of this group." });

        var member = new GroupMember(groupId, user.Id, role);
        _groupMembersRepository.Add(member);

        return CreatedAtAction(nameof(List), new { groupId }, new CreateMemberResponse(user.Id));
    }
}
