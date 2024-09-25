using App.DTO.Public.v1;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels;

/// <summary>
///     ViewModel for creating and editing a service.
/// </summary>
public class ServiceCreateEditViewModel
{
    /// <summary>
    ///     The service entity.
    /// </summary>
    public Service Service { get; set; } = default!;

    /// <summary>
    ///     Select list of companies.
    /// </summary>
    public SelectList? CompanySelectList { get; set; }
}