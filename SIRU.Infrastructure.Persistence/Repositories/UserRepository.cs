using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SIRU.Core.Domain.Common;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;
using SIRU.Infrastructure.Persistence.Contexts;

namespace SIRU.Infrastructure.Persistence.Repositories;

public class UserRepository : GenericRepository<User, string>, IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;

    public UserRepository(ApplicationDbContext context, IConfiguration config) : base(context, config)
    {
        _context = context;
        _config = config;
    }

    public async override Task<PagedResult<User>> GetAllListAsync(PaginationParameters parameters)
    {
        var totalCount = await _context.Set<User>().CountAsync();

        var entities = await _context.Set<User>()
            .OrderByDescending(u => u.CreatedAt)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<User>(entities, parameters.Page, parameters.PageSize, totalCount);
    }
}