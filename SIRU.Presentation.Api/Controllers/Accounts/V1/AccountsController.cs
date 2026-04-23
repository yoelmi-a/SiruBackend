using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIRU.Core.Application.Dtos.Accounts;
using SIRU.Core.Application.Dtos.Auth;
using SIRU.Core.Application.Interfaces.Accounts;
using SIRU.Core.Domain.Common.Pagination;
using SIRU.Presentation.Api.Handlers;
using System.Security.Claims;

namespace SIRU.Presentation.Api.Controllers.Accounts.V1
{
    [ApiVersion("1.0")]
    public class AccountsController : BaseApiController
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterAccount([FromBody] SaveAccountDto accountDto)
        {
            var registerResponse = await _accountService.RegisterAccountAsync(accountDto);

            return registerResponse.Handle(HttpContext.Request.Path, () =>
            {
                return Created();
            });
        }

        [HttpPatch("edit")]
        [Authorize]
        public async Task<IActionResult> EditAccount([FromBody] EditAccountDto accountDto)
        {
            var editResponse = await _accountService.EditAccountAsync(accountDto);

            return editResponse.Handle(HttpContext.Request.Path, () =>
            {
                return NoContent();
            });
        }

        [HttpPatch("change-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeAccountStatus([FromBody] ChangeStatusDto changeStatusDto)
        {
            var changeStatusResponse = await _accountService.ChangeAccountStatusAsync(changeStatusDto.AccountId);

            return changeStatusResponse.Handle(HttpContext.Request.Path, () =>
            {
                return NoContent();
            });
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAccounts([FromQuery] Pagination parameters)
        {
            var users = await _accountService.GetAllAccountsAsync(parameters);

            return Ok(users);
        }

        [HttpGet("{accountId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAccountById(string accountId)
        {
            var result = await _accountService.GetAccountByIdAsync(accountId);
            return result.Handle(HttpContext.Request.Path, account =>
            {
                return Ok(account);
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetAuthorizedAccount()
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

            var result = await _accountService.GetAccountByIdAsync(accountId);

            return result.Handle(HttpContext.Request.Path, account =>
            {
                return Ok(account);
            });
        }
    }
}
