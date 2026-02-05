using Microsoft.AspNetCore.Mvc;
using RoommateSplitter.Api.Contracts.Balances;
using RoommateSplitter.Domain.Repositories;
using RoommateSplitter.Domain.Balances;

namespace RoommateSplitter.Api.Controllers;

[ApiController]
[Route("api/groups/{groupId:guid}/balances")]
public sealed class BalancesController : ControllerBase
{
    private readonly IGroupsRepository _groupsRepository;
    private readonly IExpensesRepository _expensesRepository;

    private readonly IPaymentsRepository _payments;

    public BalancesController(
        IGroupsRepository groupsRepository,
        IExpensesRepository expensesRepository,
        IPaymentsRepository payments)
    {
        _groupsRepository = groupsRepository;
        _expensesRepository = expensesRepository;
        _payments = payments;
    }

    [HttpGet]
    public IActionResult GetBalances([FromRoute] Guid groupId)
    {
        var group = _groupsRepository.GetById(groupId);
        if (group is null)
            return NotFound();

        var expenses = _expensesRepository.ListByGroupId(groupId);
        var payments = _payments.ListByGroupId(groupId);

        var calculator = new BalanceCalculator();
        var result = calculator.Calculate(expenses, payments);

        var response = new GetBalancesResponse(
            GroupId: groupId,
            Currency: "DKK",
            NetBalances: result.NetBalances
                .Select(kvp => new UserBalanceDto(kvp.Key, kvp.Value))
                .Where(x => x.Amount != 0m)
                .OrderByDescending(x => x.Amount)
                .ToList(),
            Transfers: result.Transfers
                .Select(t => new TransferDto(t.FromUserId, t.ToUserId, t.Amount))
                .ToList()
        );

        return Ok(response);
    }
}
