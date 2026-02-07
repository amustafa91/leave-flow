using LeaveFlow.Core.Entities;
using LeaveFlow.Core.Interfaces;
using LeaveFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeaveFlow.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly LeaveFlowDbContext _context;

    public NotificationService(LeaveFlowDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Guid userId, string title, string message, NotificationType type = NotificationType.Info, Guid? relatedEntityId = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            RelatedEntityId = relatedEntityId
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Notification>> GetMyNotificationsAsync(Guid userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50) // Limit to last 50
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        if (unread.Any())
        {
            foreach (var n in unread)
            {
                n.IsRead = true;
            }
            await _context.SaveChangesAsync();
        }
    }
}
