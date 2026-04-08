using SIRU.Core.Application.Dtos.Users;
using SIRU.Core.Domain.Common;
using SIRU.Core.Domain.Common.Results;

namespace SIRU.Core.Application.Interfaces.Users;

public interface IUserService
{
    Task<Result<GetUserDto>> AddUserAsync(SaveUserDto dto);
    Task<Result> ActivateUserAsync(string id);
    Task<Result> DeactivateUserAsync(string id);
    Task<PagedResult<GetUserDto>> GetAllUsersAsync(PaginationParameters paginationParameters);
    Task<Result<GetUserDto>> GetUserByIdAsync(string id);
    Task<Result<GetUserDto>> UpdateUserAsync(SaveUserDto dto);
}