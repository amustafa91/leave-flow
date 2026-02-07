using LeaveFlow.Core.Enums;

namespace LeaveFlow.Core.DTOs;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Gender Gender { get; set; }
    public string? Department { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public bool IsActive { get; set; }
    public decimal? DailyWorkingHours { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    
    // Workdays (Bitmask)
    public int? WorkingDays { get; set; } 
    
    // Sensitive fields - masked by default
    public SensitiveFieldDto Salary { get; set; } = new();
    public SensitiveFieldDto PassportNumber { get; set; } = new();
    public SensitiveFieldDto Phone { get; set; } = new();
    public SensitiveFieldDto Address { get; set; } = new();
    public SensitiveFieldDto EmergencyContact { get; set; } = new();
    public SensitiveFieldDto NationalId { get; set; } = new();
    public SensitiveFieldDto BankAccount { get; set; } = new();
    
    public string? PassportDocumentPath { get; set; }
    public string? NationalIdDocumentPath { get; set; }
}

public class SensitiveFieldDto
{
    public string MaskedValue { get; set; } = "********";
    public string? Value { get; set; } // Only populated when revealed
    public bool IsRevealed { get; set; } = false;
    public bool CanReveal { get; set; } = false;
}

public class CreateEmployeeRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public Gender Gender { get; set; } = Gender.Male;
    public string? Department { get; set; }
    public Guid? ManagerId { get; set; }
    public decimal? DailyWorkingHours { get; set; } // null = use company default
    public string CountryCode { get; set; } = "AE";
    
    // Workdays (Bitmask)
    public int? WorkingDays { get; set; } 
    public string? Salary { get; set; }
    public string? PassportNumber { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
    public string? NationalId { get; set; }
    public string? BankAccount { get; set; }
}

public class UpdateEmployeeRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Gender? Gender { get; set; }
    public string? Department { get; set; }
    public Guid? ManagerId { get; set; }
    public UserRole? Role { get; set; }
    public decimal? DailyWorkingHours { get; set; }
    public string? CountryCode { get; set; }

    // Workdays (Bitmask)
    public int? WorkingDays { get; set; } 
    public bool? IsActive { get; set; }
    public string? Salary { get; set; }
    public string? PassportNumber { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
    public string? NationalId { get; set; }
    public string? BankAccount { get; set; }
}

public class RevealSensitiveFieldRequest
{
    public string FieldName { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
    public string? BankAccount { get; set; }
}
