using API.Common;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.Refresh;
using Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Register([FromBody] RegisterCommand? command)
    {
        if (command is null)
            return BadRequest(ApiResponse<object?>.Fail("Request body is required."));

        var result = await _mediator.Send(command);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RegisterResult>.Ok(result, "Registration successful."));
    }

    [HttpPost("login")]
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
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (!string.IsNullOrWhiteSpace(refreshToken))
            await _mediator.Send(new LogoutCommand(refreshToken));

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
