using App.Contracts.BLL.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace App.BLL.Services;

public class ImageUploadService : IImageUploadService
{
    private readonly IWebHostEnvironment _env;

    public ImageUploadService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string?> UploadImageAsync(IFormFile image)
    {
        if (image == null || image.Length == 0) return null;

        var fileExtensions = new[] { ".png", ".jpg", ".bmp", ".gif" };
        if (!fileExtensions.Contains(Path.GetExtension(image.FileName))) return null;

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads");
        var filename = Guid.NewGuid() + "_" + Path.GetFileName(image.FileName);
        var filePath = Path.Combine(uploadDir, filename);

        await using (var stream = File.Create(filePath))
        {
            await image.CopyToAsync(stream);
        }

        // Return the relative URL path
        return Path.Combine("/uploads", filename).Replace("\\", "/");
    }

    public void DeleteImage(string filePath)
    {
        var uploadDir = Path.Combine(_env.WebRootPath, "uploads");
        var fullPath = Path.Combine(uploadDir, Path.GetFileName(filePath));

        if (File.Exists(fullPath)) File.Delete(fullPath);
    }
}