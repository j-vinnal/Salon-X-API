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
using MonthlyRevenue = App.DTO.BLL.MonthlyRevenue;

namespace WebApp.ApiControllers.Admin;

/// <summary>
///     ApiController for managing bookings.
/// </summary>
[ApiController]
[Area("Admin")]
[ApiVersion("1.0")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/v{version:apiVersion}/{area:exists}/[controller]")]
public class BookingsController : ApiControllerBase
{
    private readonly IAppBLL _bll;
    private readonly BookingMapper _mapper;
    private readonly MonthlyRevenueMapper _monthlyRevenueMapper;

    /// <summary>
    ///     Constructor for BookingsController.
    /// </summary>
    public BookingsController(IAppBLL bll, IMapper automapper)
    {
        _bll = bll;
        _mapper = new BookingMapper(automapper);
        _monthlyRevenueMapper = new MonthlyRevenueMapper(automapper);
    }

    /// <summary>
    ///     Get all bookings for the logged-in user.
    /// </summary>
    /// <returns>List of bookings.</returns>
    /// <response code="200">Returns the list of bookings.</response>
    /// <response code="400">If user identification fails.</response>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<Booking>), 200)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.BookingService.GetAllAsync(appUserId);
            var entityPublic = entityBll.Select(p => _mapper.Map(p)!).ToList();

            return Ok(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Get a booking by ID.
    /// </summary>
    /// <param name="id">The ID of the booking.</param>
    /// <returns>The booking with the specified ID.</returns>
    /// <response code="200">Returns the booking.</response>
    /// <response code="400">If user identification fails.</response>
    /// <response code="404">If the booking is not found.</response>
    [HttpGet("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Booking), 200)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 404)]
    public async Task<ActionResult<Booking>> GetBooking(Guid id)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var entityBll = await _bll.BookingService.FindAsync(id, appUserId);
            if (entityBll == null) return GenerateErrorResponse(HttpStatusCode.NotFound, "Booking not found.");

            var entityPublic = _mapper.Map(entityBll)!;
            return Ok(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Updates a booking by ID.
    /// </summary>
    /// <param name="id">The ID of the booking to update.</param>
    /// <param name="booking">The updated booking data.</param>
    /// <returns>The updated booking.</returns>
    /// <response code="204">Returns no content.</response>
    /// <response code="400">If user identification fails or invalid data is provided.</response>
    /// <response code="404">If the booking is not found.</response>
    [HttpPut("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 404)]
    public async Task<IActionResult> PutBooking(Guid id, Booking booking)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            if (id != booking.Id)
                return GenerateErrorResponse(HttpStatusCode.NotFound, "Booking not found.");

            var existingEntity = await _bll.BookingService.FindAsync(booking.Id, appUserId);
            if (existingEntity == null)
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "No hacking (bad user id)!");

            var entityBll = _mapper.Map(booking)!;
            _bll.BookingService.Update(entityBll);
            await _bll.SaveChangesAsync();

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Creates a new booking.
    /// </summary>
    /// <param name="booking">The booking data.</param>
    /// <returns>The created booking.</returns>
    /// <response code="201">Returns the created booking.</response>
    /// <response code="400">If user identification fails.</response>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Booking), 201)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    public async Task<ActionResult<Booking>> PostBooking([FromBody] Booking booking) 
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            // Convert the booking date to UTC, because docker
            booking.BookingDate = DateTime.SpecifyKind(booking.BookingDate, DateTimeKind.Utc);

            var entityBll = _mapper.Map(booking)!;

            var result = _bll.BookingService.Add(entityBll);
            await _bll.SaveChangesAsync();

            var entityPublic = _mapper.Map(result);
            return CreatedAtAction("GetBooking", new { id = entityPublic!.Id }, entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Deletes a booking by ID.
    /// </summary>
    /// <param name="id">The ID of the booking to delete.</param>
    /// <response code="204">If the booking is deleted successfully.</response>
    /// <response code="400">If user identification fails.</response>
    /// <response code="404">If the booking is not found.</response>
    [HttpDelete("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 404)]
    public async Task<IActionResult> DeleteBooking(Guid id)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var booking = await _bll.BookingService.FindAsync(id, appUserId);
            if (booking == null) return GenerateErrorResponse(HttpStatusCode.NotFound, "Booking not found.");

            await _bll.BookingService.RemoveAsync(id, appUserId);
            await _bll.SaveChangesAsync();
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Calculates the total turnover for all bookings of the logged-in user.
    ///     Optionally filters the turnover by a specific service.
    /// </summary>
    /// <param name="serviceId">Optional service ID to filter the turnover calculation.</param>
    /// <returns>The total turnover as a decimal value.</returns>
    /// <response code="200">Returns the total turnover.</response>
    /// <response code="400">If user identification fails or an error occurs.</response>
    [HttpGet("turnover")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    public async Task<ActionResult<decimal>> GetTotalTurnover(Guid? serviceId = null)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();
            var totalTurnover = await _bll.BookingService.CalculateTotalTurnoverAsync(appUserId, serviceId);
            return Ok(totalTurnover);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>
    ///     Calculates the monthly turnover for all bookings of the logged-in user.
    ///     Optionally filters the turnover by a specific service.
    /// </summary>
    /// <param name="serviceId">Optional service ID to filter the turnover calculation.</param>
    /// <returns>A list of monthly revenues.</returns>
    /// <response code="200">Returns the list of monthly revenues.</response>
    /// <response code="400">If user identification fails or an error occurs.</response>
    [HttpGet("monthly-turnover")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<App.DTO.Public.v1.MonthlyRevenue>), 200)]
    [ProducesResponseType(typeof(RestApiErrorResponse), 400)]
    public async Task<ActionResult<List<App.DTO.Public.v1.MonthlyRevenue>>> GetMonthlyTurnover(Guid? serviceId = null)
    {
        try
        {
            var appUserId = User.GetUserIdOrThrow();

            var monthlyTurnover = await _bll.BookingService.CalculateMonthlyTurnoverAsync(appUserId, serviceId);
            var entityPublic = monthlyTurnover.Select(mr => _monthlyRevenueMapper.Map(mr)).ToList();
            return Ok(entityPublic);
        }
        catch (UnauthorizedAccessException ex)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }
}