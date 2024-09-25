using System.Net;
using System.Net.Mime;
using App.Contracts.BLL;
using App.DTO.Public.v1;
using App.DTO.Public.v1.Identity;
using App.Public.Mappers;
using Asp.Versioning;
using AutoMapper;
using Base.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.ApiControllers.Base;

namespace WebApp.ApiControllers.Admin;

/// <summary>
///     ApiController for managing working hours.
/// </summary>
[ApiController]
[Area("Admin")]
[ApiVersion("1.0")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/v{version:apiVersion}/{area:exists}/[controller]")]
public class WorkingHoursController : ApiControllerBase
{
    private readonly IAppBLL _bll;
    private readonly WorkingHourMapper _mapper;

    /// <summary>
    ///     Constructor for WorkingHoursController.
    /// </summary>
    public WorkingHoursController(IAppBLL bll, IMapper automapper)
    {
        _bll = bll;
        _mapper = new WorkingHourMapper(automapper);
    }

    /// <summary>
    ///     Get all working hours for the logged-in user.
    /// </summary>
    /// <returns>List of working hours.</returns>
    /// <response code="200">Returns the list of working hours.</response>
    /// <response code="400">If user identification fails.</response>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<WorkingHour>), 200)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    public async Task<ActionResult<IEnumerable<WorkingHour>>> GetWorkingHours()
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.WorkingHourService.GetAllAsync(appUserId);
            var entityPublic = entityBll.Select(p => _mapper.Map(p)!).ToList();

            return Ok(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Get a working hour by ID.
    /// </summary>
    /// <param name="id">The ID of the working hour.</param>
    /// <returns>The working hour with the specified ID.</returns>
    /// <response code="200">Returns the working hour.</response>
    /// <response code="400">If user identification fails.</response>
    /// <response code="404">If the working hour is not found.</response>
    [HttpGet("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(WorkingHour), 200)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 404)]
    public async Task<ActionResult<WorkingHour>> GetWorkingHour(Guid id)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.WorkingHourService.FindAsync(id, appUserId);
            if (entityBll == null)
                return GenerateErrorResponse(HttpStatusCode.NotFound, "Working hour not found.");

            var entityPublic = _mapper.Map(entityBll)!;
            return Ok(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Updates a working hour by ID.
    /// </summary>
    /// <param name="id">The ID of the working hour to update.</param>
    /// <param name="workingHour">The updated working hour data.</param>
    /// <returns>The updated working hour.</returns>
    /// <response code="201">Returns the updated working hour.</response>
    /// <response code="400">If user identification fails or invalid data is provided.</response>
    /// <response code="404">If the working hour is not found.</response>
    [HttpPut("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(WorkingHour), 201)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 404)]
    public async Task<ActionResult<WorkingHour>> PutWorkingHour(Guid id, WorkingHour workingHour)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            if (id != workingHour.Id)
                return GenerateErrorResponse(HttpStatusCode.NotFound, "Working hour not found.");

            if (!await _bll.WorkingHourService.ExistsAsync(id, appUserId))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "No hacking (bad appUserId id)!");

            var entityBll = _mapper.Map(workingHour)!;
            var result = _bll.WorkingHourService.Update(entityBll);
            await _bll.SaveChangesAsync();

            var entityPublic = _mapper.Map(result);
            return CreatedAtAction("GetWorkingHour", new { id = entityPublic!.Id }, entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Creates a new working hour.
    /// </summary>
    /// <param name="workingHour">The working hour data.</param>
    /// <returns>The created working hour.</returns>
    /// <response code="201">Returns the created working hour.</response>
    /// <response code="400">If user identification fails or the company is invalid.</response>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(WorkingHour), 201)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    public async Task<ActionResult<WorkingHour>> PostWorkingHour(WorkingHour workingHour)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var company = await _bll.CompanyService.FindAsync(workingHour.CompanyId, appUserId);
            if (company == null || company.AppUserId != appUserId)
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "Invalid company.");

            var entityBll = _mapper.Map(workingHour)!;
            var result = _bll.WorkingHourService.Add(entityBll);
            await _bll.SaveChangesAsync();

            var entityPublic = _mapper.Map(result);
            return CreatedAtAction("GetWorkingHour", new { id = entityPublic!.Id }, entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }


    /// <summary>
    ///     Deletes a working hour by ID.
    /// </summary>
    /// <param name="id">The ID of the working hour to delete.</param>
    /// <response code="204">If the working hour is deleted successfully.</response>
    /// <response code="400">If user identification fails.</response>
    /// <response code="404">If the working hour is not found.</response>
    [HttpDelete("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 404)]
    public async Task<IActionResult> DeleteWorkingHour(Guid id)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            if (!await _bll.WorkingHourService.ExistsAsync(id, appUserId))
                return GenerateErrorResponse(HttpStatusCode.NotFound, "Working hour not found.");

            var entityBll = await _bll.WorkingHourService.RemoveAsync(id, appUserId);
            if (entityBll == null) return GenerateErrorResponse(HttpStatusCode.NotFound, "Working hour not found.");

            await _bll.SaveChangesAsync();
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }
}