using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mime;
using System.Security.Claims;
using App.Contracts.BLL.Services;
using App.DAL.EF;
using App.Domain.Identity;
using App.DTO.Public.v1.Identity;
using Asp.Versioning;
using Base.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.ApiControllers.Base;

namespace WebApp.ApiControllers.Identity;

/// <summary>
///     Controller for user account related operations.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/identity/[controller]/[action]")]
public class AccountController : ApiControllerBase
{
    private const int RefreshTokenExpirationDays = 7;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly IImageUploadService _imageUploadService;
    private readonly ILogger<AccountController> _logger;
    private readonly Random _rnd = new();
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;


    /// <summary>
    ///     Constructor for AccountController.
    /// </summary>
    /// <param name="signInManager"></param>
    /// <param name="userManager"></param>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
    /// <param name="context">The database context.</param>
    /// <param name="imageUploadService"></param>
    public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager,
        IConfiguration configuration, ILogger<AccountController> logger, AppDbContext context,
        IImageUploadService imageUploadService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
        _context = context;
        _imageUploadService = imageUploadService;
    }

    /// <summary>
    ///     Registers a new user into the system.
    /// </summary>
    /// <param name="registrationData">User info.</param>
    /// <param name="expiresInSeconds">Optional, override default value.</param>
    /// <returns>JWTResponse with jwt and refresh token.</returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(JWTResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]

    //[FromQuery] means that parameter is taken from URL
    public async Task<ActionResult<JWTResponse>> Register([FromBody] Register registrationData,
        [FromQuery] int expiresInSeconds)
    {
        if (expiresInSeconds <= 0) expiresInSeconds = int.MaxValue;
        expiresInSeconds = expiresInSeconds < _configuration.GetValue<int>("JWT:expiresInSeconds")
            ? expiresInSeconds
            : _configuration.GetValue<int>("JWT:expiresInSeconds");

        // is user already registered?
        var appUser = await _userManager.FindByEmailAsync(registrationData.Email);
        if (appUser != null)
        {
            _logger.LogWarning("User with email {} is already registered", registrationData.Email);
            return GenerateErrorResponse(HttpStatusCode.BadRequest,
                $"User {registrationData.Email} is already registered");
        }

        // if not, create new user
        var refreshToken = new AppRefreshToken();


        appUser = new AppUser
        {
            Email = registrationData.Email,
            FirstName = registrationData.FirstName,
            LastName = registrationData.LastName,
            UserName = registrationData.Email,
            AppRefreshTokens = new List<AppRefreshToken> { refreshToken }
        };

        refreshToken.User = appUser;

        var result = await _userManager.CreateAsync(appUser, registrationData.Password);

        if (!result.Succeeded)
        {
            _logger.LogError("User creation failed: {ErrorMessage}", result.Errors.First().Description);
            return GenerateErrorResponse(HttpStatusCode.BadRequest,
                $"User registration failed: {result.Errors.First().Description}");
        }


        var addToRoleResult = await _userManager.AddToRoleAsync(appUser, RoleConstants.Admin);

        if (!addToRoleResult.Succeeded)
        {
            _logger.LogError("Failed to assign admin role to user: {ErrorMessage}",
                addToRoleResult.Errors.First().Description);
            return GenerateErrorResponse(HttpStatusCode.BadRequest,
                $"Failed to assign role to the user: {addToRoleResult.Errors.First().Description}");
        }


        // save into claims
        var res = await _userManager.AddClaimsAsync(appUser, new List<Claim>
        {
            new(ClaimTypes.GivenName, appUser.FirstName),
            new(ClaimTypes.Surname, appUser.LastName)
        });


        if (!res.Succeeded)
            return GenerateErrorResponse(HttpStatusCode.BadRequest,
                $"Failed to add claims to the user: {res.Errors.First().Description}");


        // get full user from system with fixed data (maybe there is something generated by identity that we might need
        appUser = await _userManager.FindByEmailAsync(appUser.Email);
        if (appUser == null)
        {
            _logger.LogWarning("User with email {} is not found after registration", registrationData.Email);
            return GenerateErrorResponse(HttpStatusCode.InternalServerError,
                "User could not be found after registration");
        }

        // create JWT
        var response = await GenerateJwtResponse(appUser, expiresInSeconds);
        return Ok(response);
    }


    /// <summary>
    ///     Logs in a user and provides them with a JWT and Refresh token.
    /// </summary>
    /// <param name="loginData">User's email and password.</param>
    /// <param name="expiresInSeconds">Optional, override default value.</param>
    /// <returns>JWTResponse with jwt and refresh token.</returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(JWTResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JWTResponse>> LogIn([FromBody] Login loginData, [FromQuery] int expiresInSeconds)
    {
        if (expiresInSeconds <= 0) expiresInSeconds = int.MaxValue;
        expiresInSeconds = expiresInSeconds < _configuration.GetValue<int>("JWT:expiresInSeconds")
            ? expiresInSeconds
            : _configuration.GetValue<int>("JWT:expiresInSeconds");

        // Verify email address
        var appUser = await _userManager.FindByEmailAsync(loginData.Email);
        if (appUser == null)
        {
            _logger.LogWarning("WebApi login failed, email {} not found", loginData.Email);

            // Brute-force attack mitigation
            await Task.Delay(_rnd.Next(100, 1000));
            return GenerateErrorResponse(HttpStatusCode.BadRequest,
                "No account found with the provided email address");
        }

        // Verify password
        var result = await _signInManager.CheckPasswordSignInAsync(appUser, loginData.Password, false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("WebApi login failed. Incorrect password for user: {LoginDataEmail}", loginData.Email);
            await Task.Delay(_rnd.Next(100, 1000));
            return GenerateErrorResponse(HttpStatusCode.BadRequest,
                "The password you entered is incorrect");
        }

        // Get claims-based user
        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(appUser);


        if (claimsPrincipal == null)
        {
            _logger.LogWarning("Could not get ClaimsPrincipal for user: {LoginDataEmail}", loginData.Email);

            //time delay to prevent brute force attack
            await Task.Delay(_rnd.Next(100, 1000));
            return GenerateErrorResponse(HttpStatusCode.BadRequest,
                "Failed to generate user session");
        }

        // Remove expired tokens directly in the database
        await _context.AppRefreshTokens
            .Where(t => t.AppUserId == appUser.Id &&
                        t.ExpirationDt < DateTime.UtcNow &&
                        (t.PreviousExpirationDt == null || t.PreviousExpirationDt < DateTime.UtcNow))
            .ExecuteDeleteAsync();

        var refreshToken = new AppRefreshToken
        {
            AppUserId = appUser.Id
        };

        _context.AppRefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Generate JWT
        var response = await GenerateJwtResponse(appUser, expiresInSeconds);
        return Ok(response);
    }

    /// <summary>
    ///     Provides a new JWT and refresh token to a user given their current JWT and refresh token.
    /// </summary>
    /// <param name="refreshTokenModel">Contains the user's current JWT and refresh token.</param>
    /// <param name="expiresInSeconds">Optional, override default value.</param>
    /// <returns>A new JWTResponse with jwt and refresh token.</returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(JWTResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RefreshToken(
        [FromBody] RefreshTokenModel refreshTokenModel,
        [FromQuery] int expiresInSeconds)
    {
        if (expiresInSeconds <= 0) expiresInSeconds = int.MaxValue;

        JwtSecurityToken jwtToken;
        // Not verify, but just deserialize, create jwt object
        try
        {
            jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(refreshTokenModel.Jwt);
            if (jwtToken == null) return GenerateErrorResponse(HttpStatusCode.BadRequest, "No token");
        }
        catch (Exception e)
        {
            return GenerateErrorResponse(HttpStatusCode.BadRequest, $"Cant parse the token, {e.Message}");
        }

        if (!IdentityHelpers.ValidateToken(
                refreshTokenModel.Jwt,
                _configuration.GetValue<string>("JWT:Key") ?? throw new InvalidOperationException(),
                _configuration.GetValue<string>("JWT:Issuer") ?? throw new InvalidOperationException(),
                _configuration.GetValue<string>("JWT:Audience") ?? throw new InvalidOperationException()
            ))
            return GenerateErrorResponse(HttpStatusCode.BadRequest, "JWT validation fail");

        var userEmail = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        if (userEmail == null) return GenerateErrorResponse(HttpStatusCode.BadRequest, "No email in jwt");

        // get user and tokens
        var appUser = await _userManager.FindByEmailAsync(userEmail);
        if (appUser == null)
            return GenerateErrorResponse(HttpStatusCode.BadRequest, $"User with email {userEmail} not found");


        // load and compare refresh tokens
        appUser.AppRefreshTokens = await _context.Entry(appUser).Collection(a => a.AppRefreshTokens!)
            .Query()
            .Where(refTok =>
                (refTok.RefreshToken == refreshTokenModel.RefreshToken && refTok.ExpirationDt > DateTime.UtcNow)
                ||
                (refTok.PreviousRefreshToken == refreshTokenModel.RefreshToken &&
                 refTok.PreviousExpirationDt > DateTime.UtcNow)
            )
            .ToListAsync();


        if (appUser.AppRefreshTokens == null || appUser.AppRefreshTokens.Count == 0)
            return GenerateErrorResponse(HttpStatusCode.BadRequest,
                $"RefreshTokens collection is null or empty - {appUser.AppRefreshTokens?.Count}");

        if (appUser.AppRefreshTokens.Count != 1)
            return GenerateErrorResponse(HttpStatusCode.BadRequest, "More than one valid refresh token found");

        // get claims based user
        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(appUser);
        if (claimsPrincipal == null)
        {
            _logger.LogWarning("Could not get ClaimsPrincipal for user {}", userEmail);
            return GenerateErrorResponse(HttpStatusCode.BadRequest, "User/Password problem");
        }

        // generate jwt
        var jwt = GenerateJwt(claimsPrincipal.Claims, expiresInSeconds);


        // make new refresh token, keep old one still valid for some time
        var refreshToken = appUser.AppRefreshTokens.First();

        //if first time, only then make new token
        if (refreshToken.RefreshToken == refreshTokenModel.RefreshToken)
        {
            refreshToken.PreviousRefreshToken = refreshToken.RefreshToken;
            refreshToken.PreviousExpirationDt = DateTime.UtcNow.AddMinutes(1);

            refreshToken.RefreshToken =
                Guid.NewGuid().ToString();
            refreshToken.ExpirationDt = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays);

            await _context.SaveChangesAsync();
        }

        var res = new JWTResponse
        {
            JWT = jwt,
            RefreshToken = refreshToken.RefreshToken
        };

        return Ok(res);
    }

    /// <summary>
    ///     Logs out the user by invalidating their refresh token.
    ///     The user's JWT will still be valid until it expires.
    /// </summary>
    /// <param name="logout">The user's refresh token.</param>
    /// <returns>A success status if the refresh token was invalidated successfully.</returns>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Logout(
        [FromBody] Logout logout)
    {
        // delete the refresh token - so user is kicked out after jwt expiration
        // We do not invalidate the jwt - that would require pipeline modification and checking against db on every request
        // so client can actually continue to use the jwt until it expires (keep the jwt expiration time short ~1 min)

        var userId = User.GetUserId();

        if (userId == null) return GenerateErrorResponse(HttpStatusCode.BadRequest, "User not found");

        var appUser = await _userManager.FindByIdAsync(userId.ToString()!);

        if (appUser == null) return GenerateErrorResponse(HttpStatusCode.NotFound, "User not found");

        // Remove the refresh tokens directly in the database
        var deleteCount = await _context.AppRefreshTokens
            .Where(x => x.AppUserId == appUser.Id &&
                        (x.RefreshToken == logout.RefreshToken || x.PreviousRefreshToken == logout.RefreshToken))
            .ExecuteDeleteAsync();

        return Ok(new { TokenDeleteCount = deleteCount });
    }


    /// <summary>
    ///     Edits the account details of the logged-in user.
    /// </summary>
    /// <param name="id">The ID of the user to update.</param>
    /// <param name="request">The new account details.</param>
    /// <returns>A JWTResponse with the updated JWT and refresh token.</returns>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPut("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(JWTResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> EditAccount(Guid id, EditAccountRequest request)
    {
        //Included AppRefreshTokens for JWTResponse
        var userId = User.GetUserId();

        if (id != userId)
            return GenerateErrorResponse(HttpStatusCode.NotFound, "User not found.");

        var appUser = await _userManager.Users
            .Include(u => u.AppRefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (appUser == null) return GenerateErrorResponse(HttpStatusCode.NotFound, "User not found");

        // Check if another user with the same email already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null && existingUser.Id != userId)
            return GenerateErrorResponse(HttpStatusCode.BadRequest, "Email is already in use by another user");

        // Update only if values are different
        var isUpdated = false;
        if (appUser.FirstName != request.FirstName)
        {
            appUser.FirstName = request.FirstName;
            isUpdated = true;
        }

        if (appUser.LastName != request.LastName)
        {
            appUser.LastName = request.LastName;
            isUpdated = true;
        }

        if (appUser.Email != request.Email)
        {
            appUser.Email = request.Email;
            appUser.UserName = request.Email;
            isUpdated = true;
        }

        if (appUser.ProfilePicturePath != request.ProfilePicturePath)
        {
            DeleteOrphanedProfilePicture(appUser.ProfilePicturePath, request.ProfilePicturePath);
            appUser.ProfilePicturePath = request.ProfilePicturePath;
            isUpdated = true;
        }

        if (isUpdated)
        {
            var result = await _userManager.UpdateAsync(appUser);
            if (!result.Succeeded) return GenerateErrorResponse(HttpStatusCode.BadRequest, "Failed to update user");

            // Update user claims
            var currentClaims = await _userManager.GetClaimsAsync(appUser);
            var givenNameClaim = currentClaims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
            var surnameClaim = currentClaims.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
            var profilePictureClaim = currentClaims.FirstOrDefault(c => c.Type == "ProfilePicturePath");

            if (givenNameClaim != null) await _userManager.RemoveClaimAsync(appUser, givenNameClaim);

            if (surnameClaim != null) await _userManager.RemoveClaimAsync(appUser, surnameClaim);

            if (profilePictureClaim != null && !string.IsNullOrEmpty(appUser.ProfilePicturePath))
                await _userManager.RemoveClaimAsync(appUser, profilePictureClaim);

            var claimsToAdd = new List<Claim>
            {
                new(ClaimTypes.GivenName, appUser.FirstName),
                new(ClaimTypes.Surname, appUser.LastName)
            };

            if (!string.IsNullOrEmpty(appUser.ProfilePicturePath))
                claimsToAdd.Add(new Claim("ProfilePicturePath", appUser.ProfilePicturePath));

            await _userManager.AddClaimsAsync(appUser, claimsToAdd);
        }

        // Generate new JWT
        var response = await GenerateJwtResponse(appUser, int.MaxValue);

        return Ok(response);
    }

    /// <summary>
    ///     Edits the password of the logged-in user.
    /// </summary>
    /// <param name="id">The ID of the user to update.</param>
    /// <param name="editPasswordRequest">The new password details.</param>
    /// <returns>A JWTResponse with the updated JWT and refresh token.</returns>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPut("{id:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(JWTResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> EditPassword(Guid id, EditPasswordRequest editPasswordRequest)
    {
        var userId = User.GetUserId();

        if (id != userId)
            return GenerateErrorResponse(HttpStatusCode.NotFound, "User not found.");


        var appUser = await _userManager.Users
            .Include(u => u.AppRefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (appUser == null) return GenerateErrorResponse(HttpStatusCode.NotFound, "User not found");

        // Change password
        if (!string.IsNullOrEmpty(editPasswordRequest.Password) &&
            !string.IsNullOrEmpty(editPasswordRequest.CurrentPassword))
        {
            var passwordChangeResult = await _userManager.ChangePasswordAsync(appUser,
                editPasswordRequest.CurrentPassword, editPasswordRequest.Password);
            if (!passwordChangeResult.Succeeded)
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "Failed to update password");
        }

        // Generate new JWT
        var response = await GenerateJwtResponse(appUser, int.MaxValue);

        return Ok(response);
    }


    private string GenerateJwt(IEnumerable<Claim> claims, int expiresInSeconds)
    {
        return IdentityHelpers.GenerateJwt(
            claims,
            _configuration.GetValue<string>("JWT:Key") ?? throw new InvalidOperationException(),
            _configuration.GetValue<string>("JWT:Issuer") ?? throw new InvalidOperationException(),
            _configuration.GetValue<string>("JWT:Audience") ?? throw new InvalidOperationException(),
            expiresInSeconds < _configuration.GetValue<int>("JWT:ExpiresInSeconds")
                ? expiresInSeconds
                : _configuration.GetValue<int>("JWT:ExpiresInSeconds")
        );
    }

    private async Task<JWTResponse> GenerateJwtResponse(AppUser appUser, int expiresInSeconds)
    {
        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(appUser);
        var jwt = GenerateJwt(claimsPrincipal.Claims, expiresInSeconds);
        var refreshToken = (appUser.AppRefreshTokens ?? throw new InvalidOperationException()).First();

        var response = new JWTResponse
        {
            JWT = jwt,
            RefreshToken = refreshToken.RefreshToken
        };

        return response;
    }

    private void DeleteOrphanedProfilePicture(string? oldPath, string? newPath)
    {
        if (!string.IsNullOrEmpty(oldPath) && oldPath != newPath) _imageUploadService.DeleteImage(oldPath);
    }
}