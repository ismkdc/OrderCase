using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StandingOrderCase.Api.Enums;
using StandingOrderCase.Api.Models;

namespace StandingOrderCase.Api.HostedServices;

public class OutboxHostedService : IHostedService, IDisposable
{
    private readonly ILogger<OutboxHostedService> _logger;
    private readonly StandingOrderCaseContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    private Timer? _timer = null;

    public OutboxHostedService(ILogger<OutboxHostedService> logger, StandingOrderCaseContext context,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxHostedService running.");

        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        _logger.LogInformation("OutboxHostedService is working.");

        var outboxMessages = _context
            .OutboxMessages
            .Where(x => x.OutboxStatusEnum == OutboxStatusEnum.Pending)
            .ToArray();

        foreach (var outboxMessage in outboxMessages)
        {
            var data = JsonSerializer.Deserialize
            (
                outboxMessage.Data,
                Type.GetType(outboxMessage.Type) ?? throw new InvalidOperationException()
            );

            if (data != null)
            {
                _publishEndpoint.Publish(data);
            }

            outboxMessage.OutboxStatusEnum = OutboxStatusEnum.Completed;
        }

        _context.SaveChanges();
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxHostedService is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}