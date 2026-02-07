using LeaveFlow.Core.Entities;

namespace LeaveFlow.Core.Interfaces;

public interface INotificationService
{
    Task CreateAsync(Guid userId, string title, string message, NotificationType type = NotificationType.Info, Guid? relatedEntityId = null);
    Task<IEnumerable<Notification>> GetMyNotificationsAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
}
