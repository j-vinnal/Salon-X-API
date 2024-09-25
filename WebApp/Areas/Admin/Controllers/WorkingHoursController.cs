using App.Contracts.BLL;
using App.DTO.Public.v1;
using App.Public.Mappers;
using AutoMapper;
using Base.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Areas.Admin.ViewModels;

namespace WebApp.Areas.Admin.Controllers;

/// <summary>
///     Controller for managing working hours.
/// </summary>
[Area("Admin")]
[Authorize(Roles = RoleConstants.Admin)]
public class WorkingHoursController : Controller
{
    private readonly IAppBLL _bll;
    private readonly WorkingHourMapper _mapper;

    /// <summary>
    ///     Constructor for WorkingHoursController.
    /// </summary>
    /// <param name="bll">Business logic layer interface.</param>
    /// <param name="automapper">AutoMapper interface.</param>
    public WorkingHoursController(IAppBLL bll, IMapper automapper)
    {
        _bll = bll;
        _mapper = new WorkingHourMapper(automapper);
    }

    /// <summary>
    ///     Get all working hours for the logged-in user.
    /// </summary>
    /// <returns>List of working hours.</returns>
    public async Task<IActionResult> Index()
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.WorkingHourService.GetAllAsync(appUserId);
            var entityPublic = entityBll.Select(p => _mapper.Map(p)!).ToList();

            return View(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    ///     Get a working hour by ID.
    /// </summary>
    /// <param name="id">The ID of the working hour.</param>
    /// <returns>The working hour with the specified ID.</returns>
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.WorkingHourService.FindAsync(id.Value, appUserId);
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
    ///     Show the create working hour form.
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var vm = new WorkingHourCreateEditViewModel
            {
                CompanySelectList = new SelectList(await _bll.CompanyService.GetAllAsync(appUserId), nameof(Company.Id),
                    nameof(Company.CompanyName))
            };

            return View(vm);
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    ///     Create a new working hour.
    /// </summary>
    /// <param name="vm">The working hour data.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkingHourCreateEditViewModel vm)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            if (ModelState.IsValid)
            {
                var entityBll = _mapper.Map(vm.WorkingHour)!;
                _bll.WorkingHourService.Add(entityBll);
                await _bll.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            vm.CompanySelectList = new SelectList(await _bll.CompanyService.GetAllAsync(appUserId), nameof(Company.Id),
                nameof(Company.CompanyName));
            return View(vm);
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    ///     Show the edit working hour form.
    /// </summary>
    /// <param name="id">The ID of the working hour to edit.</param>
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.WorkingHourService.FindAsync(id.Value, appUserId);
            if (entityBll == null) return NotFound();

            var entityPublic = _mapper.Map(entityBll)!;
            ViewData["CompanyId"] = new SelectList(await _bll.CompanyService.GetAllAsync(appUserId), "Id",
                "CompanyName", appUserId);
            return View(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    ///     Update a working hour by ID.
    /// </summary>
    /// <param name="id">The ID of the working hour to update.</param>
    /// <param name="workingHour">The updated working hour data.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, WorkingHour workingHour)
    {
        if (id != workingHour.Id) return NotFound();
        var appUserId = User.GetUserIdOrThrow();

        try
        {
            if (!await _bll.WorkingHourService.ExistsAsync(workingHour.Id, appUserId))
                return BadRequest("No hacking (bad user id)!");

            if (ModelState.IsValid)
            {
                var entityBll = _mapper.Map(workingHour)!;
                _bll.WorkingHourService.Update(entityBll);
                await _bll.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] = new SelectList(await _bll.CompanyService.GetAllAsync(appUserId), "Id",
                "CompanyName", appUserId);
            return View(workingHour);
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _bll.WorkingHourService.ExistsAsync(workingHour.Id, appUserId))
                return NotFound();
            throw;
        }
    }

    /// <summary>
    ///     Show the delete working hour confirmation form.
    /// </summary>
    /// <param name="id">The ID of the working hour to delete.</param>
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.WorkingHourService.FindAsync(id.Value, appUserId);
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
    ///     Delete a working hour by ID.
    /// </summary>
    /// <param name="id">The ID of the working hour to delete.</param>
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.WorkingHourService.RemoveAsync(id, appUserId);
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