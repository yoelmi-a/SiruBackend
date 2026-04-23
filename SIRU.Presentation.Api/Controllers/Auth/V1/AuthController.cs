using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIRU.Core.Application.Dtos.Auth;
using SIRU.Core.Application.Interfaces.Auth;
using SIRU.Core.Domain.Common.Pagination;
using SIRU.Presentation.Api.Handlers;
using System.Security.Claims;

namespace SIRU.Presentation.Api.Controllers.Auth.V1
{
    [ApiVersion("1.0")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService accountService)
        {
            _authService = accountService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthDto authRequest)
        {
            var userAgent = Request.Headers.UserAgent.ToString();
            var deviceInfo = ParseHandler.ParseUserAgent(userAgent);
            var authResponse = await _authService.AuthenticateAsync(authRequest, deviceInfo);

            return authResponse.Handle(HttpContext.Request.Path, value =>
            {
                Response.Cookies.Append("refresh-token", value.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    // Descomentar en prod
                    // Secure = true,
                    // SameSite = SameSiteMode.Strict,
                    Expires = value.RefreshTokenDurationInDays,
                    Path = "/api/v1/Auth"
                });

                return Ok(value.AccessToken);
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.JwtToken))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Detail = "El jwt token no fue proporcionado",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                });
            }

            var refreshToken = Request.Cookies["refresh-token"];

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Detail = "El refresh token no fue proporcionado",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                });
            }

            var userAgent = Request.Headers.UserAgent.ToString();
            var deviceInfo = ParseHandler.ParseUserAgent(userAgent);

            var refreshResponse = await _authService.RefreshTokensAsync(new RefreshRequestDto
            {
                AccessToken = request.JwtToken,
                DeviceInfo = deviceInfo,
                RefreshToken = refreshToken
            });

            return refreshResponse.Handle(HttpContext.Request.Path, value =>
            {
                return refreshResponse.Handle(HttpContext.Request.Path, value =>
                {
                    Response.Cookies.Append("refresh-token", value.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        // Descomentar en prod
                        // Secure = true,
                        // SameSite = SameSiteMode.Strict,
                        Expires = value.RefreshTokenDurationInDays,
                        Path = "/api/v1/Auth"
                    });

                    return Ok(value.AccessToken);
                });
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            var forgotPasswordResponse = await _authService.ForgotPasswordAsync(new ForgotPasswordDto
            {
                Email = request.Email,
                Origin = request.Origin
            });

            return forgotPasswordResponse.Handle(HttpContext.Request.Path, () =>
            {
                return NoContent();
            });

        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var resetPasswordResponse = await _authService.ResetPasswordAsync(resetPasswordDto);

            return resetPasswordResponse.Handle(HttpContext.Request.Path, () =>
            {
                return NoContent();
            });

        }

        [HttpPost("revoke")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RevokeSession([FromBody] RevokeAccessRequestDto revokeRequest)
        {
            var revokeResponse = await _authService.RevokeUserSessionAsync(revokeRequest, true);

            return revokeResponse.Handle(HttpContext.Request.Path, () =>
            {
                return NoContent();
            });

        }

        [HttpPost("me/revoke")]
        [Authorize]
        public async Task<IActionResult> RevokeSessionOfAuthorizedAccount([FromBody] RevokeSessionRequest request)
        {
            var accountId = User.FindFirstValue("uid");

            if (string.IsNullOrWhiteSpace(accountId))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "No se encontró el id de la cuenta activa en el token",
                    Status = StatusCodes.Status401Unauthorized,
                    Instance = HttpContext.Request.Path
                });
            }

            var revokeResponse = await _authService.RevokeUserSessionAsync(new RevokeAccessRequestDto
            {
                ActionMadeByAccountId = accountId,
                SessionId = request.SessionId
            });

            return revokeResponse.Handle(HttpContext.Request.Path, () =>
            {
                return NoContent();
            });

        }

        [HttpGet("account-{accountId}/sessions")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetActiveSessionsByAccountId(string accountId, [FromQuery] Pagination parameters)
        {
            var sessions = await _authService.GetAllActiveSessionsByAccountIdAsync(parameters, accountId);

            if (!sessions.Items.Any())
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = "No se encontraron sesiones activas de esta cuenta",
                    Status = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            }

            return Ok(sessions);
        }

        [HttpGet("me/sessions")]
        [Authorize]
        public async Task<IActionResult> GetActiveSessionsOfAuthorizedAccount([FromQuery] Pagination parameters)
        {
            var accountId = User.FindFirstValue("uid");

            if (string.IsNullOrWhiteSpace(accountId))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "No se encontró el id de la cuenta activa en el token",
                    Status = StatusCodes.Status401Unauthorized,
                    Instance = HttpContext.Request.Path
                });
            }

            var sessions = await _authService.GetAllActiveSessionsByAccountIdAsync(parameters, accountId);

            if (!sessions.Items.Any())
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = "No se encontraron sesiones activas de esta cuenta",
                    Status = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            }

            return Ok(sessions);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refresh-token"];

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Detail = "El refresh token no fue proporcionado",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                });
            }
            
            var logOutResponse = await _authService.LogOutAsync(refreshToken);

            return logOutResponse.Handle(HttpContext.Request.Path, () =>
            {
                Response.Cookies.Append("refresh-token", "", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(-1),
                    Path = "api/v1/Auth/refresh"
                });

                return Ok();
            });
        }
    }
}
