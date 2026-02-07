using LeaveFlow.Core.Attributes;
using LeaveFlow.Core.Enums;

using LeaveFlow.Core.Interfaces;

namespace LeaveFlow.Core.Entities;

public class LeaveRequest : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public Guid LeaveTypeId { get; set; }
    public LeaveType LeaveType { get; set; } = null!;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    // Leave duration - can be hours or full days
    public decimal LeaveHours { get; set; }  // Hours requested (e.g., 2.5)
    public bool IsFullDay { get; set; } = true; // True = full days, False = partial hours
    
    // Calculated values
    public decimal TotalDays { get; set; }  // Calculated based on hours / daily working hours
    
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public string? Reason { get; set; }
    
    // Encrypted fields
    [Encrypted]
    public string? MedicalCertificate { get; set; } // Legacy field, consider using Attachment fields below
    
    public string? AttachmentPath { get; set; }
    public string? AttachmentFileName { get; set; }
    
    [Encrypted]
    public string? PersonalNotes { get; set; }
    
    // Approval workflow
    public Guid? ApproverId { get; set; }
    public Employee? Approver { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApproverComments { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
