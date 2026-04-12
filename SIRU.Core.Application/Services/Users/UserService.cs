using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SIRU.Core.Application.Dtos.Users;
using SIRU.Core.Application.Interfaces.Auth;
using SIRU.Core.Application.Interfaces.Users;
using SIRU.Core.Domain.Common;
using SIRU.Core.Domain.Common.Results;
using SIRU.Core.Domain.Interfaces;

namespace SIRU.Core.Application.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IRoleService _roleService;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _config;

        public UserService(IUserRepository repository, IRoleService roleService, ILogger<UserService> logger, IConfiguration config)
        {
            _repository = repository;
            _roleService = roleService;
            _logger = logger;
            _config = config;
        }

        public async Task<Result<GetUserDto>> AddUserAsync(SaveUserDto dto)
        {
            _logger.LogInformation("Adding new user with email {Email} to the system", dto.Email);
            var user = SaveUserDto.ToEntity(dto);
            await _repository.AddAsync(user);

            _logger.LogInformation("New user with email {Email} successfully added", dto.Email);
            return Result.Success(GetUserDto.GetDto(user));
        }

        public async Task<Result> DeactivateUserAsync(string id)
        {
            _logger.LogInformation("Trying to deactivate user with id {Id}", id);
            var user = await _repository.GetByIdAsync(id);

            if (user == null)
            {
                _logger.LogCritical("User with id {Id} couldn't be deactivated because it was not found", id);
                return Result.NotFound(_config["UserErrors:NotFound"] ?? "");
            }

            user.IsDeleted = true;
            await _repository.SaveChangesAsync();

            _logger.LogInformation("User with id {Id} successfully deactivated", id);
            return Result.Success();
        }

        public async Task<Result> ActivateUserAsync(string id)
        {
            _logger.LogInformation("Trying to activate user with id {Id}", id);
            var user = await _repository.GetByIdAsync(id);

            if (user == null)
            {
                _logger.LogCritical("User with id {Id} couldn't be activated because it was not found", id);
                return Result.NotFound(_config["UserErrors:NotFound"] ?? "");
            }

            user.IsDeleted = false;
            await _repository.SaveChangesAsync();

            _logger.LogInformation("User with id {Id} successfully deactivated", id);
            return Result.Success();
        }

        public async Task<PagedResult<GetUserDto>> GetAllUsersAsync(PaginationParameters paginationParameters)
        {
            _logger.LogInformation("A request to get all users has arrived");
            var pagedResult = await _repository.GetAllListAsync(paginationParameters);
            var userIds = pagedResult.Items.Select(u => u.Id).ToHashSet();
            var rolesMap = await _roleService.GetRolesByUserIdsAsync(userIds);
            var userDtos = GetUserDto.GetListDto(pagedResult.Items);

            foreach (var dto in userDtos)
            {
                if (rolesMap.TryGetValue(dto.Id, out var roles))
                    dto.Roles = roles;
            }

            return new PagedResult<GetUserDto>
            {
                Items = userDtos,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalCount,
                TotalPages = pagedResult.TotalPages
            };
        }

        public async Task<Result<GetUserDto>> GetUserByIdAsync(string id)
        {
            _logger.LogInformation("A request to get user with id {Id} has arrived", id);
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
            {
                _logger.LogWarning("User with id {Id} was not found", id);
                return Result.NotFound<GetUserDto>(_config["UserErrors:NotFound"] ?? "");
            }

            var roles = await _roleService.GetRolesByUserIdAsync(entity.Id);
            var userDto = GetUserDto.GetDto(entity);
            userDto.Roles = roles;
            return Result.Success(userDto);
        }

        public async Task<Result<GetUserDto>> UpdateUserAsync(SaveUserDto dto)
        {
            _logger.LogInformation("Updating user with id {Id}", dto.Id);
            var user = SaveUserDto.ToEntity(dto);
            var updateResult = await _repository.UpdateAsync(user, user.Id);

            if (!updateResult.IsSuccess)
            {
                _logger.LogWarning("User with id {Id} couldn't be updated because it was not found", dto.Id);
                return Result.NotFound<GetUserDto>(updateResult.Errors.ToList());
            }

            _logger.LogInformation("User with id {Id} successfully updated", dto.Id);
            return Result.Success(GetUserDto.GetDto(user));
        }
    }
}
