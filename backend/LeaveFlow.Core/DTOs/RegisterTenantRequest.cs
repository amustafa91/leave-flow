using System.ComponentModel.DataAnnotations;

namespace LeaveFlow.Core.DTOs;

public class RegisterTenantRequest
{
    [Required]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string AdminEmail { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string AdminPassword { get; set; } = string.Empty;

    [Required]
    public string AdminFirstName { get; set; } = string.Empty;

    [Required]
    public string AdminLastName { get; set; } = string.Empty;
}
