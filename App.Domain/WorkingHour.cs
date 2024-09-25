using Base.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.Domain;

[Index(nameof(DayOfWeek), nameof(CompanyId), IsUnique = true)]
public class WorkingHour : AuditableEntity
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
}