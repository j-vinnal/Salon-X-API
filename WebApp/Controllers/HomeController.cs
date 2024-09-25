using System.Diagnostics;
using App.DAL.EF;
using Base.Helpers;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.ViewModels;

namespace WebApp.Controllers;

/// <summary>
/// Controller for handling home-related actions.
/// </summary>
public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="context">The database context.</param>
    /// <param name="env">The web host environment.</param>
    public HomeController(ILogger<HomeController> logger, AppDbContext context, IWebHostEnvironment env)
    {
        _logger = logger;
        _context = context;
        _env = env;
    }

    /// <summary>
    /// Test method that returns a string with the provided parameter.
    /// </summary>
    /// <param name="bar">The string parameter.</param>
    /// <returns>A string with the provided parameter.</returns>
    public string TestAlpha(string bar)
    {
        return "string " + bar;
    }

    /// <summary>
    /// Test method that returns a string with the provided integer parameter.
    /// </summary>
    /// <param name="bar">The integer parameter.</param>
    /// <returns>A string with the provided integer parameter.</returns>
    public string Test(int bar)
    {
        return "int " + bar;
    }

    /// <summary>
    /// Displays the home page.
    /// </summary>
    /// <returns>The home page view.</returns>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Displays the list of files.
    /// </summary>
    /// <returns>The list files view.</returns>
    public IActionResult ListFiles()
    {
        return View();
    }

    /// <summary>
    /// Displays the upload file page.
    /// </summary>
    /// <returns>The upload file view.</returns>
    public IActionResult Upload()
    {
        return View();
    }

    /// <summary>
    /// Handles the file upload.
    /// </summary>
    /// <param name="vm">The file upload view model.</param>
    /// <returns>The result of the upload action.</returns>
    [HttpPost]
    public async Task<IActionResult> Upload(FileUploadViewModel vm)
    {
        var fileExtensions = new[]
        {
            ".png", ".jpg", ".bmp", ".gif"
        };

        if (ModelState.IsValid)
        {
            if (vm.File.Length > 0 && fileExtensions.Contains(Path.GetExtension(vm.File.FileName)))
            {
                var uploadDir = _env.WebRootPath;

                // Save the record and then the file name will be the same GUID as the record ID
                var filename = Guid.NewGuid() + "_" + Path.GetFileName(vm.File.FileName);
                var filePath = uploadDir + Path.DirectorySeparatorChar + "uploads" + Path.DirectorySeparatorChar +
                               filename;

                await using (var stream = System.IO.File.Create(filePath))
                {
                    await vm.File.CopyToAsync(stream);
                }

                return RedirectToAction(nameof(ListFiles));
            }

            ModelState.AddModelError(nameof(FileUploadViewModel.File), "This is not an image file! " + vm.File.FileName);
        }

        return View(vm);
    }

    /// <summary>
    /// Displays the privacy page.
    /// </summary>
    /// <returns>The privacy page view.</returns>
    public async Task<IActionResult> Privacy()
    {
        var vm = new HomePrivacyVM();

        if (User.Identity == null) return View(vm);

        vm.AppUser = await _context.Users
            .Include(u => u.AppRefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == User.GetUserId());
        vm.AppUserClaims = await _context.UserClaims.Where(u => u.UserId == User.GetUserId()).ToListAsync();

        return View(vm);
    }

    /// <summary>
    /// Displays the error page.
    /// </summary>
    /// <returns>The error page view.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    /// <summary>
    /// Sets the language for the application.
    /// </summary>
    /// <param name="culture">The culture to set.</param>
    /// <param name="returnUrl">The URL to return to after setting the language.</param>
    /// <returns>A redirect to the return URL.</returns>
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(culture)
            ),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(2)
            }
        );
        return Redirect(returnUrl);
    }
}