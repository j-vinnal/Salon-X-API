using System.ComponentModel.DataAnnotations;
using App.Domain.Identity;
using Base.Contracts.Domain;
using Base.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.Domain;

[Index(nameof(PublicUrl), IsUnique = true)]
public class Company : AuditableEntity, IDomainAppUser<AppUser>
{
    [MaxLength(256)] public string CompanyName { get; set; } = default!;

    [MaxLength(256)] public string? CompanyLogoPath { get; set; }

    [MaxLength(256)] public string PublicUrl { get; set; } = default!;

    public ICollection<Service>? Services { get; set; }
    public ICollection<WorkingHour>? WorkingHours { get; set; }

    public Guid AppUserId { get; set; }
    public AppUser? User { get; set; }
}