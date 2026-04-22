using SIRU.Core.Domain.Entities;

namespace SIRU.Core.Domain.Interfaces;

public interface ICandidateRepository : IGenericRepository<Candidate>
{
    Task<Candidate?> FindByEmailAsync(string email);
}