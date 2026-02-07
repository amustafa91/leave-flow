using LeaveFlow.Core.Interfaces;

namespace LeaveFlow.Core.Entities;

public class LeaveBalance : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public Guid LeaveTypeId { get; set; }
    public LeaveType LeaveType { get; set; } = null!;
    
    public int Year { get; set; }
    
    // Track in hours for flexibility
    public decimal TotalHours { get; set; }   // e.g., 168 hours (21 days × 8 hrs)
    public decimal UsedHours { get; set; }
    public decimal PendingHours { get; set; }
    public decimal RemainingHours => TotalHours - UsedHours - PendingHours;
    public decimal CarriedOverHours { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Note: This requires migration if not exists, but for now we add property.
    public string? Note { get; set; }
}
