using StandingOrderCase.Api.Enums;

namespace StandingOrderCase.Api.Models;

public class StandingOrder : BaseEntity
{
    public Guid UserId { get; set; }
    public DateOnly ExecutionDate { get; set; }
    public decimal Amount { get; set; }
    public OrderStatusEnum OrderStatusEnum { get; set; }
}