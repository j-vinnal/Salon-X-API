using Base.Contacts;

namespace App.DTO.DAL;

public class WorkingHour : IEntityId

{
    public Guid Id { get; set; }
    
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; } = true;

        public Guid CompanyId { get; set; }
    
}