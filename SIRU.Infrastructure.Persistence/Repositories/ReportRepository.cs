using Microsoft.EntityFrameworkCore;
using SIRU.Core.Application.Dtos.Reports;
using SIRU.Core.Application.Interfaces.Reports;
using SIRU.Core.Domain.Common;
using SIRU.Core.Domain.Common.Enums;
using SIRU.Core.Domain.Common.Pagination;
using SIRU.Core.Domain.Entities;
using SIRU.Infrastructure.Persistence.Contexts;

namespace SIRU.Infrastructure.Persistence.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly ApplicationDbContext _context;

    public ReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HiringTimeReportDto> GetAverageHiringTimeAsync()
    {
        var vacancies = await _context.Vacants
            .Where(v => v.Status == VacantStatus.Closed && v.HiringDate != null)
            .ToListAsync();

        if (vacancies.Count == 0)
        {
            return new HiringTimeReportDto { AverageDays = 0, TotalClosedVacancies = 0 };
        }

        var averageDays = (float)Math.Round(
            vacancies.Average(v => (v.HiringDate!.Value - v.PublicationDate).Days), 2);

        return new HiringTimeReportDto
        {
            AverageDays = averageDays,
            TotalClosedVacancies = vacancies.Count
        };
    }

    public async Task<IEnumerable<DepartmentPerformanceDto>> GetPerformanceByDepartmentAsync()
    {
        var results = await _context.Evaluations
            .Where(e => e.EmployeePosition != null)
            .Select(e => new
            {
                DepartmentName = e.EmployeePosition!.Position != null
                    ? e.EmployeePosition.Position.Department != null
                        ? e.EmployeePosition.Position.Department.Name
                        : "N/A"
                    : "N/A",
                e.AverageScore
            })
            .Where(x => x.DepartmentName != "N/A")
            .GroupBy(x => x.DepartmentName)
            .Select(g => new DepartmentPerformanceDto
            {
                DepartmentName = g.Key,
                AverageScore = (float)Math.Round(g.Average(x => x.AverageScore), 2),
                EmployeeCount = g.Count()
            })
            .OrderByDescending(d => d.AverageScore)
            .ToListAsync();

        return results;
    }

    public async Task<PaginatedResponse<EmployeeReportDto>> GetEmployeeReportAsync(Pagination pagination, bool? isActive)
    {
        var query = _context.Employees.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(e => e.Status == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var employees = await query
            .Select(e => new
            {
                e.Id,
                e.Names,
                e.LastNames,
                e.IdCard,
                e.Status,
                CurrentPosition = e.PositionsOccupied!
                    .Where(p => p.EndDate == null)
                    .OrderByDescending(p => p.StartDate)
                    .FirstOrDefault()
            })
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var dtos = employees.Select(emp =>
        {
            var positionName = emp.CurrentPosition != null && emp.CurrentPosition.Position != null
                ? emp.CurrentPosition.Position.Name
                : "Unassigned";
            var departmentName = emp.CurrentPosition != null && emp.CurrentPosition.Position != null
                && emp.CurrentPosition.Position.Department != null
                ? emp.CurrentPosition.Position.Department.Name
                : "Unassigned";

            return new EmployeeReportDto
            {
                Id = emp.Id,
                FullName = $"{emp.Names} {emp.LastNames}",
                Cedula = emp.IdCard,
                Position = positionName,
                Department = departmentName,
                IsActive = emp.Status
            };
        }).ToList();

        return new PaginatedResponse<EmployeeReportDto>
        {
            Items = dtos,
            Pagination = new Pagination
            {
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalCount = totalCount
            }
        };
    }
}