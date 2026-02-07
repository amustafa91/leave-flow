using System.Security.Claims;
using LeaveFlow.Core.DTOs;
using LeaveFlow.Core.Entities;
using LeaveFlow.Core.Enums;
using LeaveFlow.Core.Interfaces;
using LeaveFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly LeaveFlowDbContext _context;
    private readonly IFileService _fileService;
    private readonly INotificationService _notificationService;

    public EmployeesController(LeaveFlowDbContext context, IFileService fileService, INotificationService notificationService)
    {
        _context = context;
        _fileService = fileService;
        _notificationService = notificationService;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
    {
        var employees = await _context.Employees
            .Include(e => e.Manager)
            .OrderBy(e => e.FirstName)
            .ToListAsync();

        var currentUserRole = GetCurrentUserRole();
        return Ok(employees.Select(e => MapToDto(e, currentUserRole)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetById(Guid id)
    {
        var employee = await _context.Employees
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            return NotFound();

        var currentUserId = GetCurrentUserId();
        var currentUserRole = GetCurrentUserRole();

        // Check authorization - users can only see their own data unless admin/HR
        if (currentUserId != id && currentUserRole != UserRole.SuperAdmin && currentUserRole != UserRole.HRAdmin)
            return Forbid();

        return Ok(MapToDto(employee, currentUserRole, currentUserId == id));
    }

    [HttpGet("me")]
    public async Task<ActionResult<EmployeeDto>> GetMe()
    {
        var currentUserId = GetCurrentUserId();
        var employee = await _context.Employees
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == currentUserId);

        if (employee == null)
            return NotFound();

        return Ok(MapToDto(employee, GetCurrentUserRole(), true));
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeRequest request)
    {
        if (await _context.Employees.AnyAsync(e => e.Email == request.Email))
            return BadRequest(new { message = "Email already exists" });

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role,
            Gender = request.Gender,
            Department = request.Department,
            ManagerId = request.ManagerId,
            DailyWorkingHours = request.DailyWorkingHours,
            CountryCode = request.CountryCode,
            Salary = request.Salary,
            PassportNumber = request.PassportNumber,
            Phone = request.Phone,
            Address = request.Address,
            EmergencyContact = request.EmergencyContact,
            NationalId = request.NationalId,
            BankAccount = request.BankAccount,
            WorkingDays = request.WorkingDays,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Create leave balances based on gender
        var leaveTypes = await _context.LeaveTypes.Where(lt => lt.IsActive).ToListAsync();
        foreach (var leaveType in leaveTypes)
        {
            // Skip gender-specific leave types that don't apply
            if (leaveType.ApplicableGender == ApplicableGender.Male && employee.Gender != Gender.Male)
                continue;
            if (leaveType.ApplicableGender == ApplicableGender.Female && employee.Gender != Gender.Female)
                continue;

            _context.LeaveBalances.Add(new LeaveBalance
            {
                Id = Guid.NewGuid(),
                EmployeeId = employee.Id,
                LeaveTypeId = leaveType.Id,
                Year = DateTime.UtcNow.Year,
                // Adjust TotalHours to match employee's working hours so Days are integers.
                // Assuming DefaultHoursPerYear is based on 8h standard.
                TotalHours = (leaveType.DefaultHoursPerYear / 8m) * (employee.DailyWorkingHours ?? 8m),
                UsedHours = 0,
                PendingHours = 0,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, MapToDto(employee, GetCurrentUserRole()));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<ActionResult<EmployeeDto>> Update(Guid id, [FromBody] UpdateEmployeeRequest request)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound();

        if (request.FirstName != null) employee.FirstName = request.FirstName;
        if (request.LastName != null) employee.LastName = request.LastName;
        if (request.Gender.HasValue) employee.Gender = request.Gender.Value;
        if (request.Department != null) employee.Department = request.Department;
        if (request.ManagerId != null) employee.ManagerId = request.ManagerId;
        if (request.Role.HasValue) employee.Role = request.Role.Value;
        if (request.DailyWorkingHours.HasValue) employee.DailyWorkingHours = request.DailyWorkingHours;
        if (request.CountryCode != null) employee.CountryCode = request.CountryCode;
        if (request.IsActive.HasValue) employee.IsActive = request.IsActive.Value;
        if (request.Salary != null) employee.Salary = request.Salary;
        if (request.PassportNumber != null) employee.PassportNumber = request.PassportNumber;
        if (request.Phone != null) employee.Phone = request.Phone;
        if (request.Address != null) employee.Address = request.Address;
        if (request.EmergencyContact != null) employee.EmergencyContact = request.EmergencyContact;
        if (request.NationalId != null) employee.NationalId = request.NationalId;
        if (request.BankAccount != null) employee.BankAccount = request.BankAccount;
        
        if (request.WorkingDays.HasValue) employee.WorkingDays = request.WorkingDays.Value;
        
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _context.SaveChangesAsync();
        return Ok(MapToDto(employee, GetCurrentUserRole()));
    }

    [HttpPut("profile")]
    public async Task<ActionResult<EmployeeDto>> UpdateProfile(
        [FromForm] UpdateProfileRequest request,
        IFormFile? passportFile,
        IFormFile? nationalIdFile)
    {
        var currentUserId = GetCurrentUserId();
        var employee = await _context.Employees.FindAsync(currentUserId);
        if (employee == null) return NotFound();

        if (request.Phone != null) employee.Phone = request.Phone;
        if (request.Address != null) employee.Address = request.Address;
        if (request.EmergencyContact != null) employee.EmergencyContact = request.EmergencyContact;
        if (request.BankAccount != null) employee.BankAccount = request.BankAccount;

        if (passportFile != null)
        {
            using var stream = passportFile.OpenReadStream();
            var path = await _fileService.SaveFileAsync(stream, passportFile.FileName, "documents");
            employee.PassportDocumentPath = path;
        }

        if (nationalIdFile != null)
        {
            using var stream = nationalIdFile.OpenReadStream();
            var path = await _fileService.SaveFileAsync(stream, nationalIdFile.FileName, "documents");
            employee.NationalIdDocumentPath = path;
        }

        employee.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(MapToDto(employee, GetCurrentUserRole(), true));
    }



    [HttpGet("{id}/balances")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetBalances(Guid id)
    {
        var year = DateTime.UtcNow.Year;
        var dayOfYear = DateTime.UtcNow.DayOfYear;
        var daysInYear = DateTime.IsLeapYear(year) ? 366m : 365m;

        var balances = await _context.LeaveBalances
            .Include(lb => lb.LeaveType)
            .Include(lb => lb.Employee)
            .Where(lb => lb.EmployeeId == id && lb.Year == year)
            .ToListAsync();

        return Ok(balances.Select(b => {
             var divisor = b.Employee.DailyWorkingHours ?? 8m;
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
                RemainingHours = b.TotalHours - b.UsedHours - b.PendingHours,
                AccruedHours = Math.Round(accruedHours, 2),
                TotalDays = Math.Round(b.TotalHours / divisor, 2),
                UsedDays = Math.Round(b.UsedHours / divisor, 2),
                RemainingDays = Math.Round((b.TotalHours - b.UsedHours - b.PendingHours) / divisor, 2),
                AccruedDays = Math.Round(accruedHours / divisor, 2),
                Note = b.Note
             };
        }));
    }

    // HR can adjust leave balance for individual employees
    [HttpPut("{id}/leave-balance/{leaveTypeId}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<ActionResult> UpdateLeaveBalance(Guid id, Guid leaveTypeId, [FromBody] UpdateLeaveBalanceRequest request)
    {
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(b => b.EmployeeId == id && b.LeaveTypeId == leaveTypeId && b.Year == DateTime.UtcNow.Year);

        if (balance == null)
        {
            // Create new balance
            balance = new LeaveBalance
            {
                Id = Guid.NewGuid(),
                EmployeeId = id,
                LeaveTypeId = leaveTypeId,
                Year = DateTime.UtcNow.Year,
                TotalHours = request.TotalHours,
                UsedHours = 0,
                PendingHours = 0,
                CreatedAt = DateTime.UtcNow,
                Note = request.Note
            };
            _context.LeaveBalances.Add(balance);
        }
        else
        {
            balance.TotalHours = request.TotalHours;
            balance.Note = request.Note;
            balance.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Notify
        var leaveType = await _context.LeaveTypes.FindAsync(leaveTypeId);
        var typeName = leaveType?.Name ?? "Leave";
        await _notificationService.CreateAsync(
            id,
            "Balance Updated",
            $"HR has updated your {typeName} balance. Total Hours: {request.TotalHours}" + (string.IsNullOrEmpty(request.Note) ? "." : $". Note: {request.Note}"),
            NotificationType.Info,
            balance.Id);

        return Ok(new { message = "Leave balance updated" });
    }

    [HttpPost("{id}/reveal")]
    public async Task<ActionResult<object>> RevealSensitiveField(Guid id, [FromBody] RevealSensitiveFieldRequest request)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound();

        var currentUserId = GetCurrentUserId();
        var currentUserRole = GetCurrentUserRole();

        // Check authorization
        var canView = currentUserId == id || currentUserRole == UserRole.SuperAdmin || currentUserRole == UserRole.HRAdmin;
        if (!canView)
            return Forbid();

        // Get the field value
        var value = request.FieldName.ToLower() switch
        {
            "salary" => employee.Salary,
            "passportnumber" => employee.PassportNumber,
            "phone" => employee.Phone,
            "address" => employee.Address,
            "emergencycontact" => employee.EmergencyContact,
            "nationalid" => employee.NationalId,
            "bankaccount" => employee.BankAccount,
            _ => null
        };

        if (value == null)
            return BadRequest(new { message = "Invalid field name" });

        // Create audit log
        _context.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Action = "ViewSensitiveData",
            EntityType = "Employee",
            EntityId = id,
            FieldName = request.FieldName,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return Ok(new { fieldName = request.FieldName, value });
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

    private static EmployeeDto MapToDto(Employee employee, UserRole viewerRole, bool isOwnProfile = false)
    {
        var canReveal = isOwnProfile || viewerRole == UserRole.SuperAdmin || viewerRole == UserRole.HRAdmin;

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
            ManagerName = employee.Manager?.FullName,
            IsActive = employee.IsActive,
            DailyWorkingHours = employee.DailyWorkingHours,
            CountryCode = employee.CountryCode,
            Salary = new SensitiveFieldDto { CanReveal = canReveal },
            PassportNumber = new SensitiveFieldDto { CanReveal = canReveal },
            Phone = new SensitiveFieldDto { CanReveal = canReveal },
            Address = new SensitiveFieldDto { CanReveal = canReveal },
            EmergencyContact = new SensitiveFieldDto { CanReveal = canReveal },
            NationalId = new SensitiveFieldDto { CanReveal = canReveal },
            BankAccount = new SensitiveFieldDto { CanReveal = canReveal },
            WorkingDays = employee.WorkingDays,
            PassportDocumentPath = employee.PassportDocumentPath,
            NationalIdDocumentPath = employee.NationalIdDocumentPath
        };
    }
}
