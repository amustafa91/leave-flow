using LeaveFlow.Core.Entities;

namespace LeaveFlow.Core.Interfaces;

public interface IHolidayService
{
    Task<List<PublicHoliday>> SyncHolidaysAsync(string countryCode, int year);
    Task<List<PublicHoliday>> GetHolidaysAsync(string countryCode, int year);
    bool IsHoliday(DateTime date, string countryCode);
    int CountWorkingDays(DateTime startDate, DateTime endDate, string countryCode, CompanySettings settings);
}
