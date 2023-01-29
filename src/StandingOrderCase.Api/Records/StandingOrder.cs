using StandingOrderCase.Api.Enums;

namespace StandingOrderCase.Api.Records;

public record GetStandingOrder
(
    Guid Id,
    decimal Amount,
    OrderStatusEnum OrderStatusEnum,
    DateOnly ExecutionDate,
    DateTime CreatedAt
);

public record CreateStandingOrder
(
    Guid UserId,
    DateOnly ExecutionDate,
    IEnumerable<NotificationTypeEnum> Notifications,
    decimal Amount
);