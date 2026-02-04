namespace RoommateSplitter.Api.Contracts.Payments;

public sealed record PaymentResponse(
    Guid Id,
    Guid GroupId,
    Guid FromUserId,
    Guid ToUserId,
    decimal Amount,
    DateOnly PaymentDate,
    DateTime CreatedAt
);
