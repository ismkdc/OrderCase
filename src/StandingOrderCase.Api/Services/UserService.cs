using Microsoft.EntityFrameworkCore;
using StandingOrderCase.Api.Models;
using StandingOrderCase.Api.Records;

namespace StandingOrderCase.Api.Services;

public class UserService
{
    private readonly StandingOrderCaseContext _context;

    public UserService(StandingOrderCaseContext context)
    {
        _context = context;
    }

    public Task<bool> Exists(Guid id)
    {
        return _context.Users.AnyAsync(u => u.Id == id);
    }

    public Task<GetContactInfo?> GetContactInfo(Guid id)
    {
        return _context.Users
            .Where(u => u.Id == id)
            .Select(u => new GetContactInfo(u.Email, u.PhoneNumber, u.PushToken))
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }
}