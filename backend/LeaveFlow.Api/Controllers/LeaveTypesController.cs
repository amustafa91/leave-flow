using System.Security.Claims;
using LeaveFlow.Core.DTOs;
using LeaveFlow.Core.Entities;
using LeaveFlow.Core.Enums;
using LeaveFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveTypesController : ControllerBase
{
    private readonly LeaveFlowDbContext _context;

    public LeaveTypesController(LeaveFlowDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveTypeDto>>> GetAll()
    {
        var currentUserId = GetCurrentUserId();
        var employee = await _context.Employees.FindAsync(currentUserId);
        
        var types = await _context.LeaveTypes
            .Where(lt => lt.IsActive)
            .ToListAsync();

        // Filter by gender if employee exists
        if (employee != null)
        {
            types = types.Where(lt => 
                lt.ApplicableGender == ApplicableGender.All ||
                (lt.ApplicableGender == ApplicableGender.Male && employee.Gender == Gender.Male) ||
                (lt.ApplicableGender == ApplicableGender.Female && employee.Gender == Gender.Female)
            ).ToList();
        }

        return Ok(types.Select(lt => new LeaveTypeDto
        {
            Id = lt.Id,
            Name = lt.Name,
            Description = lt.Description,
            DefaultHoursPerYear = lt.DefaultHoursPerYear,
            RequiresApproval = lt.RequiresApproval,
            RequiresDocument = lt.RequiresDocument,
            ColorCode = lt.ColorCode
        }));
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<ActionResult<LeaveTypeDto>> Create([FromBody] LeaveTypeDto request)
    {
        var leaveType = new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            DefaultHoursPerYear = request.DefaultHoursPerYear,
            RequiresApproval = request.RequiresApproval,
            RequiresDocument = request.RequiresDocument,
            ColorCode = request.ColorCode,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.LeaveTypes.Add(leaveType);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new LeaveTypeDto
        {
            Id = leaveType.Id,
            Name = leaveType.Name,
            Description = leaveType.Description,
            DefaultHoursPerYear = leaveType.DefaultHoursPerYear,
            RequiresApproval = leaveType.RequiresApproval,
            RequiresDocument = leaveType.RequiresDocument,
            ColorCode = leaveType.ColorCode
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<ActionResult<LeaveTypeDto>> Update(Guid id, [FromBody] LeaveTypeDto request)
    {
        var leaveType = await _context.LeaveTypes.FindAsync(id);
        if (leaveType == null)
            return NotFound();

        leaveType.Name = request.Name;
        leaveType.Description = request.Description;
        leaveType.DefaultHoursPerYear = request.DefaultHoursPerYear;
        leaveType.RequiresApproval = request.RequiresApproval;
        leaveType.RequiresDocument = request.RequiresDocument;
        leaveType.ColorCode = request.ColorCode;

        await _context.SaveChangesAsync();

        return Ok(new LeaveTypeDto
        {
            Id = leaveType.Id,
            Name = leaveType.Name,
            Description = leaveType.Description,
            DefaultHoursPerYear = leaveType.DefaultHoursPerYear,
            RequiresApproval = leaveType.RequiresApproval,
            RequiresDocument = leaveType.RequiresDocument,
            ColorCode = leaveType.ColorCode
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var leaveType = await _context.LeaveTypes.FindAsync(id);
        if (leaveType == null)
            return NotFound();

        leaveType.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
