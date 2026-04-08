using SIRU.Core.Domain.Entities;

namespace SIRU.Core.Application.Dtos.Users;

public class GetUserDto : SaveUserDto
{
    public required DateTime CreatedAt { get; set; }
    public required bool IsDeleted { get; set; }
    public List<string> Roles { get; set; } = [];

    public static GetUserDto GetDto(User user)
    {
        return new GetUserDto
        {
            Id = user.Id,
            Name = user.Name,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            IsDeleted = user.IsDeleted
        };
    }

    public static List<GetUserDto> GetListDto(ICollection<User> users)
    {
        return users.Select(u => new GetUserDto
        {
            Id = u.Id,
            Name = u.Name,
            LastName = u.LastName,
            PhoneNumber = u.PhoneNumber,
            Email = u.Email,
            CreatedAt = u.CreatedAt,
            IsDeleted = u.IsDeleted
        }).ToList();
    }
}