using System.Net;
using System.Net.Mime;
using App.Contracts.BLL;
using App.Contracts.BLL.Services;
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
///     ApiController for managing companies.
/// </summary>
[ApiController]
[Area("Admin")]
[ApiVersion("1.0")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/v{version:apiVersion}/{area:exists}/[controller]")]
public class CompaniesController : ApiControllerBase
{
    private readonly IAppBLL _bll;
    private readonly IImageUploadService _imageUploadService;
    private readonly CompanyMapper _mapper;


    /// <summary>
    ///     Constructor for CompaniesController.
    /// </summary>
    public CompaniesController(IAppBLL bll, IMapper automapper,
        IImageUploadService imageUploadService)
    {
        _bll = bll;
        _mapper = new CompanyMapper(automapper);
        _imageUploadService = imageUploadService;
    }

    /// <summary>
    ///     Get all companies for the logged-in user.
    /// </summary>
    /// <returns>List of companies.</returns>
    /// <response code="200">Returns the list of companies.</response>
    /// <response code="400">If user identification fails.</response>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<Company>), 200)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.CompanyService.GetAllAsync(appUserId);
            var entityPublic = entityBll.Select(p => _mapper.Map(p)!).ToList();

            return Ok(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Get a company by ID.
    /// </summary>
    /// <param name="id">The ID of the company.</param>
    /// <returns>The company with the specified ID.</returns>
    /// <response code="200">Returns the company.</response>
    /// <response code="400">If user identification fails.</response>
    /// <response code="404">If the company is not found.</response>
    [HttpGet("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Company), 200)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 404)]
    public async Task<ActionResult<Company>> GetCompany(Guid id)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.CompanyService.FindAsync(id, appUserId);
            if (entityBll == null) return GenerateErrorResponse(HttpStatusCode.NotFound, "Company not found.");

            var entityPublic = _mapper.Map(entityBll)!;

            return Ok(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Updates a company by ID.
    /// </summary>
    /// <param name="id">The ID of the company to update.</param>
    /// <param name="company">The updated company data.</param>
    /// <returns>The updated company.</returns>
    /// <response code="201">Returns the updated company.</response>
    /// <response code="400">If user identification fails or invalid data is provided.</response>
    /// <response code="404">If the company is not found.</response>
    [HttpPut("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Company), 201)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 404)]
    public async Task<ActionResult<Company>> PutCompany(Guid id, Company company)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            if (id != company.Id)
                return GenerateErrorResponse(HttpStatusCode.NotFound, "Company not found.");

            var existingEntity = await _bll.CompanyService.FindAsync(company.Id, appUserId);
            if (existingEntity == null)
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "No hacking (bad user id)!");


            //Delete orphaned image
            if (existingEntity.CompanyLogoPath != company.CompanyLogoPath &&
                !string.IsNullOrEmpty(existingEntity.CompanyLogoPath))
                _imageUploadService.DeleteImage(existingEntity.CompanyLogoPath);

            var entityBll = _mapper.Map(company)!;
            entityBll.AppUserId = appUserId;

            _bll.CompanyService.Update(entityBll);
            await _bll.SaveChangesAsync();

            return CreatedAtAction("GetCompany", new { id = company.Id }, company);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Creates a new company.
    /// </summary>
    /// <param name="company">The company data.</param>
    /// <returns>The created company.</returns>
    /// <response code="201">Returns the created company.</response>
    /// <response code="400">If user identification fails or the user already has a company.</response>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Company), 201)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    public async Task<ActionResult<Company>> PostCompany([FromBody] Company company)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            // Check if user already has a company
            var existingCompany = (await _bll.CompanyService.GetAllAsync(appUserId)).FirstOrDefault();
            if (existingCompany != null)
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "User already has a company");

            var entityBll = _mapper.Map(company)!;
            entityBll.AppUserId = appUserId;

            var result = _bll.CompanyService.Add(entityBll);
            await _bll.SaveChangesAsync();

            var entityPublic = _mapper.Map(result);

            return CreatedAtAction("GetCompany", new { id = entityPublic!.Id }, entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Deletes a company by ID.
    /// </summary>
    /// <param name="id">The ID of the company to delete.</param>
    /// <response code="204">If the company is deleted successfully.</response>
    /// <response code="400">If user identification fails.</response>
    /// <response code="404">If the company is not found.</response>
    [HttpDelete("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 404)]
    public async Task<IActionResult> DeleteCompany(Guid id)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();
            var company = await _bll.CompanyService.FindAsync(id, appUserId);
            if (company == null) return GenerateErrorResponse(HttpStatusCode.NotFound, "Company not found.");
            await _bll.CompanyService.RemoveAsync(id, appUserId);
            await _bll.SaveChangesAsync();
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }
}