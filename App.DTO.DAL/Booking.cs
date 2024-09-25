using App.Domain.Enums;
using Base.Contacts;

namespace App.DTO.DAL;

public class Booking : IEntityId
{
    public Guid Id { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public EBookingStatus Status { get; set; } // 0=Pending, 1=Confirmed, 2=Cancelled

    public string? ClientName { get; set; }
    public Guid ClientId { get; set; }

    public string? ServiceName { get; set; }
    public Guid ServiceId { get; set; }

}