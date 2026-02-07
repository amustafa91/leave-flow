using LeaveFlow.Core.Enums;

using LeaveFlow.Core.Interfaces;

namespace LeaveFlow.Core.Entities;

public class LeaveType : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultHoursPerYear { get; set; }  // e.g., 168 hours (21 days × 8 hrs)
    public bool RequiresApproval { get; set; } = true;
    public bool RequiresDocument { get; set; } = false;
    public ApplicableGender ApplicableGender { get; set; } = ApplicableGender.All;
    public string ColorCode { get; set; } = "#059669"; // Default emerald
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
}
