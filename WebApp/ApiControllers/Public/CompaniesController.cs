using System.Net.Mime;
using App.Contracts.BLL;
using App.DTO.Public.v1;
using App.Public.Mappers;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApp.ApiControllers.Base;

namespace WebApp.ApiControllers.Public;

/// <summary>
///     ApiController for managing companies.
/// </summary>
[ApiController]
[Area("Public")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/{area:exists}/[controller]")]
public class CompaniesController : ApiControllerBase
{
    private readonly IAppBLL _bll;

    private readonly CompanyMapper _mapper;
    private readonly CompanyWithServicesMapper _mapperWithServices;


    /// <summary>
    ///     Constructor for CompaniesController.
    /// </summary>
    public CompaniesController(IAppBLL bll, IMapper automapper)
    {
        _bll = bll;
        _mapper = new CompanyMapper(automapper);
        _mapperWithServices = new CompanyWithServicesMapper(automapper);
    }


    /// <summary>
    ///     Get a company by public URL.
    /// </summary>
    /// <param name="publicUrl">The public URL of the company.</param>
    /// <returns>The company with the specified public URL.</returns>
    /// <response code="200">Returns the company.</response>
    [HttpGet("{publicUrl}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(CompanyWithServices), 200)]
    public async Task<ActionResult<CompanyWithServices>> GetCompanyByPublicUrl(string publicUrl)
    {
        var entityBll = await _bll.CompanyService.GetByPublicUrlWithServicesAsync(publicUrl);
        var entityPublic = _mapperWithServices.Map(entityBll)!;
        return Ok(entityPublic);
    }
}