using LeaveFlow.Core.Attributes;
using LeaveFlow.Core.Enums;
using LeaveFlow.Core.Interfaces;

namespace LeaveFlow.Core.Entities;

public class Employee : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    
    // Plain fields (not encrypted)
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public Gender Gender { get; set; } = Gender.Male;
    public string? Department { get; set; }
    public Guid? ManagerId { get; set; }
    public Employee? Manager { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Working hours & days (can override country/company defaults)
    public decimal? DailyWorkingHours { get; set; } // null = use company default
    public string CountryCode { get; set; } = "AE"; // For holidays
    
    // Employee-specific workdays (Bitmask: Mon=1, Tue=2, Wed=4, Thu=8, Fri=16, Sat=32, Sun=64)
    // Default = 31 (Mon-Fri)
    public int? WorkingDays { get; set; } 
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Encrypted fields - stored as encrypted strings in DB
    [Encrypted]
    public string? Salary { get; set; }
    
    [Encrypted]
    public string? PassportNumber { get; set; }
    
    [Encrypted]
    public string? Phone { get; set; }
    
    [Encrypted]
    public string? Address { get; set; }
    
    [Encrypted]
    public string? EmergencyContact { get; set; }
    
    [Encrypted]
    public string? NationalId { get; set; }
    
    [Encrypted]
    public string? BankAccount { get; set; }
    
    // Document paths (uploaded files)
    public string? PassportDocumentPath { get; set; }
    public string? NationalIdDocumentPath { get; set; }
    
    // Navigation properties
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();
}
