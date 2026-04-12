using SIRU.Core.Application.Dtos.Users;

namespace SIRU.Core.Application.Dtos.Auth
{
    public class AuthResponseDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required DateTimeOffset RefreshTokenDurationInDays { get; set; }
        public required int AccessTokenDurationInMinutes { get; set; }
        public required GetUserDto User { get; set; }
    }
}
