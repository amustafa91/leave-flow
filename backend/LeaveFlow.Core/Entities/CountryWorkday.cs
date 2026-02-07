using System.ComponentModel.DataAnnotations;

namespace LeaveFlow.Core.Entities;

public class CountryWorkday
{
    [Key]
    [MaxLength(2)]
    public string CountryCode { get; set; } = string.Empty; // ISO Code: AE, US, IN

    // Workdays (true = working day)
    public bool Sunday { get; set; }
    public bool Monday { get; set; }
    public bool Tuesday { get; set; }
    public bool Wednesday { get; set; }
    public bool Thursday { get; set; }
    public bool Friday { get; set; }
    public bool Saturday { get; set; }
}
