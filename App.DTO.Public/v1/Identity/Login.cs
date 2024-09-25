using System.ComponentModel.DataAnnotations;

namespace App.DTO.Public.v1.Identity;

public class Login
{
    [StringLength(128, MinimumLength = 5, ErrorMessage = "Email length is incorrect")]
    public string Email { get; set; } = default!;

    public string Password { get; set; } = default!;
}