using Microsoft.AspNetCore.Mvc;
using RoommateSplitter.Api.Contracts.Payments;
using RoommateSplitter.Domain.Repositories;
using RoommateSplitter.Domain.Payments;

namespace RoommateSplitter.Api.Controllers;

[ApiController]
[Route("api/groups/{groupId:guid}/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IGroupsRepository _groups;
    private readonly IPaymentsRepository _payments;

    public PaymentsController(
        IGroupsRepository groups,
        IPaymentsRepository payments)
    {
        _groups = groups;
        _payments = payments;
    }

    [HttpPost]
    public IActionResult Create(Guid groupId, [FromBody] CreatePaymentRequest request)
    {
        if (_groups.GetById(groupId) is null)
            return NotFound(new { error = "Group not found." });

        if (request.Amount <= 0m)
            return BadRequest(new { error = "Amount must be > 0." });

        Payment payment;
        try
        {
            payment = new Payment(
                groupId,
                request.FromUserId,
                request.ToUserId,
                request.Amount,
                request.PaymentDate
            );
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        _payments.Add(payment);

        return CreatedAtAction(
            nameof(List),
            new { groupId },
            new CreatePaymentResponse(payment.Id)
        );
    }

    [HttpGet]
    public IActionResult List(Guid groupId)
    {
        if (_groups.GetById(groupId) is null)
            return NotFound(new { error = "Group not found." });

        var payments = _payments.ListByGroupId(groupId);

        var result = payments.Select(p => new PaymentResponse(
            p.Id,
            p.GroupId,
            p.FromUserId,
            p.ToUserId,
            p.Amount,
            p.PaymentDate,
            p.CreatedAt
        ));

        return Ok(result);
    }
}
