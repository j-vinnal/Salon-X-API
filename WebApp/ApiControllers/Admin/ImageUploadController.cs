using App.Contracts.BLL.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.ApiControllers.Admin;

/// <summary>
///     Controller for handling image upload operations.
/// </summary>
[ApiController]
[Area("Admin")]
[ApiVersion("1.0")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/v{version:apiVersion}/{area:exists}/[controller]")]
public class ImageUploadController : ControllerBase
{
    private readonly IImageUploadService _imageUploadService;

    /// <summary>
    ///     Constructor for <see cref="ImageUploadController" />.
    /// </summary>
    /// <param name="imageUploadService">Service for handling image uploads.</param>
    /// v
    public ImageUploadController(IImageUploadService imageUploadService)
    {
        _imageUploadService = imageUploadService;
    }

    /// <summary>
    ///     Endpoint for uploading an image.
    /// </summary>
    /// <param name="image">The image file to upload.</param>
    /// <returns>Action result containing the path of the uploaded image.</returns>
    /// <response code="200">Returns the path of the uploaded image.</response>
    /// <response code="400">If the image is invalid or the upload failed.</response>
    [HttpPost]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile image)
    {
        var imagePath = await _imageUploadService.UploadImageAsync(image);
        if (imagePath == null) return BadRequest("Invalid image or upload failed.");

        return Ok(new { ImagePath = imagePath });
    }
}