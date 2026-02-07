using LeaveFlow.Core.Interfaces;

namespace LeaveFlow.Core.Entities;

public class AuditLog : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Employee User { get; set; } = null!;
    public string Action { get; set; } = string.Empty; // ViewSensitiveData, UpdateEmployee, etc.
    public string EntityType { get; set; } = string.Empty; // Employee, LeaveRequest, etc.
    public Guid EntityId { get; set; }
    public string? FieldName { get; set; } // Which sensitive field was accessed
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
