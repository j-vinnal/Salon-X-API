using System.ComponentModel.DataAnnotations;
using App.Domain.Enums;
using Base.Domain;

namespace App.Domain;

public class Service : AuditableEntity
{
    [MaxLength(256)] public string ServiceName { get; set; } = default!;
    [MaxLength(1024)] public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public int Duration { get; set; } // Duration in minutes

    public EServiceStatus Status { get; set; } = EServiceStatus.Active;

    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public ICollection<Booking>? Bookings { get; set; }
}