namespace SIRU.Core.Application.Dtos.Auth;

public class ForgotPasswordDto
{
    public required string Email { get; set; }
    public required string Origin { get; set; }
}