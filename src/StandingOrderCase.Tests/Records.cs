namespace StandingOrderCase.Tests;

record CreateStandingOrderResult(Guid Id);
record ValidationError(string PropertyName, string ErrorMessage);