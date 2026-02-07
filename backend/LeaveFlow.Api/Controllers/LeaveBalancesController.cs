using System.Security.Claims;
using LeaveFlow.Core.DTOs;
using LeaveFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveBalancesController : ControllerBase
{
    private readonly LeaveFlowDbContext _context;

    public LeaveBalancesController(LeaveFlowDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetMyBalances()
    {
        var currentUserId = GetCurrentUserId();
        var year = DateTime.UtcNow.Year;

        var balances = await _context.LeaveBalances
            .Include(lb => lb.LeaveType)
            .Include(lb => lb.Employee)
            .Where(lb => lb.EmployeeId == currentUserId && lb.Year == year)
            .ToListAsync();

        return Ok(balances.Select(lb => {
            var divisor = lb.Employee.DailyWorkingHours ?? 8m;
            return new LeaveBalanceDto
            {
                Id = lb.Id,
                LeaveTypeId = lb.LeaveTypeId,
                LeaveTypeName = lb.LeaveType.Name,
                LeaveTypeColor = lb.LeaveType.ColorCode,
                Year = lb.Year,
                TotalHours = lb.TotalHours,
                UsedHours = lb.UsedHours,
                PendingHours = lb.PendingHours,
                RemainingHours = lb.TotalHours - lb.UsedHours - lb.PendingHours,
                TotalDays = Math.Round(lb.TotalHours / divisor, 2),
                UsedDays = Math.Round(lb.UsedHours / divisor, 2),
                RemainingDays = Math.Round((lb.TotalHours - lb.UsedHours - lb.PendingHours) / divisor, 2)
            };
        }));
    }

    [HttpGet("employee/{employeeId}")]
    [Authorize(Roles = "Approver,HRAdmin,SuperAdmin")]
    public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetEmployeeBalances(Guid employeeId)
    {
        var year = DateTime.UtcNow.Year;

        var balances = await _context.LeaveBalances
            .Include(lb => lb.LeaveType)
            .Include(lb => lb.Employee)
            .Where(lb => lb.EmployeeId == employeeId && lb.Year == year)
            .ToListAsync();

        return Ok(balances.Select(lb => {
            var divisor = lb.Employee.DailyWorkingHours ?? 8m;
            return new LeaveBalanceDto
            {
                Id = lb.Id,
                LeaveTypeId = lb.LeaveTypeId,
                LeaveTypeName = lb.LeaveType.Name,
                LeaveTypeColor = lb.LeaveType.ColorCode,
                Year = lb.Year,
                TotalHours = lb.TotalHours,
                UsedHours = lb.UsedHours,
                PendingHours = lb.PendingHours,
                RemainingHours = lb.TotalHours - lb.UsedHours - lb.PendingHours,
                TotalDays = Math.Round(lb.TotalHours / divisor, 2),
                UsedDays = Math.Round(lb.UsedHours / divisor, 2),
                RemainingDays = Math.Round((lb.TotalHours - lb.UsedHours - lb.PendingHours) / divisor, 2)
            };
        }));
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
