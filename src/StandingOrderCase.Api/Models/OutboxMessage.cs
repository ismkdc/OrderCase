using StandingOrderCase.Api.Enums;

namespace StandingOrderCase.Api.Models;

public class OutboxMessage : BaseEntity
{
    public required string Type { get; set; }
    public required string Data { get; set; }
    public OutboxStatusEnum OutboxStatusEnum { get; set; }
}