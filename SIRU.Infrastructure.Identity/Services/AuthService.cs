using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SIRU.Core.Application.Dtos.Auth;
using SIRU.Core.Application.Dtos.Emails;
using SIRU.Core.Application.Interfaces.Auth;
using SIRU.Core.Application.Interfaces.Shared;
using SIRU.Core.Application.Interfaces.Users;
using SIRU.Core.Domain.Common;
using SIRU.Core.Domain.Common.Results;
using SIRU.Core.Domain.Settings;
using SIRU.Infrastructure.Identity.Contexts;
using SIRU.Infrastructure.Identity.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SIRU.Infrastructure.Identity.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IUserService _userService;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly TokenSettings _tokenSettings;
    private readonly AuthDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<IdentityUser> userManager,
        IEmailService emailService,
        IUserService userService,
        SignInManager<IdentityUser> signInManager,
        IOptions<TokenSettings> tokenSettings,
        AuthDbContext context,
        IConfiguration config,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _userService = userService;
        _signInManager = signInManager;
        _tokenSettings = tokenSettings.Value;
        _context = context;
        _config = config;
        _logger = logger;
    }

    public async Task<Result<AuthResponseDto>> AuthenticateAsync(AuthDto dto, string deviceInfo)
    {
        _logger.LogInformation("Authenticating account with email {Email} and device: {DeviceInfo}", dto.Email, deviceInfo);
        var account = await _userManager.FindByEmailAsync(dto.Email);

        if (account == null)
        {
            _logger.LogWarning("Account with email {Email} not found", dto.Email);
            return Result.NotFound<AuthResponseDto>(_config["AccountErrors:NotFound"] ?? "");
        }

        if (!account.EmailConfirmed)
        {
            _logger.LogWarning("Account with email {Email} inactive", dto.Email);
            return Result.Forbidden<AuthResponseDto>(_config["AccountErrors:Inactive"] ?? "");
        }

        var authResult = await _signInManager.CheckPasswordSignInAsync(account, dto.Password, true);

        if (authResult.IsLockedOut)
        {
            _logger.LogWarning("Account with email {Email} is locked out", dto.Email);
            return Result.Locked<AuthResponseDto>(_config["AccountErrors:LockedOut"] ?? "");
        }

        if (!authResult.Succeeded)
        {
            _logger.LogWarning("Account with email {Email} has attempted to authenticate with a wrong password", dto.Email);
            return Result.BadRequest<AuthResponseDto>(_config["AccountErrors:WrongPassword"] ?? "");
        }

        JwtSecurityToken jwtSecurityToken = await GenerateJwtToken(account);

        var jwt = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        var refreshToken = GenerateRefreshToken();
        var hashedRefreshToken = HashRefreshToken(refreshToken);

        var existingUserSession = await _context
            .UserSessions
            .FirstOrDefaultAsync(
                token => token.AccountId == account.Id
                && token.DeviceInfo == deviceInfo
            );

        var refreshTokenDurationInDays = DateTime.UtcNow.AddDays(_tokenSettings.RefreshTokenDurationInDays);

        if (existingUserSession != null)
        {
            existingUserSession.RefreshTokenHash = hashedRefreshToken;
            existingUserSession.ExpiresAt = refreshTokenDurationInDays;
            existingUserSession.IsActive = true;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Account with email {Email} has a new refresh token for the session with Id: {SessionId}", dto.Email, existingUserSession.Id);
        }
        else
        {
            var id = Guid.CreateVersion7().ToString();
            await _context.UserSessions.AddAsync(new UserSession()
            {
                Id = id,
                RefreshTokenHash = hashedRefreshToken,
                AccountId = account.Id,
                DeviceInfo = deviceInfo,
                ExpiresAt = refreshTokenDurationInDays,
                IsActive = true
            });

            await _context.SaveChangesAsync();
            _logger.LogInformation("Account with email {Email} has a new Session with Id: {SessionId}", dto.Email, id);
        }

        var getUserResult = await _userService.GetUserByIdAsync(account.Id);

        if (!getUserResult.IsSuccess)
        {
            return Result.NotFound<AuthResponseDto>(getUserResult.Errors.ToList());
        }

        _logger.LogInformation("Account with email {Email} has been authenticated successfully", dto.Email);
        return Result.Success<AuthResponseDto>(new AuthResponseDto() 
        {
            AccessToken = jwt,
            RefreshToken = refreshToken,
            RefreshTokenDurationInDays = refreshTokenDurationInDays,
            AccessTokenDurationInMinutes = _tokenSettings.JwtDurationInMinutes,
            User = getUserResult.Value!
        });
    }
    public async Task<Result> LogOutAsync(string refreshToken)
    {
        _logger.LogInformation("A request to log out has arrived");

        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == HashRefreshToken(refreshToken) && s.IsActive);

        if (session == null)
        {
            _logger.LogWarning("The request to log out failed because the session was not found");
            return Result.NotFound(_config["UserSessionErrors:NotFound"] ?? "");
        }

        session.IsActive = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Session with id {SessionId} has successfully logged out", session.Id);
        return Result.Success();
    }
    public async Task<Result> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        _logger.LogInformation("Account with email {Email} is trying to get the reset password url", dto.Email);
        var account = await _userManager.FindByEmailAsync(dto.Email);

        if (account == null)
        {
            _logger.LogWarning("Account with email {Email} not found", dto.Email);
            return Result.NotFound(_config["AccountErrors:NotFound"] ?? "");
        }

        if (!account.EmailConfirmed)
        {
            _logger.LogWarning("Inactive account with email {Email} is trying to get the reset password url", dto.Email);
            return Result.Forbidden<AuthResponseDto>(_config["AccountErrors:Inactive"] ?? "");
        }

        account.EmailConfirmed = false;
        var managerResult = await _userManager.UpdateAsync(account);

        if (!managerResult.Succeeded)
        {
            _logger.LogCritical("Account with email {Email} couldn't get the reset password url due to Identity's UserManager errors while deactivating the account. Errors: {Errors}", account.Email, managerResult.Errors.Select(e => e.Description).ToList());
            return Result.BadRequest(managerResult.Errors.Select(e => e.Description).ToList());
        }

        var deactivateUserResult = await _userService.DeactivateUserAsync(account.Id);

        if (!deactivateUserResult.IsSuccess)
        {
            account.EmailConfirmed = true;
            await _userManager.UpdateAsync(account);
            _logger.LogCritical("Account with email {Email} couldn't get the reset password url due to UserService errors while deactivating the user. Errors: {Errors}", account.Email, deactivateUserResult.Errors);
            return Result.BadRequest(deactivateUserResult.Errors.ToList());
        }

        var activeSessions = await _context
                .UserSessions
                .Where(session => session.AccountId == account.Id && session.IsActive)
                .ToListAsync();

        if (activeSessions.Any())
        {
            activeSessions.ForEach(session =>
            {
                session.IsActive = false;
                session.ExpiresAt = DateTime.UtcNow.AddDays(-1);
            });

            await _context.SaveChangesAsync();
        }

        var resetUri = await GetResetPasswordUri(account, dto.Origin ?? "");

        await _emailService.SendAsync(new EmailRequestDto()
        {
            To = [dto.Email],
            HtmlBody = $"Please reset your password visiting this <a href='{resetUri}'>url</a>",
            Subject = "Reset Password"
        });

        _logger.LogInformation("Account with email {Email} has successfully obtained the reset password url", dto.Email);
        return Result.Success();
    }
    public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto)
    {
        _logger.LogInformation("Account with id {Id} is trying to reset its password", dto.AccountId);
        var account = await _userManager.FindByIdAsync(dto.AccountId);

        if (account == null)
        {
            _logger.LogWarning("Account with id {Id} not found", dto.AccountId);
            return Result.NotFound(_config["AccountErrors:NotFound"] ?? "");
        }

        var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
        var resetPasswordResult = await _userManager.ResetPasswordAsync(account, token, dto.Password);

        if (!resetPasswordResult.Succeeded)
        {
            _logger.LogWarning("Account with email {Email} couldn't reset its password due to Identity's UserManager errors: {Errors}", account.Email, resetPasswordResult.Errors.Select(e => e.Description).ToList());
            return Result.BadRequest(resetPasswordResult.Errors.Select(e => e.Description).ToList());
        }

        account.EmailConfirmed = true;
        var activateAccountResult = await _userManager.UpdateAsync(account);

        if (!activateAccountResult.Succeeded)
        {
            _logger.LogCritical("Account with email {Email} couldn't be activated due to Identity's UserManager errors while activating the account. Errors: {Errors}", account.Email, activateAccountResult.Errors.Select(e => e.Description).ToList());
            return Result.BadRequest(activateAccountResult.Errors.Select(e => e.Description).ToList());
        }

        var activateUserResult = await _userService.ActivateUserAsync(account.Id);

        if (!activateUserResult.IsSuccess)
        {
            account.EmailConfirmed = false;
            await _userManager.UpdateAsync(account);
            _logger.LogCritical("Account with email {Email} couldn't be activated due to UserService errors while activating the user. Errors: {Errors}", account.Email, activateUserResult.Errors);
            return Result.BadRequest(activateUserResult.Errors.ToList());
        }

        _logger.LogInformation("Account with email {Email} has successfully reset its password", account.Email);
        return Result.Success();
    }
    public async Task<Result<AuthResponseDto>> RefreshTokensAsync(RefreshRequestDto refreshRequest)
    {
        _logger.LogInformation("A new refresh tokens request arrived");
        var principalResult = GetPrincipalFromExpiredToken(refreshRequest.AccessToken);

        if (!principalResult.IsSuccess)
        {
            _logger.LogWarning("Tokens couldn't be refreshed due to principal errors: {Errors}", principalResult.Errors);
            return Result.Forbidden<AuthResponseDto>(principalResult.Errors.ToList());
        }

        var principal = principalResult.Value;
        var accountId = principal!.FindFirst("uid")?.Value;

        if (accountId == null)
        {
            _logger.LogCritical("A token without account id was trying to refresh");
            return Result.Forbidden<AuthResponseDto>(_config["TokenErrors:Invalid"] ?? "");
        }

        var hashedRefreshToken = HashRefreshToken(refreshRequest.RefreshToken);

        var savedUserSession = await _context.UserSessions
               .FirstOrDefaultAsync(session => session.RefreshTokenHash == hashedRefreshToken && session.AccountId == accountId);

        if (savedUserSession == null)
        {
            _logger.LogWarning("Session in device {DeviceInfo} of account with id {AccountId} couldn't refresh its tokens because it was not found", refreshRequest.DeviceInfo, accountId);
            return Result.Unauthorized<AuthResponseDto>(_config["UserSessionErrors:NotFound"] ?? "");
        }

        if (savedUserSession.ExpiresAt < DateTime.UtcNow
            || !savedUserSession.IsActive
            || savedUserSession.DeviceInfo != refreshRequest.DeviceInfo)
        {
            _logger.LogInformation("Session in device {DeviceInfo} of account with id {AccountId} has an invalid refresh token", refreshRequest.DeviceInfo, accountId);
            return Result.Unauthorized<AuthResponseDto>(_config["TokenErrors:RefreshInvalid"] ?? "");
        }

        var account = await _userManager.FindByIdAsync(accountId);

        if (account == null)
        {
            _logger.LogCritical("Session in device {DeviceInfo} of account with id {AccountId} couldn't refresh its tokens because the account was not found", refreshRequest.DeviceInfo, accountId);
            return Result.NotFound<AuthResponseDto>(_config["AccountErrors:NotFound"] ?? "");
        }

        var jwtSecurityToken = await GenerateJwtToken(account);
        var jwt = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        var newRefreshToken = GenerateRefreshToken();
        savedUserSession.RefreshTokenHash = HashRefreshToken(newRefreshToken);
        await _context.SaveChangesAsync();

        var getUserResult = await _userService.GetUserByIdAsync(account.Id);

        if (!getUserResult.IsSuccess)
        {
            return Result.NotFound<AuthResponseDto>(getUserResult.Errors.ToList());
        }

        _logger.LogInformation("Session in device {DeviceInfo} of account with id {AccountId} has successfully refreshed its tokens", refreshRequest.DeviceInfo, accountId);
        return Result.Success<AuthResponseDto>(new ()
        {
            AccessToken = jwt,
            RefreshToken = newRefreshToken,
            RefreshTokenDurationInDays = savedUserSession.ExpiresAt,
            AccessTokenDurationInMinutes = _tokenSettings.JwtDurationInMinutes,
            User = getUserResult.Value!
        });
    }
    public async Task<Result> RevokeUserSessionAsync(RevokeAccessRequestDto dto, bool isAdmin = false)
    {
        _logger.LogInformation("A revoke request by account with id {AccountId} for a session with id {SessionId} has arrived", dto.ActionMadeByAccountId, dto.SessionId);
        var session = await _context.UserSessions.FirstOrDefaultAsync(session => session.Id == dto.SessionId);

        if (session == null)
        {
            _logger.LogWarning("Session with id {Id} couldn't be revoked because it was not found", dto.SessionId);
            return Result.NotFound(_config["UserSessionErrors:NotFound"] ?? "");
        }

        var actionMadeByAccount = await _userManager.FindByIdAsync(dto.ActionMadeByAccountId);

        if (actionMadeByAccount == null)
        {
            _logger.LogWarning("Session with id {Id} couldn't be revoked because the account that request was not found", dto.SessionId);
            return Result.NotFound(_config["UserSessionErrors:AccountNotFound"] ?? "");
        }

        if (!isAdmin && session.AccountId != dto.ActionMadeByAccountId)
        {
            _logger.LogWarning("Session with id {Id} couldn't be revoked because the account with email {Email} doesn't have admin permissions", dto.SessionId, actionMadeByAccount.Email);
            return Result.Forbidden(_config["UserSessionErrors:Forbidden"] ?? "");
        }

        session.IsActive = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Session with id {Id} has been successfully revoked by the account with email {Email}", dto.SessionId, actionMadeByAccount.Email);
        return Result.Success();
    }

    public async Task<PagedResult<UserSessionDto>> GetAllActiveSessionsByAccountIdAsync(PaginationParameters parameters, string accountId)
    {
        _logger.LogInformation("A new request to get all the active sessions of the account with id {AccountId} has arrived", accountId);
        var totalCount = await _context.UserSessions.Where(s => s.AccountId == accountId && s.IsActive).CountAsync();

        var sessions = await _context.UserSessions
            .Where(s => s.AccountId == accountId && s.IsActive)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();
        
        var sessionDtos = sessions.Select(s => new UserSessionDto
        {
            Id = s.Id,
            DeviceInfo = s.DeviceInfo
        }).ToList();

        return new PagedResult<UserSessionDto>(sessionDtos, parameters.Page, parameters.PageSize, totalCount);
    }

    #region Private methods

    private async Task<JwtSecurityToken> GenerateJwtToken(IdentityUser account)
    {
        var userClaims = await _userManager.GetClaimsAsync(account);
        var roles = await _userManager.GetRolesAsync(account);
        var rolesClaims = new List<Claim>();

        foreach (var role in roles)
        {
            rolesClaims.Add(new("roles", role));
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, account.Email ?? ""),
            new Claim("uid", account.Id),
        }.Union(userClaims).Union(rolesClaims);

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.SecretKey));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        var jwtSecurityToken = new JwtSecurityToken
        (
            issuer: _tokenSettings.Issuer,
            audience: _tokenSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_tokenSettings.JwtDurationInMinutes),
            signingCredentials: signingCredentials
        );

        return jwtSecurityToken;
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private static string HashRefreshToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private Result<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_tokenSettings.SecretKey)),
            ValidateLifetime = false,
            ValidIssuer = _tokenSettings.Issuer,
            ValidAudience = _tokenSettings.Audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
        {
            return Result.Failure<ClaimsPrincipal>(_config["TokenErrors:Invalid"] ?? "");
        }

        return Result.Success<ClaimsPrincipal>(principal);
    }

    private async Task<string> GetResetPasswordUri(IdentityUser account, string origin)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(account);
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var route = "auth/reset-password";
        var completeUrl = new Uri(string.Concat(origin, "/", route));

        var resetUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "accountId", account.Id);
        resetUri = QueryHelpers.AddQueryString(resetUri.ToString(), "token", token);

        return resetUri;
    }

    #endregion
}