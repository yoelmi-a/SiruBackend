using SIRU.Core.Domain.Entities;

namespace SIRU.Core.Application.Dtos.Users;

public class SaveUserDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public required string Email { get; set; }

    public static User ToEntity(SaveUserDto dto)
    {
        return new User
        {
            Id = dto.Id,
            Name = dto.Name,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }
}