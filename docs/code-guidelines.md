# Code Guidelines — SIRUS (Smart Intelligent Resource Optimization System)

## 1. Overview

This document defines the coding standards, naming conventions, architectural rules, and design principles that all developers must follow when contributing to the SIRUS project. Adherence to these guidelines is enforced via code review on every pull request.

---

## 2. Language & Runtime

- All source code must be written in **C#** targeting **.NET 10**.
- All identifiers (class names, method names, property names, variable names), XML documentation comments, and inline code comments must be written in **English**.
- The minimum C# language version is **C# 13** (the default for .NET 10).

---

## 3. Design Principles

### 3.1 SOLID

| Principle | Requirement |
|-----------|-------------|
| **S** — Single Responsibility | Each class has exactly one reason to change. Controllers coordinate; services orchestrate; repositories access data. |
| **O** — Open/Closed | Extend behavior through new classes or interface implementations; never modify stable, tested code. |
| **L** — Liskov Substitution | Any class implementing an interface (e.g., `IRankingService`) must be substitutable without altering correctness. |
| **I** — Interface Segregation | Define narrow, focused interfaces. Avoid `IDoEverything` god interfaces. |
| **D** — Dependency Inversion | All inter-layer dependencies must be on abstractions (interfaces), never on concrete implementations registered in DI. |

### 3.2 KISS & DRY

- **KISS:** Prefer the simplest solution that fulfills the requirement. No speculative generality.
- **DRY:** Extract repeated logic into shared helpers or base classes. A piece of knowledge has exactly one representation in the codebase.

### 3.3 Fail Fast

- Validate all inputs at the boundary (controller level via data annotations, or application service level).
- Return `Result.Failure(...)` for expected business violations; never silently ignore them.

---

## 4. Naming Conventions

### 4.1 General Rules

| Construct | Convention | Example |
|-----------|-----------|---------|
| Classes, interfaces, enums | `PascalCase` | `VacancyService`, `IRankingService` |
| Methods | `PascalCase` | `GetByIdAsync`, `ComputeScoreAsync` |
| Properties | `PascalCase` | `FirstName`, `IsSuccess` |
| Private fields | `_camelCase` | `_vacancyRepository` |
| Local variables, parameters | `camelCase` | `vacancyId`, `cvText` |
| Constants | `SCREAMING_SNAKE_CASE` | `MAX_CV_SIZE_MB` |
| Async methods | Suffix `Async` | `CreateVacancyAsync` |
| Interfaces | Prefix `I` | `IVacancyRepository` |
| DTOs | Suffix `Dto` | `VacancyDto`, `CreateVacancyDto` |
| Test classes | Suffix `Tests` | `VacancyServiceTests` |
| Test methods | `MethodName_StateUnderTest_ExpectedBehavior` | `GetByIdAsync_WithValidId_ReturnsVacancyDto` |

### 4.2 Project-Specific Conventions

- Repository implementations: `{Entity}Repository` (e.g., `VacancyRepository`).
- Service implementations: `{Entity}Service` (e.g., `VacancyService`).
- EF Core entity configuration classes: `{Entity}Configuration` (e.g., `VacancyConfiguration`).
- Background services: `{Purpose}BackgroundService` (e.g., `RankingBackgroundService`).

---

## 5. Project & Layer Rules

### 5.1 SIRU.CORE.Domain

- Must have **zero dependencies** on other projects in this solution.
- Contains only: entities, enums, `Result`, domain interfaces (`IRepository`, `IRankingService`), and `DomainErrors` static strings.
- No EF Core, no Mapster, no ASP.NET Core references.
- All entity constructors that generate ULID IDs must do so in the default constructor:

```csharp
public Vacancy()
{
    Id = Ulid.NewUlid().ToString();
}
```

### 5.2 SIRU.Core.Application

- Depends only on `SIRU.Core.Domain`.
- Defines all DTOs and service interfaces.
- All entity-to-DTO and DTO-to-entity mapping must use **Mapster** via `Adapt<T>()`.
- No manual property assignment (`destination.Prop = source.Prop`) is permitted in service code.
- All service methods that can fail for business reasons must return `Result<T>`.
- Services must never reference `DbContext` or any EF Core type.

### 5.3 SIRU.Infrastructure.*

- Depends on `SIRU.Core.Application` and `SIRU.Core.Domain`.
- Repository implementations must never expose `IQueryable<T>` outside the repository class.
- Always use `async` EF Core methods.
- Lazy loading is disabled; use `Include` / `ThenInclude` explicitly.
- ML.NET pipeline instances (`MLContext`) should be created once and reused where possible (register as `Singleton` via DI).

### 5.4 SIRU.Presentation.Api

- Depends on `SIRU.Infrastructure.*` and `SIRU.Core.Application`.
- Controllers must be thin. No business logic, no repository calls, no EF Core usage.
- Every controller action must call an application service method and map the `Result` to an HTTP response.
- Use `[ProducesResponseType]` attributes on every action for accurate OpenAPI documentation.
- Standard HTTP response mapping:

| Result | HTTP Status |
|----------------|-------------|
| `Success` with data | `200 OK` or `201 Created` |
| `Success` with no data | `204 No Content` |
| `Failure` (not found) | `404 Not Found` |
| `Failure` (conflict / business rule violation) | `409 Conflict` |
| `Failure` (validation) | `400 Bad Request` or `422 Unprocessable Entity` |
| Unhandled exception (middleware) | `500 Internal Server Error` |

---

## 6. Result Pattern

Every service and repository method that can fail for expected business reasons must return `Result<T>` or `Result`. Exceptions are reserved for truly unexpected infrastructure failures.

```csharp
// ✅ Correct
public async Task<Result<VacancyDto>> GetByIdAsync(string id)
{
    var vacancy = await _repository.GetByIdAsync(id);
    if (vacancy is null)
        return Result<VacancyDto>.Failure(DomainErrors.Vacancy.NotFound);

    return Result<VacancyDto>.Success(vacancy.Adapt<VacancyDto>());
}

// ❌ Wrong — do not throw for expected cases
public async Task<VacancyDto> GetByIdAsync(string id)
{
    var vacancy = await _repository.GetByIdAsync(id)
        ?? throw new NotFoundException("Vacancy not found");
    return vacancy.Adapt<VacancyDto>();
}
```

---

## 7. Mapster Usage

- All mapping configuration must be in `MappingConfig.RegisterMappings()` and registered at startup.
- Use `Adapt<T>()` extension method for single-object mapping.
- Use `ProjectToType<T>()` for EF Core queryable projections to avoid loading full entities.
- Never write custom mapping methods outside of `MappingConfig`; always configure in `TypeAdapterConfig`.

```csharp
// ✅ Correct in service
var dto = entity.Adapt<VacancyDto>();

// ✅ Correct in repository (projection)
var dtos = await _context.Vacancies
    .ProjectToType<VacancyDto>()
    .ToListAsync();

// ❌ Wrong — manual mapping
var dto = new VacancyDto { Title = entity.Title, Profile = entity.Profile };
```

---

## 8. XML Documentation

Every public and internal method, class, property, and interface must have XML documentation comments:

```csharp
/// <summary>
/// Retrieves a vacancy by its unique identifier.
/// </summary>
/// <param name="id">The ULID string identifier of the vacancy.</param>
/// <returns>
/// An <see cref="Result{VacancyDto}"/> containing the vacancy data on success,
/// or an error message if the vacancy does not exist.
/// </returns>
public async Task<Result<VacancyDto>> GetByIdAsync(string id)
```

---

## 9. Asynchronous Programming

- All I/O-bound operations (database, file, HTTP) must be `async` / `await`.
- Never use `.Result`, `.Wait()`, or `Task.Run(...)` to synchronously block async code.
- Use `CancellationToken` parameters in public async methods and propagate them to EF Core calls.
- Background tasks must implement `IHostedService` or `BackgroundService`; do not fire-and-forget with `Task.Run`.

---

## 10. Unit Testing Standards

- Every public method must have at least **one success test** and **one failure test**.
- Use the **Arrange / Act / Assert** pattern with clear section comments.
- Use EF Core InMemory provider for unit tests; use Testcontainers (PostgreSQL) for integration tests.
- Mock external dependencies with **Moq**; never mock the system under test.
- Test method naming: `MethodName_StateUnderTest_ExpectedBehavior`.
- Minimum coverage target: **80% of all methods** in Application and Infrastructure layers.
- Tests must be isolated: no shared state between test methods; use unique InMemory DB names per test.

```csharp
[Fact]
public async Task GetByIdAsync_WithExistingId_ReturnsVacancyDto()
{
    // Arrange
    await using var context = InMemoryDbHelper.CreateContext();
    context.Vacancies.Add(new Vacancy { Id = "01HV...", Title = "Developer" });
    await context.SaveChangesAsync();
    var service = new VacancyService(new VacancyRepository(context));

    // Act
    var result = await service.GetByIdAsync("01HV...");

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("Developer", result.Value!.Title);
}
```

---

## 11. Git Workflow

| Branch | Purpose |
|--------|---------|
| `main` | Production-ready code only. Merges via PR from `develop`. |
| `develop` | Integration branch. All `feature/*` and `fix/*` branches merge here. |
| `feature/<name>` | New functionality (e.g., `feature/vacancy-ranking`). |
| `fix/<name>` | Bug fixes (e.g., `fix/score-calculation`). |

- Commit messages must follow Conventional Commits: `feat:`, `fix:`, `docs:`, `test:`, `refactor:`.
- No direct commits to `main` or `develop`; all changes require a pull request with at least one review.
- The CI pipeline must pass (build + all tests) before a PR can be merged.

---

## 12. Security Conventions

- Never commit connection strings, passwords, or API keys in any file tracked by Git.
- Use **.NET User Secrets** (`dotnet user-secrets`) in local development.
- Use **environment variables** or a secrets manager in production.
- Validate uploaded PDF files for MIME type (`application/pdf`) and maximum file size before processing.
- Sanitize all string inputs before passing them to SQL queries (EF Core parameterizes by default; avoid raw SQL).
- Log errors without exposing stack traces or sensitive data in API responses.

---

## 13. Code Formatting

- Use default **Visual Studio / Rider** formatting with an `.editorconfig` file committed to the repository.
- Maximum line length: **120 characters**.
- Use **file-scoped namespaces** (`namespace SIRU.Core.Application.Services;`).
- Prefer `var` when the type is obvious from the right-hand side.
- Use **collection expressions** (`[item1, item2]`) and **primary constructors** where appropriate (C# 13).
- Enable **Nullable Reference Types** (`<Nullable>enable</Nullable>`) project-wide; resolve all warnings.
