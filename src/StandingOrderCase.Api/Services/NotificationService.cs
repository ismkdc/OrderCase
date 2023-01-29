using Microsoft.EntityFrameworkCore;
using StandingOrderCase.Api.Enums;
using StandingOrderCase.Api.Events;
using StandingOrderCase.Api.Models;
using StandingOrderCase.Api.Records;

namespace StandingOrderCase.Api.Services;

public class NotificationService
{
    private readonly StandingOrderCaseContext _context;
    private readonly UserService _userService;
    private readonly OutboxService _outboxService;

    public NotificationService(StandingOrderCaseContext context, UserService userService, OutboxService outboxService)
    {
        _context = context;
        _userService = userService;
        _outboxService = outboxService;
    }

    public Task<GetNotification[]> Get(Guid standingOrderId)
    {
        return _context
            .Notifications
            .Where(x => x.StandingOrderId == standingOrderId)
            .Select(x => new GetNotification
                (
                    x.Id,
                    x.NotificationStatusEnum,
                    x.StandingOrderId,
                    x.NotificationTypeEnum,
                    x.Message
                )
            )
            .AsNoTracking()
            .ToArrayAsync();
    }

    public async Task Create(CreateNotification[] notifications)
    {
        if (!notifications.Any())
        {
            return;
        }

        var userId = notifications.First().UserId;
        var allContactInfo = await _userService.GetContactInfo(userId);
        
        if(allContactInfo == null)
        {
            return;
        }

        foreach (var notification in notifications)
        {
            var notificationId = Guid.NewGuid();

            var contactInfo = notification.NotificationTypeEnum switch
            {
                NotificationTypeEnum.Email => allContactInfo.Email,
                NotificationTypeEnum.Sms => allContactInfo.PhoneNumber,
                NotificationTypeEnum.Push => allContactInfo.PushToken,
                _ => throw new ArgumentOutOfRangeException()
            };

            _context.Notifications.Add(new Models.Notification
            {
                Id = notificationId,
                StandingOrderId = notification.SourceEntityId,
                NotificationTypeEnum = notification.NotificationTypeEnum,
                Message = "Your standing order has been created",
                ContactInfo = contactInfo
            });

            _context.NotificationLogs.Add(new NotificationLog
            {
                NotificationId = notificationId,
                NotificationStatusEnum = NotificationStatusEnum.Pending
            });

            var messageObject = GetMessageObject(notification.NotificationTypeEnum, contactInfo, notificationId,
                notification.Message);

            _outboxService.AddMessage(
                new AddOutboxMessage
                (
                    messageObject.GetType().Name,
                    messageObject
                )
            );
        }
    }

    private object GetMessageObject(NotificationTypeEnum type, string? contactInfo, Guid notificationId,
        string message)
    {
        return type switch
        {
            NotificationTypeEnum.Email => new SendEmailNotification
            {
                ContactInfo = contactInfo,
                NotificationId = notificationId,
                Message = message
            },
            NotificationTypeEnum.Sms => new SendSmsNotification
            {
                ContactInfo = contactInfo,
                NotificationId = notificationId,
                Message = message
            },
            NotificationTypeEnum.Push => new SendPushNotification
            {
                ContactInfo = contactInfo,
                NotificationId = notificationId,
                Message = message
            },
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}