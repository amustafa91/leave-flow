using System.Security.Claims;
using LeaveFlow.Core.Interfaces;
using LeaveFlow.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Notification>>> GetMyNotifications()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notifications = await _notificationService.GetMyNotificationsAsync(userId);
        return Ok(notifications);
    }

    [HttpPut("{id}/read")]
    public async Task<ActionResult> MarkAsRead(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _notificationService.MarkAsReadAsync(id, userId);
        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _notificationService.MarkAllAsReadAsync(userId);
        return NoContent();
    }
}
