using LeaveFlow.Core.DTOs;
using LeaveFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeaveFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
            return Unauthorized(new { message = "Invalid email or password" });

        return Ok(result);
    }

    [HttpPost("register-tenant")]
    public async Task<ActionResult> RegisterTenant([FromBody] RegisterTenantRequest request)
    {
        // Handled via AuthService/Service layer ideally, but for speed implementing here
        // We need access to DbContext directly or move logic to AuthService. 
        // Let's add RegisterTenantAsync to IAuthService instead.
        var result = await _authService.RegisterTenantAsync(request);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Tenant registered successfully", tenantId = result.TenantId });
    }
}
