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
