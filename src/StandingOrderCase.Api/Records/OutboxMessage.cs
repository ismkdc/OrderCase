namespace StandingOrderCase.Api.Records;

public record AddOutboxMessage
(
    string ExchangeName,
    object Data
);