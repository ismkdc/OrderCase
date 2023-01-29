using StandingOrderCase.Api.Enums;

namespace StandingOrderCase.Api.Models;

public class NotificationLog : BaseEntity
{
    public Guid NotificationId { get; set; }
    public NotificationStatusEnum NotificationStatusEnum { get; set; }
}