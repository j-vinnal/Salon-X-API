using System.ComponentModel.DataAnnotations;

namespace App.DTO.Public.v1.Identity;

public class EditAccountRequest
{
    [MaxLength(256)] public string FirstName { get; set; } = default!;

    [MaxLength(256)] public string LastName { get; set; } = default!;

    [MaxLength(256)] public string Email { get; set; } = default!;

    public string? ProfilePicturePath { get; set; }
}