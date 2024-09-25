using System.ComponentModel.DataAnnotations;

namespace App.DTO.Public.v1.Identity;

public class Register
{
    [StringLength(128, MinimumLength = 1, ErrorMessage = "Incorrect length")]
    public string Email { get; set; } = default!;

    [StringLength(128, MinimumLength = 1, ErrorMessage = "Incorrect length")]
    public string Password { get; set; } = default!;

    [StringLength(128, MinimumLength = 1, ErrorMessage = "Incorrect length")]
    public string FirstName { get; set; } = default!;

    [StringLength(128, MinimumLength = 1, ErrorMessage = "Incorrect length")]
    public string LastName { get; set; } = default!;
}