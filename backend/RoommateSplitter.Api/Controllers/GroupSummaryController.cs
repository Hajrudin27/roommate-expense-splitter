using Microsoft.AspNetCore.Mvc;
using RoommateSplitter.Api.Contracts.Balances;
using RoommateSplitter.Api.Contracts.Expenses;
using RoommateSplitter.Api.Contracts.Groups;
using RoommateSplitter.Api.Contracts.Members;
using RoommateSplitter.Api.Contracts.Payments;
using RoommateSplitter.Api.Contracts.Summary;
using RoommateSplitter.Domain.Balances;
using RoommateSplitter.Domain.Repositories;

namespace RoommateSplitter.Api.Controllers;

[ApiController]
[Route("api/groups/{groupId:guid}/summary")]
public sealed class GroupSummaryController : ControllerBase
{
    private readonly IGroupsRepository _groups;
    private readonly IExpensesRepository _expenses;
    private readonly IPaymentsRepository _payments;
    private readonly IGroupMembersRepository _groupMembersRepository;
    private readonly IUsersRepository _usersRepository;

    public GroupSummaryController(
        IGroupsRepository groups,
        IExpensesRepository expenses,
        IPaymentsRepository payments,
        IGroupMembersRepository groupMembersRepository,
        IUsersRepository usersRepository)
    {
        _groups = groups;
        _expenses = expenses;
        _payments = payments;
        _groupMembersRepository = groupMembersRepository;
        _usersRepository = usersRepository;
    }

    [HttpGet]
    public IActionResult Get(Guid groupId)
    {
        var group = _groups.GetById(groupId);
        if (group is null)
            return NotFound(new { error = "Group not found." });

        var membersDomain = _groupMembersRepository.ListByGroupId(groupId);
        var membersDto = membersDomain.Select(m =>
        {
            var u = _usersRepository.GetById(m.UserID);

            return new MemberResponse(
                GroupId: m.GroupID,
                UserId: m.UserID,
                Email: u?.Email ?? "",
                Name: u?.Name ?? "",
                Role: m.Role.ToString(),
                JoinedAt: m.JoinedAt
            );
        }).ToList();

        var expenses = _expenses.ListByGroupId(groupId);
        var payments = _payments.ListByGroupId(groupId);

        var calculator = new BalanceCalculator();
        var balanceResult = calculator.Calculate(expenses, payments);

        var balancesDto = new GetBalancesResponse(
            groupId,
            group.Currency,
            balanceResult.NetBalances
                .Where(x => Math.Abs(x.Value) >= 0.01m)
                .Select(x => new UserBalanceDto(x.Key, x.Value))
                .OrderByDescending(x => x.Amount)
                .ToList(),
            balanceResult.Transfers
                .Select(t => new TransferDto(t.FromUserId, t.ToUserId, t.Amount))
                .ToList()
        );

        var response = new GroupSummaryResponse(
            Group: new GroupResponse(
                group.Id,
                group.Name,
                group.Currency,
                group.CreatedAt
            ),
            Members: membersDto,
            Expenses: expenses.Select(e => new ExpenseResponse(
                e.Id,
                e.GroupId,
                e.Description,
                e.Amount,
                e.PaidByUserId,
                e.ExpenseDate,
                e.CreatedAt,
                e.Shares.Select(s => new ExpenseShareResponse(s.UserId, s.Amount)).ToList()
            )).ToList(),
            Payments: payments.Select(p => new PaymentResponse(
                p.Id,
                p.GroupId,
                p.FromUserId,
                p.ToUserId,
                p.Amount,
                p.PaymentDate,
                p.CreatedAt
            )).ToList(),
            Balances: balancesDto
        );

        return Ok(response);
    }
}
