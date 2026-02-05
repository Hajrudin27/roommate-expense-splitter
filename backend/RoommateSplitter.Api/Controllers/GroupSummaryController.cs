using Microsoft.AspNetCore.Mvc;
using RoommateSplitter.Api.Contracts.Balances;
using RoommateSplitter.Api.Contracts.Expenses;
using RoommateSplitter.Api.Contracts.Payments;
using RoommateSplitter.Api.Contracts.Summary;
using RoommateSplitter.Domain.Repositories;
using RoommateSplitter.Domain.Balances;
using RoommateSplitter.Api.Contracts.Groups;

namespace RoommateSplitter.Api.Controllers;

[ApiController]
[Route("api/groups/{groupId:guid}/summary")]
public sealed class GroupSummaryController : ControllerBase
{
    private readonly IGroupsRepository _groups;
    private readonly IExpensesRepository _expenses;
    private readonly IPaymentsRepository _payments;

    public GroupSummaryController(
        IGroupsRepository groups,
        IExpensesRepository expenses,
        IPaymentsRepository payments)
    {
        _groups = groups;
        _expenses = expenses;
        _payments = payments;
    }

    [HttpGet]
    public IActionResult Get(Guid groupId)
    {
        var group = _groups.GetById(groupId);
        if (group is null)
            return NotFound(new { error = "Group not found." });

        var expenses = _expenses.ListByGroupId(groupId);
        var payments = _payments.ListByGroupId(groupId);

        var calculator = new BalanceCalculator();
        var balanceResult = calculator.Calculate(expenses, payments);

        var balancesDto = new GetBalancesResponse(
            groupId,
            "DKK",
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
