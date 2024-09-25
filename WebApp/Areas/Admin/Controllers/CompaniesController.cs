using App.Contracts.BLL;
using App.DTO.Public.v1;
using App.Public.Mappers;
using AutoMapper;
using Base.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Areas.Admin.Controllers;

/// <summary>
///     Controller for managing companies.
/// </summary>
[Area("Admin")]
[Authorize(Roles = RoleConstants.Admin)]
public class CompaniesController : Controller
{
    private readonly IAppBLL _bll;
    private readonly CompanyMapper _mapper;

    /// <summary>
    ///     Constructor for CompaniesController.
    /// </summary>
    public CompaniesController(IAppBLL bll, IMapper automapper)
    {
        _bll = bll;
        _mapper = new CompanyMapper(automapper);
    }

    /// <summary>
    ///     Get all companies for the logged-in user.
    /// </summary>
    /// <returns>List of companies.</returns>
    public async Task<IActionResult> Index()
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.CompanyService.GetAllAsync(appUserId);
            var entityPublic = entityBll.Select(p => _mapper.Map(p)!).ToList();

            return View(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    ///     Get a company by ID.
    /// </summary>
    /// <param name="id">The ID of the company.</param>
    /// <returns>The company with the specified ID.</returns>
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.CompanyService.FindAsync(id.Value, appUserId);
            if (entityBll == null) return NotFound();

            var entityPublic = _mapper.Map(entityBll)!;

            return View(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    ///     Show the create company form.
    /// </summary>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    ///     Create a new company.
    /// </summary>
    /// <param name="company">The company data.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Company company)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            if (ModelState.IsValid)
            {
                var entityBll = _mapper.Map(company)!;
                entityBll.AppUserId = appUserId;

                _bll.CompanyService.Add(entityBll);
                await _bll.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(company);
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    ///     Show the edit company form.
    /// </summary>
    /// <param name="id">The ID of the company to edit.</param>
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.CompanyService.FindAsync(id.Value, appUserId);
            if (entityBll == null) return NotFound();

            var entityPublic = _mapper.Map(entityBll)!;

            return View(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    ///     Update a company by ID.
    /// </summary>
    /// <param name="id">The ID of the company to update.</param>
    /// <param name="company">The updated company data.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Company company)
    {
        if (id != company.Id) return NotFound();
        var appUserId = User.GetUserIdOrThrow();

        try
        {

            if (!await _bll.CompanyService.ExistsAsync(company.Id, appUserId))
                return BadRequest("No hacking (bad user id)!");

            if (ModelState.IsValid)
            {
                var entityBll = _mapper.Map(company)!;
                entityBll.AppUserId = appUserId;

                _bll.CompanyService.Update(entityBll);
                await _bll.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(company);
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {

            if (!await _bll.CompanyService.ExistsAsync(company.Id, appUserId))
                return NotFound();
            throw;
        }
    }

    /// <summary>
    ///     Show the delete company confirmation form.
    /// </summary>
    /// <param name="id">The ID of the company to delete.</param>
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.CompanyService.FindAsync(id.Value, appUserId);
            if (entityBll == null) return NotFound();

            var entityPublic = _mapper.Map(entityBll)!;

            return View(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    ///     Delete a company by ID.
    /// </summary>
    /// <param name="id">The ID of the company to delete.</param>
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.CompanyService.RemoveAsync(id, appUserId);
            if (entityBll == null) return NotFound();

            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}