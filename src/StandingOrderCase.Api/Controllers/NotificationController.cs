using Microsoft.AspNetCore.Mvc;
using StandingOrderCase.Api.Enums;
using StandingOrderCase.Api.Records;
using StandingOrderCase.Api.Services;

namespace StandingOrderCase.Api.Controllers;

[ApiController]
[Route("notifications")]
public class NotificationController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetNotification[]))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotifications([FromQuery] Guid standingOrderId)
    {
        var notifications = await _notificationService.Get(standingOrderId);

        if (!notifications.Any())
        {
            return NotFound();
        }

        return Ok(notifications);
    }
}