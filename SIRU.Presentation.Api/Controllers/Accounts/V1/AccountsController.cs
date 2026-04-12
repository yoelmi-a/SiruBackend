using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIRU.Core.Application.Dtos.Accounts;
using SIRU.Core.Application.Dtos.Auth;
using SIRU.Core.Application.Interfaces.Accounts;
using SIRU.Presentation.Api.Handlers;

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
        [Authorize(Roles = "Owner, Admin, SuperAdmin")]
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
        [Authorize(Roles = "Owner, Admin, SuperAdmin")]
        public async Task<IActionResult> ChangeAccountStatus([FromBody] ChangeStatusDto changeStatusDto)
        {
            var changeStatusResponse = await _accountService.ChangeAccountStatusAsync(changeStatusDto.AccountId);

            return changeStatusResponse.Handle(HttpContext.Request.Path, () =>
            {
                return NoContent();
            });
        }
    }
}
