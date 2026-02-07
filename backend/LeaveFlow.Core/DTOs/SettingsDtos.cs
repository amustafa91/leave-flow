namespace LeaveFlow.Core.DTOs;

public class CompanySettingsDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public decimal DefaultDailyWorkingHours { get; set; }
    public decimal MinLeaveHours { get; set; }
    
    // Workdays
    public bool Sunday { get; set; }
    public bool Monday { get; set; }
    public bool Tuesday { get; set; }
    public bool Wednesday { get; set; }
    public bool Thursday { get; set; }
    public bool Friday { get; set; }
    public bool Saturday { get; set; }
    
    public string DefaultCountryCode { get; set; } = string.Empty;
}

public class UpdateCompanySettingsRequest
{
    public string? CompanyName { get; set; }
    public decimal? DefaultDailyWorkingHours { get; set; }
    public decimal? MinLeaveHours { get; set; }
    
    // Workdays
    public bool? Sunday { get; set; }
    public bool? Monday { get; set; }
    public bool? Tuesday { get; set; }
    public bool? Wednesday { get; set; }
    public bool? Thursday { get; set; }
    public bool? Friday { get; set; }
    public bool? Saturday { get; set; }
    
    public string? DefaultCountryCode { get; set; }
}
