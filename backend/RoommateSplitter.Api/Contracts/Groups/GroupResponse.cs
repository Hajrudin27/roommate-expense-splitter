namespace RoomateSplitter.Api.Contracts.Groups;

public record GroupResponse(Guid Id, string name, string Currency, DateTime CreatedAt);