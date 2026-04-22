using SIRU.Core.Domain.Entities;

namespace SIRU.Core.Domain.Interfaces;

/// <summary>
/// Specific repository for Employee with work history queries.
/// </summary>
public interface IEmployeeRepository : IGenericRepository<Employee>
{
    /// <summary>
    /// Retrieves all EmployeePosition records for an employee including Position and Department details.
    /// </summary>
    /// <param name="employeeId">The ULID string identifier of the employee.</param>
    /// <returns>A list of EmployeePosition records ordered by StartDate descending.</returns>
    Task<IEnumerable<EmployeePosition>> GetEmployeeHistoryWithDetailsAsync(string employeeId);
}