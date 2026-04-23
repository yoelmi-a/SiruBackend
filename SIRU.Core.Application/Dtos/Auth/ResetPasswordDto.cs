namespace SIRU.Core.Application.Dtos.Auth;

public class ResetPasswordDto
{
    public required string Password { get; set; }
    public required string AccountId { get; set; }
    public required string Token { get; set; }
}