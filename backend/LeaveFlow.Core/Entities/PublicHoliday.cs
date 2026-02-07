using LeaveFlow.Core.Interfaces;

namespace LeaveFlow.Core.Entities;

/// <summary>
/// Public holiday synced from external API
/// </summary>
public class PublicHoliday : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    
    public DateTime Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LocalName { get; set; }
    public string CountryCode { get; set; } = "AE";
    public int Year { get; set; }
    
    // Can be manually added by HR
    public bool IsCustom { get; set; } = false;

    // Set to true if an official holiday was edited by HR
    public bool IsModified { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
