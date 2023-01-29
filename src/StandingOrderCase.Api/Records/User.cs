namespace StandingOrderCase.Api.Records;

public record GetContactInfo
(
    string Email,
    string PhoneNumber,
    string PushToken
);