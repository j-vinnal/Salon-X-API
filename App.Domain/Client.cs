using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.Domain;


[Index(nameof(Email), IsUnique = true)]
public class Client : AuditableEntity
{
    [MaxLength(256)] public string FirstName { get; set; } = default!;
    [MaxLength(256)] public string LastName { get; set; } = default!;
    [MaxLength(256)] public string Email { get; set; } = default!;
    [MaxLength(256)] public string? PhoneNumber { get; set; }

    public ICollection<Booking>? Bookings { get; set; }
}