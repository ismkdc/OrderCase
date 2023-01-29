using StandingOrderCase.Api.Enums;

namespace StandingOrderCase.Api.Models;

public class Notification : BaseEntity
{
    public Guid StandingOrderId { get; set; }
    public required string Message { get; set; }
    public required string ContactInfo { get; set; }
    public NotificationTypeEnum NotificationTypeEnum { get; set; }
    public NotificationStatusEnum NotificationStatusEnum { get; set; }
}