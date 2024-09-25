using System.ComponentModel.DataAnnotations;

namespace App.DTO.Public.v1.Identity;

public class EditPasswordRequest
{
    [MaxLength(256)] public string Password { get; set; } = default!;

    [MaxLength(256)] public string CurrentPassword { get; set; } = default!;
}