using Microsoft.AspNetCore.Http;

namespace App.DTO.Public.v1.Identity;

public class EditProfilePictureRequest
{
    public IFormFile? ProfilePicture { get; set; }
}