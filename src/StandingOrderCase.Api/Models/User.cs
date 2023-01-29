namespace StandingOrderCase.Api.Models;

public class User : BaseEntity
{
    public required string Name { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Email { get; set; }
    public required string PushToken { get; set; }
}