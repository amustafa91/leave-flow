using LeaveFlow.Core.Enums;

using LeaveFlow.Core.Interfaces;

namespace LeaveFlow.Core.Entities;

/// <summary>
/// Company-wide settings managed by HR/Admin
/// </summary>
public class CompanySettings : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    
    // Working hours
    public decimal DefaultDailyWorkingHours { get; set; } = 8.0m;
    public decimal MinLeaveHours { get; set; } = 0.5m; // 30 minutes minimum
    
    // Workdays (true = working day)
    public bool Sunday { get; set; } = true;    // UAE default
    public bool Monday { get; set; } = true;
    public bool Tuesday { get; set; } = true;
    public bool Wednesday { get; set; } = true;
    public bool Thursday { get; set; } = true;
    public bool Friday { get; set; } = false;   // Weekend in UAE
    public bool Saturday { get; set; } = false; // Weekend in UAE
    
    // Country for public holidays
    public string DefaultCountryCode { get; set; } = "AE";
    
    // Company info
    public string CompanyName { get; set; } = "LeaveFlow";
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
