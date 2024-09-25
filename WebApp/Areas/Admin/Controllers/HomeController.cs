using App.Contracts.DAL;
using App.Domain.Identity;
using Base.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Areas.Admin.Controllers;

/// <summary>
///     Controller for admin home operations.
/// </summary>
[Area("Admin")]
[Authorize(Roles = RoleConstants.Admin)]
public class HomeController : Controller
{
    private readonly IAppUOW _uow;
    private readonly UserManager<AppUser> _userManager;

    /// <summary>
    /// </summary>
    /// <param name="userManager"></param>
    /// <param name="uow"></param>
    public HomeController(UserManager<AppUser> userManager, IAppUOW uow)
    {
        _userManager = userManager;
        _uow = uow;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public IActionResult Index()
    {
        return View();
    }
}