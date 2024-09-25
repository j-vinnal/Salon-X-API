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
///     ApiController for managing services.
/// </summary>
[ApiController]
[Area("Admin")]
[ApiVersion("1.0")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/v{version:apiVersion}/{area:exists}/[controller]")]
public class ServicesController : ApiControllerBase
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
    /// <response code="200">Returns the list of services.</response>
    /// <response code="400">If user identification fails.</response>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<Service>), 200)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    public async Task<ActionResult<IEnumerable<Service>>> GetServices()
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.ServiceService.GetAllAsync(appUserId);
            var entityPublic = entityBll.Select(p => _mapper.Map(p)!).ToList();

            return Ok(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Get a service by ID.
    /// </summary>
    /// <param name="id">The ID of the service.</param>
    /// <returns>The service with the specified ID.</returns>
    /// <response code="200">Returns the service.</response>
    /// <response code="400">If user identification fails.</response>
    /// <response code="404">If the service is not found.</response>
    [HttpGet("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Service), 200)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 404)]
    public async Task<ActionResult<Service>> GetService(Guid id)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.ServiceService.FindAsync(id, appUserId);
            if (entityBll == null) return GenerateErrorResponse(HttpStatusCode.NotFound, "Service not found.");

            var entityPublic = _mapper.Map(entityBll)!;
            return Ok(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Updates a service by ID.
    /// </summary>
    /// <param name="id">The ID of the service to update.</param>
    /// <param name="service">The updated service data.</param>
    /// <returns>The updated service.</returns>
    /// <response code="201">Returns the updated service.</response>
    /// <response code="400">If user identification fails or invalid data is provided.</response>
    /// <response code="404">If the service is not found.</response>
    [HttpPut("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Service), 201)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 404)]
    public async Task<ActionResult<Service>> PutService(Guid id, Service service)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            if (id != service.Id)
                return GenerateErrorResponse(HttpStatusCode.NotFound, "Service not found.");

            if (!await _bll.ServiceService.ExistsAsync(service.Id, appUserId))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "No hacking (bad appUserId)!");

            var entityBll = _mapper.Map(service)!;
            var result = _bll.ServiceService.Update(entityBll);
            await _bll.SaveChangesAsync();

            var entityPublic = _mapper.Map(result);
            return CreatedAtAction("GetService", new { id = entityPublic!.Id }, entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Creates a new service.
    /// </summary>
    /// <param name="service">The service data.</param>
    /// <returns>The created service.</returns>
    /// <response code="201">Returns the created service.</response>
    /// <response code="400">If user identification fails.</response>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Service), 201)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    public async Task<ActionResult<Service>> PostService(Service service)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = _mapper.Map(service)!;

            var company = await _bll.CompanyService.FindAsync(service.CompanyId, appUserId);
            if (company == null || company.AppUserId != appUserId)
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "Invalid company.");

            var result = _bll.ServiceService.Add(entityBll);
            await _bll.SaveChangesAsync();

            var entityPublic = _mapper.Map(result);
            return CreatedAtAction("GetService", new { id = entityPublic!.Id }, entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Deletes a service by ID.
    /// </summary>
    /// <param name="id">The ID of the service to delete.</param>
    /// <response code="204">If the service is deleted successfully.</response>
    /// <response code="400">If user identification fails.</response>
    /// <response code="404">If the service is not found.</response>
    [HttpDelete("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 404)]
    public async Task<IActionResult> DeleteService(Guid id)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            if (!await _bll.ServiceService.ExistsAsync(id, appUserId))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "Service not found.");

            var entityBll = await _bll.ServiceService.RemoveAsync(id, appUserId);
            if (entityBll == null) return GenerateErrorResponse(HttpStatusCode.NotFound, "Service not found.");

            await _bll.SaveChangesAsync();
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }
}