using System.Security.Claims;
using LeaveFlow.Core.DTOs;
using LeaveFlow.Core.Entities;
using LeaveFlow.Core.Enums;
using LeaveFlow.Infrastructure.Data;
using LeaveFlow.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeaveFlow.Core.Interfaces;

namespace LeaveFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveRequestsController : ControllerBase
{
    private readonly LeaveFlowDbContext _context;
    private readonly IFileService _fileService;
    private readonly INotificationService _notificationService;
    private readonly IHolidayService _holidayService;

    public LeaveRequestsController(LeaveFlowDbContext context, IFileService fileService, INotificationService notificationService, IHolidayService holidayService)
    {
        _context = context;
        _fileService = fileService;
        _notificationService = notificationService;
        _holidayService = holidayService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetAll()
    {
        var currentUserId = GetCurrentUserId();
        var currentUserRole = GetCurrentUserRole();

        IQueryable<LeaveRequest> query = _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.Approver);

        // Filter based on role
        if (currentUserRole == UserRole.Employee)
        {
            query = query.Where(lr => lr.EmployeeId == currentUserId);
        }
        else if (currentUserRole == UserRole.Approver)
        {
            var directReportIds = await _context.Employees
                .Where(e => e.ManagerId == currentUserId)
                .Select(e => e.Id)
                .ToListAsync();
            directReportIds.Add(currentUserId);
            query = query.Where(lr => directReportIds.Contains(lr.EmployeeId));
        }

        var requests = await query.OrderByDescending(lr => lr.CreatedAt).ToListAsync();
        return Ok(requests.Select(MapToDto));
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Approver,HRAdmin,SuperAdmin")]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetPending()
    {
        var currentUserId = GetCurrentUserId();
        var currentUserRole = GetCurrentUserRole();

        IQueryable<LeaveRequest> query = _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .Where(lr => lr.Status == LeaveStatus.Pending);

        if (currentUserRole == UserRole.Approver)
        {
            var directReportIds = await _context.Employees
                .Where(e => e.ManagerId == currentUserId)
                .Select(e => e.Id)
                .ToListAsync();
            query = query.Where(lr => directReportIds.Contains(lr.EmployeeId));
        }

        var requests = await query.OrderBy(lr => lr.CreatedAt).ToListAsync();
        return Ok(requests.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveRequestDto>> GetById(Guid id)
    {
        var request = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.Approver)
            .FirstOrDefaultAsync(lr => lr.Id == id);

        if (request == null)
            return NotFound();

        return Ok(MapToDto(request));
    }

    [HttpPost]
    public async Task<ActionResult<LeaveRequestDto>> Create([FromForm] CreateLeaveRequest request, IFormFile? file)
    {
        var currentUserId = GetCurrentUserId();
        var employee = await _context.Employees.FindAsync(currentUserId);
        if (employee == null)
            return Unauthorized();

        var leaveType = await _context.LeaveTypes.FindAsync(request.LeaveTypeId);
        if (leaveType == null)
            return BadRequest(new { message = "Invalid leave type" });

        // Get company settings for working hours
        var settings = await _context.CompanySettings.FirstOrDefaultAsync();
        var dailyHours = employee.DailyWorkingHours ?? settings?.DefaultDailyWorkingHours ?? 8m;

        // Calculate leave hours
        decimal leaveHours;
        decimal totalDays;
        
        if (request.IsFullDay)
        {
            // Full days - calculate working days between dates
            // Fetch holidays for the employee's country
            var countryCode = employee.CountryCode ?? settings?.DefaultCountryCode ?? "AE";
            var holidays = await _holidayService.GetHolidaysAsync(countryCode, request.StartDate.Year);
            if (request.EndDate.Year != request.StartDate.Year)
            {
                holidays.AddRange(await _holidayService.GetHolidaysAsync(countryCode, request.EndDate.Year));
            }
            
            // Get Country Settings
            var countryWorkdays = await _context.CountryWorkdays.FindAsync(countryCode);

            decimal workingDays = 0;
            for (var date = request.StartDate.Date; date <= request.EndDate.Date; date = date.AddDays(1))
            {
                // Check if working day (using priority: Country -> Company -> Default)
                if (!date.IsWorkingDay(employee, countryWorkdays, settings)) continue;

                // Check holiday
                if (holidays.Any(h => h.Date.Date == date)) continue;

                workingDays++;
            }
            
            if (workingDays == 0) return BadRequest(new { message = "Selected dates contain no working days" });

            leaveHours = workingDays * dailyHours;
            totalDays = workingDays;
        }
        else
        {
            // Partial hours
            leaveHours = request.LeaveHours;
            totalDays = leaveHours / dailyHours;
        }

        // Check leave balance
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.EmployeeId == currentUserId && 
                                       lb.LeaveTypeId == request.LeaveTypeId &&
                                       lb.Year == DateTime.UtcNow.Year);

        if (balance == null || balance.RemainingHours < leaveHours)
            return BadRequest(new { message = "Insufficient leave balance" });

        var leaveRequest = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = currentUserId,
            LeaveTypeId = request.LeaveTypeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            LeaveHours = leaveHours,
            IsFullDay = request.IsFullDay,
            TotalDays = totalDays,
            Reason = request.Reason,
            PersonalNotes = request.PersonalNotes,
            Status = leaveType.RequiresApproval ? LeaveStatus.Pending : LeaveStatus.Approved,
            CreatedAt = DateTime.UtcNow
        };

        if (file != null)
        {
            using var stream = file.OpenReadStream();
            var path = await _fileService.SaveFileAsync(stream, file.FileName, "leaves");
            leaveRequest.AttachmentPath = path;
            leaveRequest.AttachmentFileName = file.FileName;
        }

        // Update pending hours
        balance.PendingHours += leaveHours;

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();

        await _context.Entry(leaveRequest).Reference(lr => lr.Employee).LoadAsync();
        await _context.Entry(leaveRequest).Reference(lr => lr.LeaveType).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = leaveRequest.Id }, MapToDto(leaveRequest));
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Approver,HRAdmin,SuperAdmin")]
    public async Task<ActionResult<LeaveRequestDto>> Approve(Guid id, [FromBody] ApproveLeaveRequest request)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .FirstOrDefaultAsync(lr => lr.Id == id);

        if (leaveRequest == null)
            return NotFound();

        if (leaveRequest.Status != LeaveStatus.Pending)
            return BadRequest(new { message = "Request is not pending" });

        var currentUserId = GetCurrentUserId();
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.EmployeeId == leaveRequest.EmployeeId && 
                                       lb.LeaveTypeId == leaveRequest.LeaveTypeId &&
                                       lb.Year == DateTime.UtcNow.Year);

        if (request.Approve)
        {
            leaveRequest.Status = LeaveStatus.Approved;
            if (balance != null)
            {
                balance.PendingHours -= leaveRequest.LeaveHours;
                balance.UsedHours += leaveRequest.LeaveHours;
            }
        }
        else
        {
            leaveRequest.Status = LeaveStatus.Rejected;
            if (balance != null)
            {
                balance.PendingHours -= leaveRequest.LeaveHours;
            }
        }

        leaveRequest.ApproverId = currentUserId;
        leaveRequest.ApprovedAt = DateTime.UtcNow;
        leaveRequest.ApproverComments = request.Comments;
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Send Notification
        var type = request.Approve ? NotificationType.Success : NotificationType.Warning;
        var action = request.Approve ? "Approved" : "Rejected";
        await _notificationService.CreateAsync(
            leaveRequest.EmployeeId, 
            $"Leave Request {action}", 
            $"Your {leaveRequest.LeaveType.Name} leave request ({leaveRequest.TotalDays} days) has been {action.ToLower()}.", 
            type, 
            leaveRequest.Id);

        await _context.Entry(leaveRequest).Reference(lr => lr.Approver).LoadAsync();
        return Ok(MapToDto(leaveRequest));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var leaveRequest = await _context.LeaveRequests.FindAsync(id);

        if (leaveRequest == null)
            return NotFound();

        if (leaveRequest.EmployeeId != currentUserId)
            return Forbid();

        if (leaveRequest.Status == LeaveStatus.Rejected || leaveRequest.Status == LeaveStatus.Cancelled)
            return BadRequest(new { message = "Request is already processed" });

        // Allow cancellation if Pending OR (Approved AND StartDate > Now)
        if (leaveRequest.Status == LeaveStatus.Approved && leaveRequest.StartDate.Date <= DateTime.UtcNow.Date)
            return BadRequest(new { message = "Cannot cancel started or past leave" });

        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.EmployeeId == leaveRequest.EmployeeId && 
                                       lb.LeaveTypeId == leaveRequest.LeaveTypeId &&
                                       lb.Year == DateTime.UtcNow.Year);

        if (balance != null)
        {
            if (leaveRequest.Status == LeaveStatus.Approved)
            {
                balance.UsedHours -= leaveRequest.LeaveHours;
            }
            else if (leaveRequest.Status == LeaveStatus.Pending)
            {
                balance.PendingHours -= leaveRequest.LeaveHours;
            }
        }

        leaveRequest.Status = LeaveStatus.Cancelled;
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("balances")]
    public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetBalances()
    {
        var currentUserId = GetCurrentUserId();
        var year = DateTime.UtcNow.Year;
        var dayOfYear = DateTime.UtcNow.DayOfYear;
        var daysInYear = DateTime.IsLeapYear(year) ? 366m : 365m;

        var balances = await _context.LeaveBalances
            .Include(lb => lb.LeaveType)
            .Include(lb => lb.Employee) // Include Employee for DailyHours
            .Where(lb => lb.EmployeeId == currentUserId && lb.Year == year)
            .ToListAsync();

        return Ok(balances.Select(b => {
             var divisor = b.Employee.DailyWorkingHours ?? 8m; // Dynamic divisor
             var accruedHours = (b.TotalHours / daysInYear) * dayOfYear;
             if (accruedHours > b.TotalHours) accruedHours = b.TotalHours;
             
             return new LeaveBalanceDto
             {
                Id = b.Id,
                LeaveTypeId = b.LeaveTypeId,
                LeaveTypeName = b.LeaveType.Name,
                LeaveTypeColor = b.LeaveType.ColorCode,
                Year = b.Year,
                TotalHours = b.TotalHours,
                UsedHours = b.UsedHours,
                PendingHours = b.PendingHours,
                RemainingHours = b.RemainingHours,
                AccruedHours = Math.Round(accruedHours, 2),
                
                // Use dynamic divisor for display
                TotalDays = Math.Round(b.TotalHours / divisor, 2),
                UsedDays = Math.Round(b.UsedHours / divisor, 2),
                RemainingDays = Math.Round(b.RemainingHours / divisor, 2),
                AccruedDays = Math.Round(accruedHours / divisor, 2)
             };
        }));
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    private UserRole GetCurrentUserRole()
    {
        var claim = User.FindFirst(ClaimTypes.Role)?.Value;
        return Enum.TryParse<UserRole>(claim, out var role) ? role : UserRole.Employee;
    }

    private static LeaveRequestDto MapToDto(LeaveRequest lr)
    {
        return new LeaveRequestDto
        {
            Id = lr.Id,
            EmployeeId = lr.EmployeeId,
            EmployeeName = lr.Employee.FullName,
            LeaveTypeId = lr.LeaveTypeId,
            LeaveTypeName = lr.LeaveType.Name,
            LeaveTypeColor = lr.LeaveType.ColorCode,
            StartDate = lr.StartDate,
            EndDate = lr.EndDate,
            LeaveHours = lr.LeaveHours,
            IsFullDay = lr.IsFullDay,
            TotalDays = lr.TotalDays,
            Status = lr.Status,
            Reason = lr.Reason,
            ApproverName = lr.Approver?.FullName,
            ApprovedAt = lr.ApprovedAt,
            ApproverComments = lr.ApproverComments,
            AttachmentPath = lr.AttachmentPath,
            AttachmentFileName = lr.AttachmentFileName,
            CreatedAt = lr.CreatedAt
        };
    }
}
