using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIRU.Core.Application.Interfaces.Users;
using SIRU.Core.Domain.Common;
using SIRU.Presentation.Api.Handlers;
using System.Security.Claims;

namespace SIRU.Presentation.Api.Controllers.Users.V1
{
    [ApiVersion("1.0")]
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("all")]
        [Authorize(Roles = "Owner, Admin, SuperAdmin")]
        public async Task<IActionResult> GetAllUsers([FromQuery] PaginationParameters parameters)
        {
            var users = await _userService.GetAllUsersAsync(parameters);

            return Ok(users);
        }

        [HttpGet("{userId}")]
        [Authorize(Roles = "Owner, Admin, SuperAdmin")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var result = await _userService.GetUserByIdAsync(userId);

            return result.Handle(HttpContext.Request.Path, user =>
            {
                return Ok(user);
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetAuthorizedUser()
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

            var result = await _userService.GetUserByIdAsync(accountId);

            return result.Handle(HttpContext.Request.Path, user =>
            {
                return Ok(user);
            });
        }
    }
}
