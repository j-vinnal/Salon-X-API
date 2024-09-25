namespace WebApp.ViewModels;

/// <summary>
/// ViewModel for displaying error information.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Gets or sets the request ID.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets a value indicating whether to show the request ID.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}