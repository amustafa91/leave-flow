using LeaveFlow.Core.DTOs;
using LeaveFlow.Core.Entities;
using LeaveFlow.Core.Interfaces;
using LeaveFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HolidaysController : ControllerBase
{
    private readonly LeaveFlowDbContext _context;
    private readonly IHolidayService _holidayService;

    public HolidaysController(LeaveFlowDbContext context, IHolidayService holidayService)
    {
        _context = context;
        _holidayService = holidayService;
    }

    [HttpGet("{countryCode}/{year}")]
    public async Task<ActionResult<List<PublicHolidayDto>>> GetHolidays(string countryCode, int year)
    {
        var holidays = await _holidayService.GetHolidaysAsync(countryCode.ToUpper(), year);
        
        return Ok(holidays.Select(h => new PublicHolidayDto
        {
            Id = h.Id,
            Date = h.Date,
            Name = h.Name,
            LocalName = h.LocalName,
            CountryCode = h.CountryCode,
            IsCustom = h.IsCustom
        }).ToList());
    }

    [HttpPost("sync/{countryCode}/{year}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<ActionResult<List<PublicHolidayDto>>> SyncHolidays(string countryCode, int year)
    {
        var holidays = await _holidayService.SyncHolidaysAsync(countryCode.ToUpper(), year);
        
        return Ok(holidays.Select(h => new PublicHolidayDto
        {
            Id = h.Id,
            Date = h.Date,
            Name = h.Name,
            LocalName = h.LocalName,
            CountryCode = h.CountryCode,
            IsCustom = h.IsCustom
        }).ToList());
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<ActionResult<PublicHolidayDto>> AddCustomHoliday([FromBody] CreateHolidayRequest request)
    {
        var holiday = new PublicHoliday
        {
            Date = request.Date,
            Name = request.Name,
            LocalName = request.LocalName,
            CountryCode = request.CountryCode.ToUpper(),
            Year = request.Date.Year,
            IsCustom = true
        };

        _context.PublicHolidays.Add(holiday);
        await _context.SaveChangesAsync();

        return Ok(new PublicHolidayDto
        {
            Id = holiday.Id,
            Date = holiday.Date,
            Name = holiday.Name,
            LocalName = holiday.LocalName,
            CountryCode = holiday.CountryCode,
            IsCustom = holiday.IsCustom
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<ActionResult<PublicHolidayDto>> UpdateHoliday(Guid id, [FromBody] CreateHolidayRequest request)
    {
        var holiday = await _context.PublicHolidays.FindAsync(id);
        if (holiday == null) return NotFound();

        holiday.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
        holiday.Name = request.Name;
        holiday.LocalName = request.LocalName ?? request.Name;
        holiday.CountryCode = request.CountryCode.ToUpper();
        holiday.Year = request.Date.Year;
        holiday.IsModified = true;

        await _context.SaveChangesAsync();

        return Ok(new PublicHolidayDto
        {
            Id = holiday.Id,
            Date = holiday.Date,
            Name = holiday.Name,
            LocalName = holiday.LocalName,
            CountryCode = holiday.CountryCode,
            IsCustom = holiday.IsCustom
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,HRAdmin")]
    public async Task<ActionResult> DeleteHoliday(Guid id)
    {
        var holiday = await _context.PublicHolidays.FindAsync(id);
        if (holiday == null) return NotFound();

        _context.PublicHolidays.Remove(holiday);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
