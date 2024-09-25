using System.Net;

namespace App.DTO.Public.v1.Identity;

public class RestApiErrorResponse
{
    public HttpStatusCode Status { get; set; }
    public string Error { get; set; } = default!;
}