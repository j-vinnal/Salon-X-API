using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Base.Helpers;

public static class IdentityHelpers
{
    // this - extension method, User.GetUserId() is the same as IdentityHelpers.GetUserId(User)
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)) return userId;

        return null;
    }
    
    public static Guid GetUserIdOrThrow(this ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        if (userId == null)
        {
            throw new UnauthorizedAccessException("Unable to identify the user.");
        }
        return userId.Value;
    }


    public static string GenerateJwt(IEnumerable<Claim> claims, string key, string issuer, string audience,
        int expiresInSeconds)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddSeconds(expiresInSeconds);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expires,
            signingCredentials: signingCredentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static bool ValidateToken(string jwt, string key,
        string issuer, string audience, bool ignoreExpiration = true) // Retrieve token from request header
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = GetValidationParameters(key, issuer, audience);

        SecurityToken validatedToken;
        try
        {
            var principal = tokenHandler.ValidateToken(jwt, validationParameters, out validatedToken);
        }
        catch (SecurityTokenExpiredException)
        {
            // it's ok to be expired
            return ignoreExpiration;
        }
        catch (Exception)
        {
            // something else was wrong
            return false;
        }

        return true;
    }

    private static TokenValidationParameters GetValidationParameters(string key,
        string issuer, string audience)
    {
        return new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidIssuer = issuer,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true
        };
    }
}