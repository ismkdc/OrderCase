using System.Text.Json;
using StandingOrderCase.Api.Models;
using StandingOrderCase.Api.Records;

namespace StandingOrderCase.Api.Services;

public class OutboxService
{
    private readonly StandingOrderCaseContext _context;

    public OutboxService(StandingOrderCaseContext context)
    {
        _context = context;
    }

    public void AddMessage(AddOutboxMessage message)
    {
        var outboxMessage = new OutboxMessage
        {
            Type = $"{message.Data.GetType().FullName}, {message.Data.GetType().Assembly.GetName().Name}",
            Data = JsonSerializer.Serialize(message.Data)
        };

        _context.OutboxMessages.Add(outboxMessage);
    }
}