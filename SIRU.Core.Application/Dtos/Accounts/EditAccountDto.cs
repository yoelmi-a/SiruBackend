using SIRU.Core.Domain.Common.Enums;

namespace SIRU.Core.Application.Dtos.Accounts;

public class EditAccountDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required Roles Role { get; set; }
    public string? PhoneNumber { get; set; }
}