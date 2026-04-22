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

/// <summary>
    /// Retrieves the most recent active EmployeePosition for an employee (no EndDate).
    /// </summary>
    /// <param name="employeeId">The ULID string identifier of the employee.</param>
    /// <returns>The current EmployeePosition or null if none found.</returns>
    Task<EmployeePosition?> GetCurrentPositionAsync(string employeeId);

    /// <summary>
    /// Retrieves all evaluations for an employee across all their positions,
    /// including criteria details, ordered by evaluation date descending.
    /// </summary>
    /// <param name="employeeId">The ULID string identifier of the employee.</param>
    /// <returns>A list of Evaluation records.</returns>
    Task<IEnumerable<Evaluation>> GetEmployeeEvaluationsAsync(string employeeId);
}