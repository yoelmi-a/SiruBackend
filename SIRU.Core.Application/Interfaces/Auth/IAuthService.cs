using SIRU.Core.Application.Dtos.Auth;
using SIRU.Core.Domain.Common.Pagination;
using SIRU.Core.Domain.Common.Results;

namespace SIRU.Core.Application.Interfaces.Auth;

public interface IAuthService 
{
    Task<Result> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<Result> ResetPasswordAsync(ResetPasswordDto dto);
    Task<Result<AuthResponseDto>> AuthenticateAsync(AuthDto dto, string deviceInfo);
    Task<Result> LogOutAsync(string refreshToken);
    Task<Result<AuthResponseDto>> RefreshTokensAsync(RefreshRequestDto refreshRequest);
    Task<Result> RevokeUserSessionAsync(RevokeAccessRequestDto dto, bool isAdmin = false);
    Task<PaginatedResponse<UserSessionDto>> GetAllActiveSessionsByAccountIdAsync(Pagination parameters, string accountId);
}