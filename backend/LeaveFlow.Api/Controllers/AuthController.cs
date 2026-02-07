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

    // Self-registration removed - HR adds employees via EmployeesController
}
