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
///     ApiController for managing clients.
/// </summary>
[ApiController]
[Area("Admin")]
[ApiVersion("1.0")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/v{version:apiVersion}/{area:exists}/[controller]")]
public class ClientsController : ApiControllerBase
{
    private readonly IAppBLL _bll;
    private readonly ClientMapper _mapper;

    /// <summary>
    ///     Constructor for ClientsController.
    /// </summary>
    public ClientsController(IAppBLL bll, IMapper automapper)
    {
        _bll = bll;
        _mapper = new ClientMapper(automapper);
    }

    /// <summary>
    ///     Get all clients for the logged-in user.
    /// </summary>
    /// <returns>List of clients.</returns>
    /// <response code="200">Returns the list of clients.</response>
    /// <response code="400">If user identification fails.</response>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<Client>), 200)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    public async Task<ActionResult<IEnumerable<Client>>> GetClients()
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.ClientService.GetAllAsync(appUserId);
            var entityPublic = entityBll.Select(p => _mapper.Map(p)!).ToList();

            return Ok(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Get a client by ID.
    /// </summary>
    /// <param name="id">The ID of the client.</param>
    /// <returns>The client with the specified ID.</returns>
    /// <response code="200">Returns the client.</response>
    /// <response code="400">If user identification fails.</response>
    /// <response code="404">If the client is not found.</response>
    [HttpGet("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Client), 200)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 404)]
    public async Task<ActionResult<Client>> GetClient(Guid id)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.ClientService.FindAsync(id, appUserId);
            if (entityBll == null) return GenerateErrorResponse(HttpStatusCode.NotFound, "Client not found.");

            var entityPublic = _mapper.Map(entityBll)!;
            return Ok(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Updates a client by ID.
    /// </summary>
    /// <param name="id">The ID of the client to update.</param>
    /// <param name="client">The updated client data.</param>
    /// <returns>The updated client.</returns>
    /// <response code="204">Returns no content.</response>
    /// <response code="400">If user identification fails or invalid data is provided.</response>
    /// <response code="404">If the client is not found.</response>
    [HttpPut("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 404)]
    public async Task<IActionResult> PutClient(Guid id, Client client)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            if (id != client.Id)
                return GenerateErrorResponse(HttpStatusCode.NotFound, "Client not found.");

            var existingEntity = await _bll.ClientService.FindAsync(client.Id, appUserId);
            if (existingEntity == null)
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "No hacking (bad user id)!");

            var entityBll = _mapper.Map(client)!;
            _bll.ClientService.Update(entityBll);
            await _bll.SaveChangesAsync();

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Creates a new client.
    /// </summary>
    /// <param name="client">The client data.</param>
    /// <returns>The created client.</returns>
    /// <response code="201">Returns the created client.</response>
    /// <response code="400">If user identification fails.</response>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Client), 201)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    public async Task<ActionResult<Client>> PostClient(Client client)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var existingEntity = await _bll.ClientService.FindByEmailAsync(client.Email);
            if (existingEntity != null)
            {
                var existingClient = _mapper.Map(existingEntity);
                return Ok(existingClient);
            }

            var entityBll = _mapper.Map(client)!;
            var result = _bll.ClientService.Add(entityBll);
            await _bll.SaveChangesAsync();

            var entityPublic = _mapper.Map(result);
            return CreatedAtAction("GetClient", new { id = entityPublic!.Id }, entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Deletes a client by ID.
    /// </summary>
    /// <param name="id">The ID of the client to delete.</param>
    /// <response code="204">If the client is deleted successfully.</response>
    /// <response code="400">If user identification fails.</response>
    /// <response code="404">If the client is not found.</response>
    [HttpDelete("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 404)]
    public async Task<IActionResult> DeleteClient(Guid id)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var client = await _bll.ClientService.FindAsync(id, appUserId);
            if (client == null) return GenerateErrorResponse(HttpStatusCode.NotFound, "Client not found.");

            await _bll.ClientService.RemoveAsync(id, appUserId);
            await _bll.SaveChangesAsync();
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }
}