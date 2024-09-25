using Microsoft.AspNetCore.Http;

namespace App.Contracts.BLL.Services;

public interface IImageUploadService
{
    Task<string?> UploadImageAsync(IFormFile image);
    void DeleteImage(string filePath);
}