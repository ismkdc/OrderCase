using MassTransit;
using Microsoft.EntityFrameworkCore;
using StandingOrderCase.Api.Enums;
using StandingOrderCase.Api.Events;
using StandingOrderCase.Api.Models;

namespace StandingOrderCase.Api.Consumers.Notification;

public class SendEmailNotificationConsumer : IConsumer<SendEmailNotification>
{
    private readonly StandingOrderCaseContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SendEmailNotificationConsumer> _logger;

    public SendEmailNotificationConsumer(ILogger<SendEmailNotificationConsumer> logger,
        StandingOrderCaseContext context,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    public async Task Consume(ConsumeContext<SendEmailNotification> context)
    {
        var notification = context.Message;

        var entity = await _context
                .Notifications
                .SingleOrDefaultAsync(x => x.Id == notification.NotificationId);

        if (entity == null)
        {
            return;
        }

        _logger.LogInformation($"Sending Email to {notification.ContactInfo} with message {notification.Message}");

        var client = _httpClientFactory.CreateClient();
        await client.PostAsJsonAsync("http://google.com", context.Message);

        _context.NotificationLogs.Add(new NotificationLog
        {
            NotificationStatusEnum = NotificationStatusEnum.Sent,
            NotificationId = notification.NotificationId
        });

        entity.NotificationStatusEnum = NotificationStatusEnum.Sent;

        _logger.LogInformation($"Email sent to {notification.ContactInfo} with message {notification.Message}");

        await _context.SaveChangesAsync();
    }
}