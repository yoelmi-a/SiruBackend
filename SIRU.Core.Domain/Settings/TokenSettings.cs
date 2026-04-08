namespace SIRU.Core.Domain.Settings;

public class TokenSettings
{
    public required string SecretKey { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required int JwtDurationInMinutes { get; set; }
    public required int RefreshTokenDurationInDays { get; set; }
}