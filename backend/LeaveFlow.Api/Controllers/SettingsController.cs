using LeaveFlow.Core.DTOs;
using LeaveFlow.Core.Entities;
using LeaveFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly LeaveFlowDbContext _context;

    public SettingsController(LeaveFlowDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<CompanySettingsDto>> GetSettings()
    {
        var settings = await _context.CompanySettings.FirstOrDefaultAsync();
        if (settings == null) return NotFound();

        return Ok(new CompanySettingsDto
        {
            Id = settings.Id,
            CompanyName = settings.CompanyName,
            DefaultDailyWorkingHours = settings.DefaultDailyWorkingHours,
            MinLeaveHours = settings.MinLeaveHours,
            Sunday = settings.Sunday,
            Monday = settings.Monday,
            Tuesday = settings.Tuesday,
            Wednesday = settings.Wednesday,
            Thursday = settings.Thursday,
            Friday = settings.Friday,
            Saturday = settings.Saturday,
            DefaultCountryCode = settings.DefaultCountryCode
        });
    }

    [HttpPut]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<ActionResult<CompanySettingsDto>> UpdateSettings([FromBody] UpdateCompanySettingsRequest request)
    {
        var settings = await _context.CompanySettings.FirstOrDefaultAsync();
        if (settings == null) return NotFound();

        if (request.CompanyName != null) settings.CompanyName = request.CompanyName;
        if (request.DefaultDailyWorkingHours.HasValue) settings.DefaultDailyWorkingHours = request.DefaultDailyWorkingHours.Value;
        if (request.MinLeaveHours.HasValue) settings.MinLeaveHours = request.MinLeaveHours.Value;
        if (request.Sunday.HasValue) settings.Sunday = request.Sunday.Value;
        if (request.Monday.HasValue) settings.Monday = request.Monday.Value;
        if (request.Tuesday.HasValue) settings.Tuesday = request.Tuesday.Value;
        if (request.Wednesday.HasValue) settings.Wednesday = request.Wednesday.Value;
        if (request.Thursday.HasValue) settings.Thursday = request.Thursday.Value;
        if (request.Friday.HasValue) settings.Friday = request.Friday.Value;
        if (request.Saturday.HasValue) settings.Saturday = request.Saturday.Value;
        if (request.DefaultCountryCode != null) settings.DefaultCountryCode = request.DefaultCountryCode;

        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new CompanySettingsDto
        {
            Id = settings.Id,
            CompanyName = settings.CompanyName,
            DefaultDailyWorkingHours = settings.DefaultDailyWorkingHours,
            MinLeaveHours = settings.MinLeaveHours,
            Sunday = settings.Sunday,
            Monday = settings.Monday,
            Tuesday = settings.Tuesday,
            Wednesday = settings.Wednesday,
            Thursday = settings.Thursday,
            Friday = settings.Friday,
            Saturday = settings.Saturday,
            DefaultCountryCode = settings.DefaultCountryCode
        });
    }
    [HttpGet("country/{code}")]
    public async Task<ActionResult<CountryWorkday>> GetCountryWorkdays(string code)
    {
        var workdays = await _context.CountryWorkdays.FindAsync(code.ToUpper());
        if (workdays == null)
        {
            // Return defaults if not configured
            // Defaults: Mon-Fri working, Sat-Sun off (Standard) or assume Company Default?
            // Let's return a default based on Company Settings logic if possible, or just standard 5-day week
            return Ok(new CountryWorkday
            {
                CountryCode = code.ToUpper(),
                Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true,
                Saturday = false, Sunday = false
            });
        }
        return Ok(workdays);
    }

    [HttpPost("country")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<ActionResult<CountryWorkday>> UpdateCountryWorkdays([FromBody] CountryWorkday request)
    {
        var workdays = await _context.CountryWorkdays.FindAsync(request.CountryCode.ToUpper());
        if (workdays == null)
        {
            workdays = new CountryWorkday { CountryCode = request.CountryCode.ToUpper() };
            _context.CountryWorkdays.Add(workdays);
        }

        workdays.Sunday = request.Sunday;
        workdays.Monday = request.Monday;
        workdays.Tuesday = request.Tuesday;
        workdays.Wednesday = request.Wednesday;
        workdays.Thursday = request.Thursday;
        workdays.Friday = request.Friday;
        workdays.Saturday = request.Saturday;

        await _context.SaveChangesAsync();
        return Ok(workdays);
    }
}
