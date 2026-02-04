namespace RoommateSplitter.Api.Contracts.Payments;

public sealed record CreatePaymentRequest(
    Guid FromUserId,
    Guid ToUserId,
    decimal Amount,
    DateOnly PaymentDate
);
