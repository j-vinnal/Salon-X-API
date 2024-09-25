using App.Domain.Enums;
using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class Booking : AuditableEntity
{
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public EBookingStatus Status { get; set; } // 0=Pending, 1=Confirmed, 2=Cancelled
    
    public Guid ClientId { get; set; }
    public Client? Client { get; set; }

    public Guid ServiceId { get; set; }
    public Service? Service { get; set; }
}