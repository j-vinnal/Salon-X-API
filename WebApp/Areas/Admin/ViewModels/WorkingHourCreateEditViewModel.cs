using App.DTO.Public.v1;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels;

/// <summary>
///     ViewModel for creating and editing working hours.
/// </summary>
public class WorkingHourCreateEditViewModel
{
    /// <summary>
    ///     The working hour entity.
    /// </summary>
    public WorkingHour WorkingHour { get; set; } = default!;

    /// <summary>
    ///     Select list for companies.
    /// </summary>
    public SelectList CompanySelectList { get; set; } = default!;
}