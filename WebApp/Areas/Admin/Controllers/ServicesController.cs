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
///     Controller for managing services.
/// </summary>
[Area("Admin")]
[Authorize(Roles = RoleConstants.Admin)]
public class ServicesController : Controller
{
    private readonly IAppBLL _bll;
    private readonly ServiceMapper _mapper;

    /// <summary>
    ///     Constructor for ServicesController.
    /// </summary>
    public ServicesController(IAppBLL bll, IMapper automapper)
    {
        _bll = bll;
        _mapper = new ServiceMapper(automapper);
    }

    /// <summary>
    ///     Get all services for the logged-in user.
    /// </summary>
    /// <returns>List of services.</returns>
    public async Task<IActionResult> Index()
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.ServiceService.GetAllAsync(appUserId);
            var entityPublic = entityBll.Select(p => _mapper.Map(p)!).ToList();

            return View(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    ///     Get a service by ID.
    /// </summary>
    /// <param name="id">The ID of the service.</param>
    /// <returns>The service with the specified ID.</returns>
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.ServiceService.FindAsync(id.Value, appUserId);
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
    ///     Show the create service form.
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var vm = new ServiceCreateEditViewModel
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
    ///     Create a new service.
    /// </summary>
    /// <param name="vm">The service data.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceCreateEditViewModel vm)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            if (ModelState.IsValid)
            {
                var entityBll = _mapper.Map(vm.Service)!;
                _bll.ServiceService.Add(entityBll);
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
    ///     Show the edit service form.
    /// </summary>
    /// <param name="id">The ID of the service to edit.</param>
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.ServiceService.FindAsync(id.Value, appUserId);
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
    ///     Update a service by ID.
    /// </summary>
    /// <param name="id">The ID of the service to update.</param>
    /// <param name="service">The updated service data.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Service service)
    {
        if (id != service.Id) return NotFound();

        var appUserId = User.GetUserIdOrThrow();

        try
        {
            if (!await _bll.ServiceService.ExistsAsync(service.Id, appUserId))
                return BadRequest("No hacking (bad user id)!");

            if (ModelState.IsValid)
            {
                var entityBll = _mapper.Map(service)!;
                _bll.ServiceService.Update(entityBll);
                await _bll.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] = new SelectList(await _bll.CompanyService.GetAllAsync(appUserId), "Id",
                "CompanyName", appUserId);
            return View(service);
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _bll.ServiceService.ExistsAsync(service.Id, appUserId))
                return NotFound();
            throw;
        }
    }

    /// <summary>
    ///     Show the delete service confirmation form.
    /// </summary>
    /// <param name="id">The ID of the service to delete.</param>
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.ServiceService.FindAsync(id.Value, appUserId);
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
    ///     Delete a service by ID.
    /// </summary>
    /// <param name="id">The ID of the service to delete.</param>
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.ServiceService.RemoveAsync(id, appUserId);
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