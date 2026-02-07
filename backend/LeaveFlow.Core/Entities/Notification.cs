using LeaveFlow.Core.Enums;

using LeaveFlow.Core.Interfaces;

namespace LeaveFlow.Core.Entities;

public class Notification : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? RelatedEntityId { get; set; }

    // Navigation property
    public Employee? User { get; set; }
}

public enum NotificationType
{
    Info = 0,
    Success = 1,
    Warning = 2,
    Error = 3
}
