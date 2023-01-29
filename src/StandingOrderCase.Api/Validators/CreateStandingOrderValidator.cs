using FluentValidation;
using StandingOrderCase.Api.Records;
using StandingOrderCase.Api.Services;

namespace StandingOrderCase.Api.Validators;

public class CreateStandingOrderValidator : AbstractValidator<CreateStandingOrder>
{
    public CreateStandingOrderValidator(StandingOrderService standingOrderService, UserService userService)
    {
        RuleFor(x => x.ExecutionDate).Must(x => x > DateOnly.FromDateTime(DateTime.Today));
        RuleFor(x => x.ExecutionDate.Day).InclusiveBetween(1, 28);
        RuleFor(x => x.Amount).InclusiveBetween(100, 20_000);

        RuleFor(x => x).MustAsync(async (x, _) =>
        {
            var exists = await userService.Exists(x.UserId);
            return exists;
        }).WithMessage("User does not exist");

        RuleFor(x => x).MustAsync(async (x, _) =>
        {
            var exists = await standingOrderService.Exists(x.UserId);
            return !exists;
        }).WithMessage("User already has a standing order");
    }
}