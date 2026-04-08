namespace SIRU.Core.Application.Interfaces.Auth
{
    public interface IRoleService
    {
        Task<IReadOnlyDictionary<string, List<string>>> GetRolesByUserIdsAsync(IEnumerable<string> userIds);
        Task<List<string>> GetRolesByUserIdAsync(string userId);
    }
}
