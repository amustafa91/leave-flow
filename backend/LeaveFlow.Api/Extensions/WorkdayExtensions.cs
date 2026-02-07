using LeaveFlow.Core.DTOs;
using LeaveFlow.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using LeaveFlow.Core.Entities;

namespace LeaveFlow.Api.Extensions;

public static class WorkdayExtensions 
{
    public static bool IsWorkingDay(this DateTime date, Employee? employee, CountryWorkday? countrySettings, CompanySettings? globalSettings)
    {
        var day = date.DayOfWeek;
        
        // Priority: Employee Settings (Bitmask) -> Country Settings -> Global Settings -> Hardcoded Default (Sat/Sun off)
        if (employee != null && employee.WorkingDays.HasValue)
        {
            var bit = date.DayOfWeek switch
            {
                DayOfWeek.Monday => 1,
                DayOfWeek.Tuesday => 2,
                DayOfWeek.Wednesday => 4,
                DayOfWeek.Thursday => 8,
                DayOfWeek.Friday => 16,
                DayOfWeek.Saturday => 32,
                DayOfWeek.Sunday => 64,
                _ => 0
            };
            
            return (employee.WorkingDays.Value & bit) != 0;
        }
        
        if (countrySettings != null)
        {
            return day switch
            {
                DayOfWeek.Sunday => countrySettings.Sunday,
                DayOfWeek.Monday => countrySettings.Monday,
                DayOfWeek.Tuesday => countrySettings.Tuesday,
                DayOfWeek.Wednesday => countrySettings.Wednesday,
                DayOfWeek.Thursday => countrySettings.Thursday,
                DayOfWeek.Friday => countrySettings.Friday,
                DayOfWeek.Saturday => countrySettings.Saturday,
                _ => false
            };
        }
        
        if (globalSettings != null)
        {
            return day switch
            {
                DayOfWeek.Sunday => globalSettings.Sunday,
                DayOfWeek.Monday => globalSettings.Monday,
                DayOfWeek.Tuesday => globalSettings.Tuesday,
                DayOfWeek.Wednesday => globalSettings.Wednesday,
                DayOfWeek.Thursday => globalSettings.Thursday,
                DayOfWeek.Friday => globalSettings.Friday,
                DayOfWeek.Saturday => globalSettings.Saturday,
                _ => false
            };
        }

        // Default: Mon-Fri working
        return day != DayOfWeek.Saturday && day != DayOfWeek.Sunday;
    }
}
