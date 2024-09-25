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
///     ApiController for managing bookings.
/// </summary>
[ApiController]
[Area("Public")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/{area:exists}/[controller]")]
public class BookingsController : ApiControllerBase
{
    private readonly IAppBLL _bll;
    private readonly BookingMapper _mapper;


    /// <summary>
    ///     Constructor for BookingsController.
    /// </summary>
    public BookingsController(IAppBLL bll, IMapper automapper)
    {
        _bll = bll;
        _mapper = new BookingMapper(automapper);
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
        
        // Convert the booking date to UTC, because docker
        booking.BookingDate = DateTime.SpecifyKind(booking.BookingDate, DateTimeKind.Utc);
        
        var entityBll = _mapper.Map(booking)!;

        var result = _bll.BookingService.Add(entityBll);
        await _bll.SaveChangesAsync();

        var entityPublic = _mapper.Map(result);

        return Ok(entityPublic);
    }
}