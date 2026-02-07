using System.Net.Http.Json;
using LeaveFlow.Core.Entities;
using LeaveFlow.Core.Interfaces;
using LeaveFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LeaveFlow.Infrastructure.Services;

public class HolidayService : IHolidayService
{
    private readonly LeaveFlowDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public HolidayService(LeaveFlowDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
        _apiKey = configuration["Calendarific:ApiKey"] ?? "";
    }

    public async Task<List<PublicHoliday>> SyncHolidaysAsync(string countryCode, int year)
    {
        // Use Calendarific API
        var url = $"https://calendarific.com/api/v2/holidays?api_key={_apiKey}&country={countryCode}&year={year}";
        var responseMsg = await _httpClient.GetAsync(url);
        
        if (!responseMsg.IsSuccessStatusCode)
        {
             var error = await responseMsg.Content.ReadAsStringAsync();
             throw new Exception($"Failed to sync holidays from Calendarific. Status: {responseMsg.StatusCode}. Detail: {error}");
        }

        var json = await responseMsg.Content.ReadAsStringAsync();
        Console.WriteLine($"Calendarific Response: {json.Substring(0, Math.Min(500, json.Length))}...");

        if (string.IsNullOrWhiteSpace(json)) return new List<PublicHoliday>();
        
        CalendarificResponse? response;
        try 
        {
            response = System.Text.Json.JsonSerializer.Deserialize<CalendarificResponse>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to parse Calendarific JSON. Error: {ex.Message}");
        }

        if (response?.Response?.Holidays == null) return new List<PublicHoliday>();

        // Remove existing non-custom/non-modified holidays for this country/year
        var existing = await _context.PublicHolidays
            .Where(h => h.CountryCode == countryCode && h.Year == year && !h.IsCustom && !h.IsModified)
            .ToListAsync();
        if (existing.Any())
        {
            _context.PublicHolidays.RemoveRange(existing);
            await _context.SaveChangesAsync(); // Commit deletes before inserts
        }

        // Filter API response: don't add if we already have a modified/custom holiday for that date
        var protectedDates = await _context.PublicHolidays
            .Where(h => h.CountryCode == countryCode && h.Year == year && (h.IsCustom || h.IsModified))
            .Select(h => h.Date.Date)
            .ToListAsync();

        // Add new holidays (deduplicate by date and skip protected dates)
        var holidays = response.Response.Holidays
            .GroupBy(h => h.Date.Iso)
            .Select(g => g.First())
            .Where(h => !protectedDates.Contains(DateTime.Parse(h.Date.Iso).Date))
            .Select(h => new PublicHoliday
            {
                Date = DateTime.SpecifyKind(DateTime.Parse(h.Date.Iso), DateTimeKind.Utc),
                Name = h.Name,
                LocalName = h.Name,
                CountryCode = countryCode.ToUpper(),
                Year = year,
                IsCustom = false,
                IsModified = false
            }).ToList();

        _context.PublicHolidays.AddRange(holidays);
        await _context.SaveChangesAsync();

        return holidays;
    }

    public async Task<List<PublicHoliday>> GetHolidaysAsync(string countryCode, int year)
    {
        var holidays = await _context.PublicHolidays
            .Where(h => h.CountryCode == countryCode && h.Year == year)
            .OrderBy(h => h.Date)
            .ToListAsync();

        // Sync if empty
        if (!holidays.Any())
        {
            holidays = await SyncHolidaysAsync(countryCode, year);
        }

        return holidays;
    }

    public bool IsHoliday(DateTime date, string countryCode)
    {
        return _context.PublicHolidays
            .Any(h => h.CountryCode == countryCode && h.Date.Date == date.Date);
    }

    public int CountWorkingDays(DateTime startDate, DateTime endDate, string countryCode, CompanySettings settings)
    {
        var workingDays = 0;
        var current = startDate.Date;

        while (current <= endDate.Date)
        {
            if (IsWorkingDay(current, countryCode, settings))
            {
                workingDays++;
            }
            current = current.AddDays(1);
        }

        return workingDays;
    }

    private bool IsWorkingDay(DateTime date, string countryCode, CompanySettings settings)
    {
        // Check if it's a holiday
        if (IsHoliday(date, countryCode)) return false;

        // Check if it's a working day of the week
        return date.DayOfWeek switch
        {
            DayOfWeek.Sunday => settings.Sunday,
            DayOfWeek.Monday => settings.Monday,
            DayOfWeek.Tuesday => settings.Tuesday,
            DayOfWeek.Wednesday => settings.Wednesday,
            DayOfWeek.Thursday => settings.Thursday,
            DayOfWeek.Friday => settings.Friday,
            DayOfWeek.Saturday => settings.Saturday,
            _ => false
        };
    }

    // DTOs for Calendarific API response
    private class CalendarificResponse
    {
        public CalendarificMeta? Meta { get; set; }
        public CalendarificResponseData? Response { get; set; }
    }

    private class CalendarificMeta
    {
        public int Code { get; set; }
    }

    private class CalendarificResponseData
    {
        public List<CalendarificHoliday>? Holidays { get; set; }
    }

    private class CalendarificHoliday
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public CalendarificDate Date { get; set; } = new();
        public List<string>? Type { get; set; }
    }

    private class CalendarificDate
    {
        public string Iso { get; set; } = "";
    }
}
