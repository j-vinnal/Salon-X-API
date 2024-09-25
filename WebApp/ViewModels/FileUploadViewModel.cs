namespace WebApp.ViewModels;

/// <summary>
/// ViewModel for handling file uploads.
/// </summary>
public class FileUploadViewModel
{
    /// <summary>
    /// Gets or sets the file to be uploaded.
    /// </summary>
    public IFormFile File { get; set; } = default!;
}