using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LeaveFlow.Core.DTOs;
using LeaveFlow.Core.Entities;
using LeaveFlow.Core.Enums;
using LeaveFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LeaveFlow.Infrastructure.Services;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<Employee?> GetCurrentUserAsync(ClaimsPrincipal principal);
    string GenerateJwtToken(Employee employee);
    Task<(bool Success, string ErrorMessage, Guid? TenantId)> RegisterTenantAsync(RegisterTenantRequest request);
}

public class AuthService : IAuthService
{
    private readonly LeaveFlowDbContext _context;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;

    public AuthService(LeaveFlowDbContext context, string jwtSecret, string jwtIssuer)
    {
        _context = context;
        _jwtSecret = jwtSecret;
        _jwtIssuer = jwtIssuer;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Email == request.Email && e.IsActive);

        if (employee == null || !BCrypt.Net.BCrypt.Verify(request.Password, employee.PasswordHash))
            return null;

        var token = GenerateJwtToken(employee);
        var refreshToken = GenerateRefreshToken();

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = MapToDto(employee)
        };
    }

    public async Task<Employee?> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        return await _context.Employees.FindAsync(userId);
    }

    public string GenerateJwtToken(Employee employee)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new Claim("tenant_id", employee.TenantId.ToString()), // Critical for multi-tenancy
            new Claim(ClaimTypes.Email, employee.Email),
            new Claim(ClaimTypes.Name, employee.FullName),
            new Claim(ClaimTypes.Role, employee.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtIssuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<(bool Success, string ErrorMessage, Guid? TenantId)> RegisterTenantAsync(RegisterTenantRequest request)
    {
        // 1. Check if email already exists globally (assuming email unique across system for login)
        // Or per tenant? For simple login, let's keep it global unique.
        if (await _context.Employees.AnyAsync(e => e.Email == request.AdminEmail))
            return (false, "Email already registered", null);

        var tenantId = Guid.NewGuid();

        // 2. Create Company Settings (The Tenant)
        var companySettings = new CompanySettings
        {
            Id = tenantId, // Using TenantId as ID
            TenantId = tenantId,
            CompanyName = request.CompanyName,
            DefaultDailyWorkingHours = 8.0m,
            MinLeaveHours = 0.5m,
            DefaultCountryCode = "AE",
            Sunday = true, Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = false, Saturday = false
        };

        // 3. Create Admin User
        var admin = new Employee
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = request.AdminEmail,
            FirstName = request.AdminFirstName,
            LastName = request.AdminLastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword),
            Role = UserRole.SuperAdmin,
            Gender = Gender.Male, // Default, can be updated
            Department = "Administration",
            IsActive = true,
            CountryCode = "AE",
            DailyWorkingHours = 8.0m,
            WorkingDays = 31
        };

        // 4. Seed Leave Types
        var leaveTypes = new[] {
            new LeaveType { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Annual", Description = "Annual vacation leave", DefaultHoursPerYear = 168, ApplicableGender = ApplicableGender.All, ColorCode = "#059669" },
            new LeaveType { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Sick", Description = "Sick leave", DefaultHoursPerYear = 112, RequiresDocument = true, ApplicableGender = ApplicableGender.All, ColorCode = "#ef4444" },
            new LeaveType { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Personal", Description = "Personal leave", DefaultHoursPerYear = 40, ApplicableGender = ApplicableGender.All, ColorCode = "#8b5cf6" },
            new LeaveType { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Maternity", Description = "Maternity leave", DefaultHoursPerYear = 720, ApplicableGender = ApplicableGender.Female, ColorCode = "#ec4899" },
            new LeaveType { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Paternity", Description = "Paternity leave", DefaultHoursPerYear = 40, ApplicableGender = ApplicableGender.Male, ColorCode = "#3b82f6" }
        };

        try
        {
            // We need to bypass the Query Filter because we are creating data for a NEW tenant
             // while typically running under NO tenant or a different tenant context.
             // However, for ADDING data, the filter doesn't block us.
             // But Wait! modifying CurrentTenantService? 
             // We are manually setting TenantId on entities.
             // SaveChangesAsync in DbContext will try to overwrite TenantId from CurrentTenantService if it's not null.
             // If we are unauthenticated (registering), CurrentTenantService returns null.
             // My logic in SaveChangesAsync: if (tenantId.HasValue) ... entry.Entity.TenantId = tenantId.Value;
             // So if unauthenticated, it won't overwrite our manual values. Perfect.

            _context.CompanySettings.Add(companySettings);
            _context.Employees.Add(admin);
            _context.LeaveTypes.AddRange(leaveTypes);

            // Create initial balances for Admin
             foreach (var type in leaveTypes)
            {
                _context.LeaveBalances.Add(new LeaveBalance
                {
                    TenantId = tenantId,
                    EmployeeId = admin.Id,
                    LeaveTypeId = type.Id,
                    Year = DateTime.UtcNow.Year,
                    TotalHours = (type.DefaultHoursPerYear / 8.0m) * 8.0m,
                    UsedHours = 0,
                    PendingHours = 0,
                    Note = "Initial balance"
                });
            }

            await _context.SaveChangesAsync();
            return (true, string.Empty, tenantId);
        }
        catch (Exception ex)
        {
            return (false, $"Registration failed: {ex.Message}", null);
        }
    }

    // Helper methods...
    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            Email = employee.Email,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            FullName = employee.FullName,
            Role = employee.Role,
            Gender = employee.Gender,
            Department = employee.Department,
            ManagerId = employee.ManagerId,
            IsActive = employee.IsActive,
            DailyWorkingHours = employee.DailyWorkingHours,
            CountryCode = employee.CountryCode
        };
    }
}
