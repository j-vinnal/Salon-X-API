using System.Net;
using System.Net.Mime;
using App.Contracts.BLL;
using App.DTO.Public.v1;
using App.DTO.Public.v1.Identity;
using App.Public.Mappers;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApp.ApiControllers.Base;

namespace WebApp.ApiControllers.Public;

/// <summary>
///     ApiController for managing clients.
/// </summary>
[ApiController]
[Area("Public")]
[ApiVersion("1.0")]
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
            return Ok(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }
}