namespace StandingOrderCase.Api.Events;
public abstract record NotificationBase
{
    public Guid NotificationId { get; init; }
    public string? Message { get; init; }
    public string? ContactInfo { get; init; }
}

public record SendSmsNotification : NotificationBase;

public record SendEmailNotification : NotificationBase;

public record SendPushNotification : NotificationBase;