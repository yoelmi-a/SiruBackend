# Architecture — SIRUS (Smart Intelligent Resource Optimization System)

## 1. Overview

SIRUS is a **RESTful API** built with ASP.NET Core 10 following **Onion Architecture**. The architecture enforces a strict dependency rule: outer layers depend on inner layers; inner layers never reference outer layers.

```
┌────────────────────────────────────────────────────────────┐
│                     Presentation Layer                     │
│                      SIRU.Presentation.Api                 │
│   Controllers · Middleware · Filters · Program.cs          │
└────────────────────────────┬───────────────────────────────┘
                             │ depends on
┌────────────────────────────▼───────────────────────────────┐
│                   Infrastructure Layer                     │
│  SIRU.Infrastructure.Persistence  (EF Core / PostgreSQL)   │
│  SIRU.Infrastructure.Ranking      (ML.NET NLP)             │
│  SIRU.Infrastructure.Shared       (Email, PDF, Storage)    │
│  SIRU.Infrastructure.Auth         (Auth stubs — OOS)       │
└────────────────────────────┬───────────────────────────────┘
                             │ depends on
┌────────────────────────────▼───────────────────────────────────┐
│                      Core Layer                                │
│  SIRU.Core.Application   (Use cases, services, DTOs, mapping)  │
│  SIRU.Core.Domain        (Entities, enums, interfaces, errors) │
└────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│                      Test Layer                            │
│  SIRU.Tests   (Unit + Integration — xUnit + Testcontainers)│
└────────────────────────────────────────────────────────────┘
```

> **No JWT, no OAuth 2.0, no native mobile app. Authentication is out of scope.**

---

## 2. Solution Structure

```
SIRU.sln
├── src/
│   ├── Core/
│   │   ├── SIRU.Core.Domain/
│   │   │   ├── Common/
│   │   │   │   ├── BaseEntity.cs
│   │   │   │   ├── Person.cs
│   │   │   │   ├── Enums/
│   │   │   │   │   ├── CandidateStatus.cs
│   │   │   │   │   └── VacancyStatus.cs
│   │   │   │   ├── Results/
│   │   │   │   │   ├── GenericResult.cs
│   │   │   │   │   └── Result.cs
│   │   │   │   ├── Pagination/
│   │   │   │   │   ├── PaginatedResponse.cs
│   │   │   │   │   └── Pagination.cs
│   │   │   ├── Entities/
│   │   │   │   ├── Vacancy.cs
│   │   │   │   ├── Candidate.cs
│   │   │   │   ├── CandidateToVacancy.cs
│   │   │   │   ├── Employee.cs
│   │   │   │   ├── Position.cs
│   │   │   │   ├── Department.cs
│   │   │   │   ├── EmployeePosition.cs
│   │   │   │   ├── EvaluationCriterion.cs
│   │   │   │   ├── Evaluation.cs
│   │   │   │   └── CriterionInEvaluation.cs
│   │   │   ├── Errors/
│   │   │   │   └── DomainErrors.cs
│   │   │   └── Interfaces/
│   │   │       ├── Repositories/
│   │   │       │   ├── IVacancyRepository.cs
│   │   │       │   ├── ICandidateRepository.cs
│   │   │       │   ├── IApplicationRepository.cs
│   │   │       │   ├── IEmployeeRepository.cs
│   │   │       │   ├── IPositionRepository.cs
│   │   │       │   ├── IDepartmentRepository.cs
│   │   │       │   ├── IEvaluationRepository.cs
│   │   │       │   └── IEvaluationCriterionRepository.cs
│   │   │       └── Services/
│   │   │           └── IRankingService.cs
│   │   │
│   │   └── SIRU.Core.Application/
│   │       ├── Common/
│   │       │   ├── Interfaces/
│   │       │   │   └── IGenericService.cs
│   │       │   └── Mappings/
│   │       │       └── MappingConfig.cs           # Mapster config
│   │       ├── DTOs/
│   │       │   ├── Vacancy/
│   │       │   ├── Candidate/
│   │       │   ├── Employee/
│   │       │   ├── Position/
│   │       │   ├── Department/
│   │       │   ├── Evaluation/
│   │       │   └── Report/
│   │       └── Services/
│   │           ├── VacancyService.cs
│   │           ├── CandidateService.cs
│   │           ├── ApplicationService.cs
│   │           ├── EmployeeService.cs
│   │           ├── PositionService.cs
│   │           ├── DepartmentService.cs
│   │           ├── EvaluationService.cs
│   │           ├── EvaluationCriterionService.cs
│   │           └── ReportService.cs
│   │
│   └── Infrastructure/
│       ├── SIRU.Infrastructure.Persistence/
│       │   ├── Context/
│       │   │   └── AppDbContext.cs
│       │   ├── Configurations/               # IEntityTypeConfiguration<T>
│       │   ├── Migrations/
│       │   └── Repositories/
│       │       ├── VacancyRepository.cs
│       │       ├── CandidateRepository.cs
│       │       ├── ApplicationRepository.cs
│       │       ├── EmployeeRepository.cs
│       │       ├── PositionRepository.cs
│       │       ├── DepartmentRepository.cs
│       │       ├── EvaluationRepository.cs
│       │       └── EvaluationCriterionRepository.cs
│       │
│       ├── SIRU.Infrastructure.Ranking/
│       │   ├── Services/
│       │   │   └── RankingService.cs          # ML.NET TF-IDF implementation
│       │   ├── Helpers/
│       │   │   └── PdfTextExtractor.cs        # PdfPig-based extraction
│       │   └── Background/
│       │       └── RankingBackgroundService.cs
│       │
│       ├── SIRU.Infrastructure.Shared/
│       │   ├── Email/
│       │   │   └── EmailService.cs
│       │   ├── Pdf/
│       │   │   └── PdfReportService.cs        # QuestPDF-based generation
│       │   └── Storage/
│       │       └── FileStorageService.cs
│       │
│       └── SIRU.Infrastructure.Auth/         # Auth stubs — out of scope
│           └── (placeholder)
│
├── Presentation/
│   └── SIRU.Presentation.Api/
│       ├── Controllers/
│       │   ├── VacanciesController.cs
│       │   ├── CandidatesController.cs
│       │   ├── ApplicationsController.cs
│       │   ├── EmployeesController.cs
│       │   ├── PositionsController.cs
│       │   ├── DepartmentsController.cs
│       │   ├── EvaluationsController.cs
│       │   ├── EvaluationCriteriaController.cs
│       │   └── ReportsController.cs
│       ├── Middleware/
│       │   └── GlobalExceptionMiddleware.cs
│       ├── Extensions/
│       │   └── ServiceCollectionExtensions.cs
│       └── Program.cs
│
└── Test/
    └── Sirus.Tests/
        ├── Unit/
        │   ├── Application/
        │   └── Domain/
        ├── Integration/
        │   ├── Repositories/
        │   └── Controllers/
        └── Helpers/
            ├── InMemoryDbHelper.cs
            └── TestcontainersHelper.cs
```

---

## 3. Layer Descriptions

### 3.1 SIRU.Core.Domain — Domain Layer

**No dependencies on other projects.**  
**Responsibility:** Define entities, enums, repository interfaces, service interfaces, and the `Result` pattern.

#### 3.1.1 BaseEntity

```csharp
/// <summary>Base entity.</summary>
public abstract class BaseEntity<TKey>
{
    public TKey Id { get; set; } = default!;
}
```

#### 3.1.2 Result

```csharp
/// <summary>
/// Clase base Result para operaciones que pueden fallar sin retornar un valor específico.
/// Ejemplo: operaciones de guardado, envío de emails, validaciones simples.
/// </summary>
public class Result
{
    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Mensajes de error cuando la operación falla
    /// </summary>
    public ICollection<string> Error { get; }

    /// <summary>
    /// Constructor protegido para forzar el uso de factory methods
    /// Esto garantiza que solo se puedan crear Results válidos
    /// </summary>
    /// <param name="isSuccess">True si la operación fue exitosa</param>
    /// <param name="errors">Mensajes de error (solo requerido cuando isSuccess = false)</param>
    /// <exception cref="InvalidOperationException">
    /// - Si isSuccess=true, pero hay error (estado inconsistente)
    /// - Si isSuccess=false, pero no hay error (fallo sin descripción)
    /// </exception>
    protected Result(bool isSuccess, ICollection<string>? errors = null)
    {
        var errorsList = errors ?? [];
        // Invariante 1: Un éxito no puede tener error
        if (isSuccess && errorsList.Count > 0)
        {
            throw new InvalidOperationException("Success result cannot have error");
        }

        // Invariante 2: Un fallo debe tener descripción del error
        if (!isSuccess && errorsList.Count == 0)
        {
            throw new InvalidOperationException("Failure result must have error");
        }

        IsSuccess = isSuccess;
        Error = errorsList;
    }

    /// <summary>
    /// Factory method para crear un Result exitoso sin valor de retorno
    /// Uso típico: Result.Success() para operaciones como "GuardarUsuario()"
    /// </summary>
    public static Result Success() => new Result(true);

    /// <summary>
    /// Factory method para crear un Result fallido sin valor de retorno
    /// </summary>
    /// <param name="errors">Lista con los errores ocurridos</param>
    public static Result Failure(ICollection<string> errors) => new Result(false, errors);

    /// <summary>
    /// Factory method para crear un Result exitoso con valor de retorno
    /// Uso típico: Result.Success(usuario) para operaciones como "ObtenerUsuario()"
    /// </summary>
    /// <typeparam name="T">Tipo del valor de retorno</typeparam>
    /// <param name="value">Valor a retornar en caso de éxito</param>
    public static Result<T> Success<T>(T value) => new Result<T>(true, null, value);

    /// <summary>
    /// Factory method para crear un Result fallido con tipo específico
    /// </summary>
    /// <typeparam name="T">Tipo que debería retornar en caso de éxito</typeparam>
    /// <param name="errors">Lista de errores</param>
    public static Result<T> Failure<T>(List<string> errors) => new Result<T>(false, errors);
}

/// <summary>
/// Result genérico que encapsula tanto el éxito/fallo como un valor de retorno.
/// Hereda de Result base para mantener las propiedades IsSuccess, IsFailure y Error.
/// 
/// CUÁNDO USAR:
/// - Operaciones que retornan un valor: ObtenerUsuario(), CalcularDescuento(), etc.
/// - Conversiones: ConvertirAEntero(), ParsearFecha(), etc.
/// - Búsquedas: BuscarPorId(), EncontrarPrimero(), etc.
/// </summary>
/// <typeparam name="T">Tipo del valor que se retorna en caso de éxito</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Valor retornado en caso de éxito.
    /// IMPORTANTE: Solo acceder a este valor después de verificar IsSuccess = true
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Constructor interno para forzar el uso de factory methods
    /// </summary>
    /// <param name="value">Valor a almacenar</param>
    /// <param name="isSuccess">Estado de la operación</param>
    /// <param name="errors">Errores en caso de fallos</param>
    internal Result(bool isSuccess, ICollection<string>? errors, T? value = default) : base(isSuccess, errors)
    {
        Value = value;
    }
}
```

#### 3.1.3 Key Enums

```csharp
public enum CandidateStatus { Pending, UnderEvaluation, Hired, Rejected }
public enum VacancyStatus   { Open, Closed, Cancelled }
```

#### 3.1.4 IRankingService (Domain Interface)

```csharp
/// <summary>Abstraction for CV-to-vacancy NLP scoring.</summary>
public interface IRankingService
{
    Task<OperationResult<float>> ComputeScoreAsync(string cvText, string vacancyText);
}
```

---

### 3.2 SIRU.Core.Application — Application Layer

**Depends on:** `SIRU.Core.Domain`  
**Responsibility:** Define DTOs, application services, generic service contract, and Mapster mapping configuration.

#### 3.2.1 Generic Service Interface

```csharp
/// <summary>Generic CRUD service contract.</summary>
public interface IGenericService<TEntity, TDto, TCreateDto, TUpdateDto, TKey>
    where TEntity : BaseEntity<TKey>
{
    Task<Result<TDto>> GetByIdAsync(TKey id);
    Task<Result<IEnumerable<TDto>>> GetAllAsync();
    Task<Result<TDto>> CreateAsync(TCreateDto createDto);
    Task<Result<TDto>> UpdateAsync(TKey id, TUpdateDto updateDto);
    Task<Result> DeleteAsync(TKey id);
}
```

#### 3.2.2 Mapster Configuration

```csharp
/// <summary>Centralized Mapster mapping configuration registered at startup.</summary>
public static class MappingConfig
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<Vacancy, VacancyDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString());

        TypeAdapterConfig<CandidateToVacancy, ApplicationDto>.NewConfig()
            .Map(dest => dest.CandidateName,
                 src => $"{src.Candidate.FirstName} {src.Candidate.LastName}");

        // Register all remaining mappings here...
        TypeAdapterConfig.GlobalSettings.Compile();
    }
}
```

**Key rules for the Application layer:**
- Services must never reference `DbContext` directly; all data access goes through repository interfaces.
- All service methods must return `Result<T>` or `Result`.
- Mapping between entities and DTOs must use Mapster's `Adapt<T>()` extension; no manual property mapping.

---

### 3.3 SIRU.Infrastructure.Persistence — Data Access

**Depends on:** `SIRU.Core.Application`, `SIRU.Core.Domain`  
**Responsibility:** Implement `AppDbContext`, entity configurations, EF Core migrations, and repository implementations.

#### 3.3.1 AppDbContext

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Vacancy> Vacancies => Set<Vacancy>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<CandidateToVacancy> CandidatesToVacancies => Set<CandidateToVacancy>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<EmployeePosition> EmployeePositions => Set<EmployeePosition>();
    public DbSet<EvaluationCriterion> EvaluationCriteria => Set<EvaluationCriterion>();
    public DbSet<Evaluation> Evaluations => Set<Evaluation>();
    public DbSet<CriterionInEvaluation> CriteriaInEvaluations => Set<CriterionInEvaluation>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

**Key rules for the Persistence layer:**
- Never expose `IQueryable<T>` outside a repository method; projections are done inside the repository.
- Always use async EF Core methods (`ToListAsync`, `SaveChangesAsync`, etc.).
- Use explicit `Include` / `ThenInclude` for related data; lazy loading is disabled.
- String-keyed entities use ULID generated in the entity constructor.
- PostgreSQL provider: `Npgsql.EntityFrameworkCore.PostgreSQL`.

---

### 3.4 SIRU.Infrastructure.Ranking — NLP Module

**Depends on:** `SIRU.Core.Application`, `SIRU.Core.Domain`

#### NLP Library Choice: Microsoft.ML (ML.NET)

After evaluating available .NET-native NLP options, **Microsoft.ML (ML.NET)** is the recommended library for this project for the following reasons:

| Criterion | Microsoft.ML (ML.NET) | Reason |
|-----------|----------------------|--------|
| .NET Native | ✅ First-class .NET 10 support | No Python interop needed |
| TF-IDF + Cosine Similarity | ✅ Built-in text featurization + cosine distance | Directly matches the use case |
| Sentence Similarity API | ✅ NAS-BERT transformer model via `Microsoft.ML.TorchSharp` | Semantic similarity option |
| NuGet Package | `Microsoft.ML` (v5.x) | Stable, MIT licensed |
| Offline / No External Service | ✅ Fully in-process | No API costs or latency |
| PDF Text Support | Combined with `PdfPig` | Covers the full pipeline |

For this project, the implementation uses **TF-IDF vectorization followed by cosine similarity**, which is computationally lightweight and effective for structured text matching (skills, job titles, experience). If higher semantic accuracy is needed in a future iteration, the `Microsoft.ML.TorchSharp` sentence similarity API can be swapped in without changing the `IRankingService` interface.

#### RankingService Implementation Sketch

```csharp
/// <summary>Computes TF-IDF cosine similarity between CV text and vacancy text.</summary>
public class RankingService : IRankingService
{
    private readonly MLContext _mlContext = new();

    public Task<OperationResult<float>> ComputeScoreAsync(string cvText, string vacancyText)
    {
        try
        {
            var data = new[]
            {
                new TextPair { Text1 = cvText,       Text2 = vacancyText },
                new TextPair { Text1 = vacancyText,  Text2 = vacancyText }  // anchor
            };

            var dataView = _mlContext.Data.LoadFromEnumerable(data);

            var pipeline = _mlContext.Transforms.Text
                .FeaturizeText("Features1", nameof(TextPair.Text1))
                .Append(_mlContext.Transforms.Text
                    .FeaturizeText("Features2", nameof(TextPair.Text2)));

            var transformed = pipeline.Fit(dataView).Transform(dataView);
            // Cosine similarity computation from the feature vectors
            float score = ComputeCosineSimilarity(transformed);

            return Task.FromResult(Result<float>.Success(Math.Clamp(score, 0f, 1f)));
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                Result<float>.Failure($"Ranking computation failed: {ex.Message}"));
        }
    }

    private static float ComputeCosineSimilarity(IDataView transformed) { /* ... */ }
}
```

#### PDF Text Extraction

```csharp
// Uses PdfPig NuGet package (UglyToad.PdfPig)
public static string ExtractText(string filePath)
{
    using var document = PdfDocument.Open(filePath);
    return string.Join(" ", document.GetPages().SelectMany(p => p.GetWords()));
}
```

---

### 3.5 SIRU.Infrastructure.Shared — Cross-Cutting Services

| Component | Library | Purpose |
|-----------|---------|---------|
| `PdfReportService` | `QuestPDF` | Generate downloadable PDF reports |
| `FileStorageService` | System.IO | Store and retrieve uploaded CV files |
| `EmailService` | `MailKit` | Send email notifications (future use) |

---

### 3.6 SIRU.Presentation.Api — Presentation Layer

**Technology:** ASP.NET Core 10 API + Controller-based routing  
**Responsibility:** Handle HTTP requests, validate input, call application services, return standardized JSON responses.

**Key rules:**
- Controllers must be thin — no business logic, no direct repository or DbContext access.
- All action methods must inspect `Result` and map to appropriate HTTP status codes.
- A `GlobalExceptionMiddleware` catches unhandled exceptions and returns a generic `500` response.
- OpenAPI / Swagger is configured via `Swashbuckle.AspNetCore` and exposed at `/swagger` in development.
- All list endpoints support `page` and `pageSize` query parameters.

**DI Registration:**

```csharp
// Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    // Mapster
    MappingConfig.RegisterMappings();
    services.AddMapster();

    // Application services
    services.AddScoped<IVacancyService, VacancyService>();
    services.AddScoped<ICandidateService, CandidateService>();
    services.AddScoped<IApplicationService, ApplicationService>();
    services.AddScoped<IEmployeeService, EmployeeService>();
    services.AddScoped<IEvaluationService, EvaluationService>();
    services.AddScoped<IReportService, ReportService>();
    // ... remaining services

    // Repositories
    services.AddScoped<IVacancyRepository, VacancyRepository>();
    // ... remaining repositories

    // Infrastructure
    services.AddScoped<IRankingService, RankingService>();
    services.AddScoped<IPdfReportService, PdfReportService>();
    services.AddScoped<IFileStorageService, FileStorageService>();
    services.AddHostedService<RankingBackgroundService>();

    return services;
}
```

---

### 3.7 SIRU.Tests — Test Layer

**Technology:** xUnit + Moq + EF Core InMemory + Testcontainers  
**Responsibility:** Verify correctness of every method in the Application and Infrastructure layers.

**Rules:**
- Every method must have at least one **success** test and one **failure** test.
- Unit tests use EF Core InMemory or Moq-mocked repositories.
- Integration tests use **Testcontainers** (PostgreSQL container) to verify repository behavior against a real engine.
- Tests follow the **Arrange / Act / Assert** pattern.
- Test method naming: `MethodName_StateUnderTest_ExpectedBehavior`.

**InMemory Helper:**

```csharp
public static class InMemoryDbHelper
{
    public static AppDbContext CreateContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
```

---

## 4. Dependency Flow

```
SIRU.Presentation.Api           ──►  SIRU.Infrastructure.*
SIRU.Presentation.Api           ──►  SIRU.Core.Application
SIRU.Infrastructure.* ──►  SIRU.Core.Application
SIRU.Infrastructure.* ──►  SIRU.Core.Domain
SIRU.Core.Application   ──►  SIRU.Core.Domain
SIRU.Tests         ──►  SIRU.Presentation.Api
SIRU.Tests         ──►  SIRU.Core.Application
SIRU.Tests         ──►  SIRU.Infrastructure.*
SIRU.Core.Domain        ──►  (none)
```

---

## 5. Key NuGet Packages

| Package | Version | Layer | Purpose |
|---------|---------|-------|---------|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.x | Persistence | PostgreSQL EF Core provider |
| `Mapster` | 7.x | Application | Object mapping |
| `MapsterMapper` | 7.x | Application | DI integration for Mapster |
| `Microsoft.ML` | 5.x | Ranking | TF-IDF text featurization |
| `Microsoft.ML.TorchSharp` | 0.x | Ranking | Transformer sentence similarity (optional upgrade) |
| `UglyToad.PdfPig` | 0.1.x | Ranking | PDF text extraction |
| `QuestPDF` | 2024.x | Shared | PDF report generation |
| `MailKit` | 4.x | Shared | Email sending |
| `Swashbuckle.AspNetCore` | 6.x | API | OpenAPI / Swagger |
| `xunit` | 2.x | Tests | Unit and integration testing |
| `Moq` | 4.x | Tests | Mocking dependencies |
| `Testcontainers.PostgreSql` | 3.x | Tests | Integration test DB |

---

## 6. Configuration Reference

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=SirusDb;Username=sirus_user;Password=..."
  },
  "FileStorage": {
    "CvBasePath": "/var/sirus/cv-uploads",
    "MaxFileSizeMb": 10
  }
}
```

> Connection strings and secrets must never be committed to source control. Use environment variables or .NET User Secrets in development.

---

## 7. Order Status Lifecycles

### Vacancy Lifecycle
```
Open ──► Closed     (automatic when a candidate is hired)
Open ──► Cancelled  (manual by HR Administrator)
```

### Candidate Application Lifecycle
```
Pending ──► UnderEvaluation ──► Hired
                            └──► Rejected
```
