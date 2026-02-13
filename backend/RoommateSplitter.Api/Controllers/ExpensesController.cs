using Microsoft.AspNetCore.Mvc;
using RoommateSplitter.Api.Contracts.Expenses;
using RoommateSplitter.Domain.Expenses;
using RoommateSplitter.Domain.Repositories;

namespace RoommateSplitter.Api.Controllers;

[ApiController]
[Route("api/groups/{groupId:guid}/expenses")]
public sealed class ExpensesController : ControllerBase
{
    private readonly IGroupsRepository _groups;
    private readonly IExpensesRepository _expenses;

    public ExpensesController(IGroupsRepository groups, IExpensesRepository expenses)
    {
        _groups = groups;
        _expenses = expenses;
    }

    [HttpPost]
    public IActionResult Create(Guid groupId, [FromBody] CreateExpenseRequest request)
    {
        // 1) group must exist
        if (_groups.GetById(groupId) is null)
            return NotFound(new { error = "Group not found." });

        // 2) basic validation
        if (string.IsNullOrWhiteSpace(request.Description))
            return BadRequest(new { error = "Description is required." });

        if (request.Amount <= 0)
            return BadRequest(new { error = "Amount must be > 0." });

        if (request.PaidByUserId == Guid.Empty)
            return BadRequest(new { error = "PaidByUserId is required." });

        if (request.ParticipantUserIds is null || request.ParticipantUserIds.Count == 0)
            return BadRequest(new { error = "At least one participant is required." });

        var shares = ExpenseSplit.Equal(request.Amount, request.ParticipantUserIds);

        Expense expense;
        try
        {
            expense = new Expense(
                groupId,
                request.Description.Trim(),
                request.Amount,
                request.PaidByUserId,
                request.ExpenseDate, // ✅ DateOnly stays DateOnly
                shares
            );
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        _expenses.Add(expense);

        return CreatedAtAction(nameof(List), new { groupId }, new CreateExpenseResponse(expense.Id));
    }

    [HttpGet]
    public IActionResult List(Guid groupId)
    {
        if (_groups.GetById(groupId) is null)
            return NotFound(new { error = "Group not found." });

        var expenses = _expenses.ListByGroupId(groupId);

        var result = expenses.Select(e => new ExpenseResponse(
            e.Id,
            e.GroupId,
            e.Description,
            e.Amount,
            e.PaidByUserId,
            e.ExpenseDate, // ✅ DateOnly
            e.CreatedAt,   // likely DateTime
            e.Shares.Select(s => new ExpenseShareResponse(s.UserId, s.Amount)).ToList()
        ));

        return Ok(result);
    }
}
