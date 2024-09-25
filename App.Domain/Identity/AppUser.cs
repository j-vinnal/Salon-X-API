using System.ComponentModel.DataAnnotations;
using Base.Contacts;
using Microsoft.AspNetCore.Identity;

namespace App.Domain.Identity;

public class AppUser : IdentityUser<Guid>, IEntityId
{
    [MaxLength(256)] public string FirstName { get; set; } = default!;

    [MaxLength(256)] public string LastName { get; set; } = default!;

    [MaxLength(256)] public string? ProfilePicturePath { get; set; }

    public ICollection<AppRefreshToken>? AppRefreshTokens { get; set; }
    public ICollection<Company>? Companies { get; set; }
}