using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SIRU.Core.Application.Dtos.Accounts;
using SIRU.Core.Application.Dtos.Users;
using SIRU.Core.Application.Interfaces.Accounts;
using SIRU.Core.Application.Interfaces.Users;
using SIRU.Core.Domain.Common.Enums;
using SIRU.Core.Domain.Common.Results;

namespace SIRU.Infrastructure.Identity.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserService _userService;
        private readonly IConfiguration _config;
        private readonly ILogger<AccountService> _logger;

        public AccountService(UserManager<IdentityUser> userManager, IUserService userService, IConfiguration config, ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _userService = userService;
            _config = config;
            _logger = logger;
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

            var userResult = account.EmailConfirmed ? await _userService.ActivateUserAsync(accountId) : await _userService.DeactivateUserAsync(accountId);

            if (!userResult.IsSuccess)
            {
                _logger.LogWarning("Account status with email {Email} couldn't be changed due to UserService errors: {Errors}", account.Email, userResult.Errors);
                account.EmailConfirmed = !account.EmailConfirmed;
                await _userManager.UpdateAsync(account);
                return Result.BadRequest(userResult.Errors.ToList());
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

            var user = new SaveUserDto()
            {
                Id = dto.Id,
                Name = dto.Name,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber
            };

            var updateUserResult = await _userService.UpdateUserAsync(user);

            if (!updateUserResult.IsSuccess)
            {
                _logger.LogWarning("Account with id {Id} couldn't be updated due to UserService errors: {Errors}", account.Id, updateUserResult.Errors);
                await _userManager.UpdateAsync(oldAccountData);

                if (roleChanged)
                {
                    await _userManager.RemoveFromRoleAsync(account, newRoleName); // rollback rol nuevo
                    await _userManager.AddToRolesAsync(account, currentRoles);   // rollback roles anteriores
                }

                return Result.BadRequest(updateUserResult.Errors.ToList());
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

            IdentityUser account = new()
            {
                Email = dto.Email,
                UserName = dto.Email,
                EmailConfirmed = true,
                PhoneNumber = dto.PhoneNumber
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
                case Roles.Owner:
                    roleResult = await _userManager.AddToRoleAsync(account, nameof(Roles.Owner));
                    break;
                case Roles.Admin:
                    roleResult = await _userManager.AddToRoleAsync(account, nameof(Roles.Admin));
                    break;
                case Roles.Delivery:
                    roleResult = await _userManager.AddToRoleAsync(account, nameof(Roles.Delivery));
                    break;
                case Roles.Mechanic:
                    roleResult = await _userManager.AddToRoleAsync(account, nameof(Roles.Mechanic));
                    break;
                case Roles.Clerk:
                    roleResult = await _userManager.AddToRoleAsync(account, nameof(Roles.Clerk));
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

            var user = new SaveUserDto()
            {
                Id = account.Id,
                Name = dto.Name,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber
            };

            var addUserResult = await _userService.AddUserAsync(user);

            if (!addUserResult.IsSuccess)
            {
                _logger.LogWarning("New Account couldn't be registered due to errors while creating the User entity: {Errors}", addUserResult.Errors.ToList());
                await _userManager.DeleteAsync(account);
                return addUserResult;
            }

            _logger.LogInformation("New Account with email {Email} has been registered successfully", dto.Email);
            return Result.Success();
        }
    }
}
