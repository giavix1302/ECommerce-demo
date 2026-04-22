using API.Common;
using API.RateLimit;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.Refresh;
using Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.JsonWebTokens;

namespace API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private const string RefreshTokenCookieName = "refreshToken";

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [EnableRateLimiting(RateLimitPolicy.AuthStrict)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand? command)
    {
        if (command is null)
            return BadRequest(ApiResponse<object?>.Fail("Request body is required."));

        var result = await _mediator.Send(command);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RegisterResult>.Ok(result, "Registration successful."));
    }

    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicy.AuthStrict)]
    public async Task<IActionResult> Login([FromBody] LoginCommand? command)
    {
        if (command is null)
            return BadRequest(ApiResponse<object?>.Fail("Request body is required."));

        var result = await _mediator.Send(command);

        SetRefreshTokenCookie(result.RefreshToken);

        var publicResult = new LoginResult(result.AccessToken, result.Email, result.FullName, result.Role);
        return Ok(ApiResponse<LoginResult>.Ok(publicResult));
    }

    [HttpPost("refresh")]
    [EnableRateLimiting(RateLimitPolicy.AuthNormal)]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(ApiResponse<object?>.Fail("Refresh token is missing."));

        var result = await _mediator.Send(new RefreshCommand(refreshToken));

        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(ApiResponse<RefreshPublicResult>.Ok(new RefreshPublicResult(result.AccessToken)));
    }

    [HttpPost("logout")]
    [Authorize]
    [EnableRateLimiting(RateLimitPolicy.AuthNormal)]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        DateTime? accessTokenExpiry = expClaim is not null
            ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime
            : null;

        if (!string.IsNullOrWhiteSpace(refreshToken))
            await _mediator.Send(new LogoutCommand(refreshToken, jti, accessTokenExpiry));

        Response.Cookies.Delete(RefreshTokenCookieName);
        return Ok(ApiResponse<object?>.Ok(null, "Logged out successfully."));
    }

    private void SetRefreshTokenCookie(string token)
    {
        Response.Cookies.Append(RefreshTokenCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }
}
