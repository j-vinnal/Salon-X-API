using Base.Contacts;

namespace App.DTO.Public.v1;

public class WorkingHour : IEntityId

{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CompanyId { get; set; }
    public Guid Id { get; set; }
}