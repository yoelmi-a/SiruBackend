using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SIRU.Core.Application.Dtos.Accounts;
using SIRU.Core.Application.Interfaces.Accounts;
using SIRU.Core.Application.Interfaces.Auth;
using SIRU.Core.Domain.Common.Enums;
using SIRU.Core.Domain.Common.Pagination;
using SIRU.Core.Domain.Common.Results;
using SIRU.Infrastructure.Identity.Entities;

namespace SIRU.Infrastructure.Identity.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AuthAccount> _userManager;
        private readonly IConfiguration _config;
        private readonly ILogger<AccountService> _logger;
        private readonly IRoleService _roleService;

        public AccountService(UserManager<AuthAccount> userManager, IConfiguration config, ILogger<AccountService> logger, IRoleService roleService)
        {
            _userManager = userManager;
            _config = config;
            _logger = logger;
            _roleService = roleService;
        }
        public async Task<Result> ChangeAccountStatusAsync(string accountId)
        {
            _logger.LogInformation("Changing the status of the account with the Id {AccountId}", accountId);
            var account = await _userManager.FindByIdAsync(accountId);

            if (account == null)
            {
                _logger.LogWarning("Account status with id {AccountId} couldn't be changed because it was not found", accountId);
                return Result.NotFound(_config["AccountErrors:NotFound"] ?? "");
            }

            account.EmailConfirmed = !account.EmailConfirmed;
            var updateResult = await _userManager.UpdateAsync(account);

            if (!updateResult.Succeeded)
            {
                _logger.LogWarning("Account status with email {Email} couldn't be changed due to Identity's UserManager errors: {Errors}", account.Email, updateResult.Errors.Select(e => e.Description).ToList());
                return Result.BadRequest(updateResult.Errors.Select(e => e.Description).ToList());
            }

            _logger.LogInformation("Account status with email {Email} has been changed successfully", account.Email);
            return Result.Success();
        }

        public async Task<Result> EditAccountAsync(EditAccountDto dto)
        {
            _logger.LogInformation("Updating account with id {AccountId}", dto.Id);
            var accountWithSameEmail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.Id != dto.Id);
            if (accountWithSameEmail != null)
            {
                _logger.LogWarning("Account with id {Id} couldn't be updated because email {Email} is already in use by other the system account", dto.Id, dto.Email);
                return Result.BadRequest(_config["AccountErrors:EmailInUse"] ?? "");
            }

            var account = await _userManager.FindByIdAsync(dto.Id);

            if (account == null)
            {
                _logger.LogWarning("Account with id {id} not found", dto.Id);
                return Result.NotFound(_config["AccountErrors:NotFound"] ?? "");
            }

            // ── 1. Verificar si el rol cambió ────────────────────────────────
            var newRoleName = dto.Role.ToString();
            var currentRoles = await _userManager.GetRolesAsync(account);
            var roleChanged = !currentRoles.Contains(newRoleName);

            var oldAccountData = account;
            account.Email = dto.Email;
            account.UserName = dto.Email;
            account.PhoneNumber = dto.PhoneNumber;
            account.Name = dto.Name;
            account.LastName = dto.LastName;
            account.IdCard = dto.IdCard;

            var managerResult = await _userManager.UpdateAsync(account);

            if (!managerResult.Succeeded)
            {
                _logger.LogWarning("Account with id {Id} couldn't be updated due to Identity's UserManager errors: {Errors}", dto.Id, managerResult.Errors.Select(e => e.Description).ToList());
                return Result.BadRequest(managerResult.Errors.Select(e => e.Description).ToList());
            }

            // ── 2. Actualizar roles solo si cambió ───────────────────────────
            if (roleChanged)
            {
                // Elimina todos los roles actuales
                var removeResult = await _userManager.RemoveFromRolesAsync(account, currentRoles);

                if (!removeResult.Succeeded)
                {
                    _logger.LogWarning("Couldn't remove roles from account with id {Id}: {Errors}", dto.Id, removeResult.Errors.Select(e => e.Description));
                    await _userManager.UpdateAsync(oldAccountData); // rollback datos
                    return Result.BadRequest(removeResult.Errors.Select(e => e.Description).ToList());
                }

                // Asigna el nuevo rol
                var addResult = await _userManager.AddToRoleAsync(account, newRoleName);

                if (!addResult.Succeeded)
                {
                    _logger.LogWarning("Couldn't assign role {Role} to account with id {Id}: {Errors}", newRoleName, dto.Id, addResult.Errors.Select(e => e.Description));
                    await _userManager.UpdateAsync(oldAccountData); // rollback datos
                    await _userManager.AddToRolesAsync(account, currentRoles); // rollback roles
                    return Result.BadRequest(addResult.Errors.Select(e => e.Description).ToList());
                }

                _logger.LogInformation("Role updated from [{OldRoles}] to {NewRole} for account with id {Id}", string.Join(", ", currentRoles), newRoleName, dto.Id);
            }

            _logger.LogInformation("Account with email {Email} has been updated successfully", dto.Email);
            return Result.Success();
        }

        public async Task<Result> RegisterAccountAsync(SaveAccountDto dto)
        {
            _logger.LogInformation("Registering new account with email {Email}", dto.Email);
            var accountWithSameEmail = await _userManager.FindByEmailAsync(dto.Email);

            if (accountWithSameEmail != null)
            {
                _logger.LogWarning("New Account couldn't be registered because email {Email} is already in the system", dto.Email);
                return Result.BadRequest(_config["AccountErrors:EmailDuplicated"] ?? "");
            }

            AuthAccount account = new()
            {
                Email = dto.Email,
                UserName = dto.Email,
                EmailConfirmed = true,
                PhoneNumber = dto.PhoneNumber,
                Name = dto.Name,
                LastName = dto.LastName,
                IdCard = dto.IdCard
            };

            var managerResult = await _userManager.CreateAsync(account, dto.Password);

            if (!managerResult.Succeeded)
            {
                _logger.LogWarning("New Account couldn't be registered due to Identity's UserManager errors: {Errors}", managerResult.Errors.Select(e => e.Description).ToList());
                return Result.BadRequest(managerResult.Errors.Select(e => e.Description).ToList());
            }

            IdentityResult roleResult;

            switch (dto.Role)
            {
                case Roles.Admin:
                    roleResult = await _userManager.AddToRoleAsync(account, nameof(Roles.Admin));
                    break;
                case Roles.Supervisor:
                    roleResult = await _userManager.AddToRoleAsync(account, nameof(Roles.Supervisor));
                    break;
                default:
                    _logger.LogWarning("New Account with {Email} couldn't be registered due to invalid role", dto.Email);
                    return Result.BadRequest(_config["AccountErrors:InvalidRole"] ?? "");

            }

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(account);
                _logger.LogWarning("New Account couldn't be registered due to errors while assigning the role: {Errors}", managerResult.Errors.Select(e => e.Description).ToList());
                return Result.BadRequest(roleResult.Errors.Select(e => e.Description).ToList());
            }

            _logger.LogInformation("New Account with email {Email} has been registered successfully", dto.Email);
            return Result.Success();
        }

        public async Task<PaginatedResponse<GetAccountDto>> GetAllAccountsAsync(Pagination parameters)
        {
            _logger.LogInformation("A request to get all accounts has arrived");
            var accounts = await _userManager.Users
                .OrderByDescending(e => e.Id)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            var accountIds = accounts.Select(u => u.Id).ToHashSet();
            var rolesMap = await _roleService.GetRolesByUserIdsAsync(accountIds);

            var accountDtos = accounts.Select(e => new GetAccountDto()
            {
                Id = e.Id,
                Name = e.Name,
                LastName = e.LastName,
                Email = e.Email ?? "",
                IdCard = e.IdCard,
                IsVerified = e.EmailConfirmed,
                PhoneNumber = e.PhoneNumber,
                Role = ""
            }).ToList();

            foreach (var dto in accountDtos)
            {
                if (rolesMap.TryGetValue(dto.Id, out var roles))
                    dto.Role = roles.FirstOrDefault() ?? "";
            }

            return new PaginatedResponse<GetAccountDto>()
            {
                Items = accountDtos,
                Pagination = parameters
            };
        }

        public async Task<Result<GetAccountDto>> GetAccountByIdAsync(string id)
        {
            _logger.LogInformation("A request to get account with id {Id} has arrived", id);
            var account = await _userManager.FindByIdAsync(id);

            if (account == null)
            {
                _logger.LogWarning("Account with id {Id} was not found", id);
                return Result.NotFound<GetAccountDto>(_config["AccountErrors:NotFound"] ?? "");
            }

            var roles = await _roleService.GetRolesByUserIdAsync(account.Id);
            var accountDto = new GetAccountDto()
            {
                Id = account.Id,
                Name = account.Name,
                LastName = account.LastName,
                Email = account.Email ?? "",
                IdCard = account.IdCard,
                IsVerified = account.EmailConfirmed,
                PhoneNumber = account.PhoneNumber,
                Role = roles.FirstOrDefault() ?? ""
            };

            return Result.Success(accountDto);
        }
    }
}
