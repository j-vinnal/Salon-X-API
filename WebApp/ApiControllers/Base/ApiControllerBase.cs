using System.Net;
using App.DTO.Public.v1.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.ApiControllers.Base;

/// <summary>
///     Base controller providing common functionalities.
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    ///     Generates an error response with the specified HTTP status code and error message.
    /// </summary>
    /// <param name="code">The HTTP status code to return.</param>
    /// <param name="errorMessage">The error message to include in the response.</param>
    /// <returns>An ObjectResult containing the status code and error message.</returns>
    protected ObjectResult GenerateErrorResponse(HttpStatusCode code, string errorMessage)
    {
        return StatusCode((int)code, new RestApiErrorResponse
        {
            Status = code,
            Error = errorMessage
        });
    }
}