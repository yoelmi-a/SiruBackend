using Microsoft.EntityFrameworkCore;
using SIRU.Core.Application.Interfaces.Auth;
using SIRU.Infrastructure.Identity.Contexts;

namespace SIRU.Infrastructure.Identity.Services
{
    public class RoleService : IRoleService
    {
        private readonly AuthDbContext _context;
        public RoleService(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyDictionary<string, List<string>>> GetRolesByUserIdsAsync(
        IEnumerable<string> userIds)
        {
            return await _context.UserRoles
                .AsNoTracking()
                .Where(ur => userIds.Contains(ur.UserId))
                .Join(_context.Roles,
                      ur => ur.RoleId,
                      r => r.Id,
                      (ur, r) => new { ur.UserId, RoleName = r.Name })
                .GroupBy(x => x.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Roles = g.Select(x => x.RoleName!).ToList()
                })
                .ToDictionaryAsync(
                    x => x.UserId,
                    x => x.Roles
                );
        }

        public async Task<List<string>> GetRolesByUserIdAsync(string userId)
        {
            return await _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .Join(_context.Roles,
                      ur => ur.RoleId,
                      r => r.Id,
                      (ur, r) => r.Name!)
                .ToListAsync();
        }
    }
}
