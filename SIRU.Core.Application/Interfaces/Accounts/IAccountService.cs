using SIRU.Core.Application.Dtos.Accounts;
using SIRU.Core.Domain.Common.Results;

namespace SIRU.Core.Application.Interfaces.Accounts
{
    public interface IAccountService
    {
        Task<Result> RegisterAccountAsync(SaveAccountDto dto);
        Task<Result> EditAccountAsync(EditAccountDto dto);
        Task<Result> ChangeAccountStatusAsync(string accountId);
    }
}
