using LeaveFlow.Core.Enums;

namespace LeaveFlow.Core.DTOs;

public class LeaveRequestDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public string LeaveTypeColor { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal LeaveHours { get; set; }  // Hours requested
    public bool IsFullDay { get; set; }      // True = full days
    public decimal TotalDays { get; set; }   // Calculated
    public LeaveStatus Status { get; set; }
    public string? Reason { get; set; }
    public string? ApproverName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApproverComments { get; set; }
    public string? AttachmentPath { get; set; }
    public string? AttachmentFileName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateLeaveRequest
{
    public Guid LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsFullDay { get; set; } = true;    // True = full days
    public decimal LeaveHours { get; set; }         // Hours if partial day
    public string? Reason { get; set; }
    public string? PersonalNotes { get; set; }
}

public class ApproveLeaveRequest
{
    public bool Approve { get; set; }
    public string? Comments { get; set; }
}

public class LeaveBalanceDto
{
    public Guid Id { get; set; }
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public string LeaveTypeColor { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal TotalHours { get; set; }
    public decimal UsedHours { get; set; }
    public decimal PendingHours { get; set; }
    public decimal RemainingHours { get; set; }
    public decimal AccruedHours { get; set; }
    // Helper for display
    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal RemainingDays { get; set; }
    public decimal AccruedDays { get; set; }
    public string? Note { get; set; }
}

public class UpdateLeaveBalanceRequest
{
    public decimal TotalHours { get; set; }
    public string? Note { get; set; }
}  // HR can set custom hours

public class LeaveTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultHoursPerYear { get; set; }
    public int DefaultDaysPerYear => (int)(DefaultHoursPerYear / 8);
    public bool RequiresApproval { get; set; }
    public bool RequiresDocument { get; set; }
    public string ColorCode { get; set; } = string.Empty;
}

public class PublicHolidayDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LocalName { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public bool IsCustom { get; set; }
}

public class CreateHolidayRequest
{
    public DateTime Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LocalName { get; set; }
    public string CountryCode { get; set; } = "AE";
}
