using StandingOrderCase.Api.Enums;

namespace StandingOrderCase.Api.Records;

public record CreateNotification
(
    Guid UserId,
    Guid SourceEntityId,
    NotificationTypeEnum NotificationTypeEnum,
    string Message
);

public record GetNotification
(
    Guid Id,
    NotificationStatusEnum NotificationStatusEnum,
    Guid StandingOrderId,
    NotificationTypeEnum NotificationTypeEnum,
    string Message
);