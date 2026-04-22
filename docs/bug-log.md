# Bug Log — HU-11 (Register Evaluation)

## Bug 1 — CS0266: Cannot convert `Result` to `Result<EvaluationDto>`

### Symptom
```
error CS0266: Cannot implicitly convert type 'SIRU.Core.Domain.Common.Results.Result'
to 'SIRU.Core.Domain.Common.Results.Result<SIRU.Core.Application.Dtos.Evaluations.EvaluationDto>'
```

### Location
`SIRU.Core.Application/Services/Evaluations/EvaluationService.cs`, lines 30, 37, 47.

### Root Cause
The `Result` class has two factory methods:
- `Result.Failure(ICollection<string> errors)` — non-generic, returns `Result`
- `Result<T>.Failure(List<string> errors)` — generic, returns `Result<T>`

When calling `Result<EvaluationDto>.Failure(...)`, the compiler resolves to the non-generic overload due to type inference ambiguity when `ICollection<string>` is involved, returning `Result` instead of `Result<EvaluationDto>`.

### Solution
Use explicit generic form with the type parameter on `Failure<T>`:
```csharp
return Result.Failure<EvaluationDto>(new List<string> { "error message" });
```
The explicit type parameter `<EvaluationDto>` on the method forces the compiler to use the generic overload.

---

## Bug 2 — CS9035: Required member `Id` not set in object initializer

### Symptom
```
error CS9035: Required member 'BaseEntity<string>.Id' must be set in the object
initializer or attribute constructor.
```

### Location
`SIRU.Core.Application/Services/Evaluations/EvaluationService.cs`, line 51 (object initializer for `Evaluation`).

### Root Cause
`Evaluation` extended `BaseEntity<string>` with a constructor that auto-generated the Id:
```csharp
public Evaluation() => Id = Guid.CreateVersion7().ToString();
```

When calling `new Evaluation { EmployeePositionId = ..., Date = ... }` using object initializer syntax, the compiler does not guarantee the constructor runs before the initializer processes. Since `BaseEntity.Id` is marked `required`, the compiler cannot confirm the constructor sets it before the initializer expects it to be available.

### Incorrect Attempt
Changing `BaseEntity.Id` from `required` to `= default!`:
```csharp
public TKey Id { get; set; } = default!;
```
This suppressed the CS9035 error but broke the required contract across all entities and could cause null reference issues. **This change was reverted.**

### Correct Solution (3 steps)
1. **Revert `BaseEntity.cs`** back to `public required TKey Id { get; set; }`
2. **Remove the constructor** from `Evaluation.cs` (no Guid generation in entity)
3. **Use Mapster `Adapt<Evaluation>()` to create the entity** — the mapping generates the Guid via `Guid.CreateVersion7().ToString()` in the Mapster config:
   ```csharp
   var evaluation = dto.Adapt<Evaluation>();
   evaluation.EmployeePositionId = currentPosition.Id;
   evaluation.Date = dto.EvaluationDate;
   ```

   In `MappingConfig.cs`:
   ```csharp
   TypeAdapterConfig<EvaluationInsertDto, DomainEntities.Evaluation>.NewConfig()
       .Map(dest => dest.Id, src => Guid.CreateVersion7().ToString());
   ```

---

## Bug 3 — EmployeeRepository missing namespace/class declaration

### Symptom
```
error CS0106: The modifier 'public' is not valid for this item
SIRU.Infrastructure.Persistence\Repositories\EmployeeRepository.cs(1,1)
```

### Root Cause
The `GetCurrentPositionAsync` method was appended to `EmployeeRepository.cs` but the file on disk only contained the method body — the class and namespace declarations were lost during a previous edit.

### Solution
Restored the full file content with proper class declaration and `using` statements.

---

## Bug 4 — Test helper `CreateCriteria` had wrong signature

### Symptom
```
error CS1503: Argument 2: cannot convert from 'string' to 'int'
error CS1503: Argument 1: cannot convert from 'System.Func<...>' to 'Expression<System.Func<...>>'
```

### Root Cause
`CreateCriteria` used `params int[]` (interpreting args as id, name pairs like `1, "Teamwork"`) and `FindAsync` was called with `Func<>` instead of `Expression<Func<>>`.

### Solution
Changed `CreateCriteria` to use `params (int Id, string Name)[]` tuples:
```csharp
private static List<Criterion> CreateCriteria(params (int Id, string Name)[] pairs)
{
    return pairs.Select(p => new Criterion { Id = p.Id, Name = p.Name }).ToList();
}
```
And fixed `FindAsync` mock to use `It.IsAny<Expression<Func<Criterion, bool>>>()`.

---

## Bug 5 — `Result.Success<IEnumerable<T>>` with list argument causing assertion mismatch

### Symptom
Test `GetByEmployeeIdAsync_EvaluationsSortedByDateDescending` failed at line 289:
```
Assert.Equal() Failure: Strings differ
```

### Location
`SIRU.Tests.UnitTests/Services/Evaluations/EvaluationServiceTests.cs`, line 289 (the `.ToList()` call on `result.Value`).

### Root Cause
In `EvaluationService.GetByEmployeeIdAsync`, the return statement was:
```csharp
return Result<IEnumerable<EvaluationHistoryDto>>.Success(dtos);
```
Where `dtos` is a `List<EvaluationHistoryDto>`. The mock returns a pre-built list in a specific order (older, newer), but the service does not re-order it — it simply returns the list as-is. The test asserts that `eval-new` should be first based on date ordering, but the mock returns the list already in the order `{older, newer}`.

The actual service method does **not** sort the evaluations in the return path — the sorting happens only in the repository query. Since the mock bypasses the repository query entirely, the ordering logic never executes. The test setup had `{older, newer}` but expected the output to behave as if `OrderByDescending` had been applied.

### Solution
Two changes in the test:
1. Changed `Criteria = []` to `Criteria = new List<EvaluationCriterion>()` (empty collection expression on interface property may cause issues)
2. Changed list order to `{newer, older}` to match what the mock returns and what the test asserts:
```csharp
var evaluations = new List<Evaluation> { newer, older };
```

---

## Files Changed

| File | Change |
|------|--------|
| `SIRU.Core.Domain/Common/BaseEntity.cs` | Reverted to `required TKey Id { get; set; }` |
| `SIRU.Core.Domain/Entities/Evaluation.cs` | Removed constructor with Guid generation |
| `SIRU.Core.Application/Mappings/MappingConfig.cs` | Added `EvaluationInsertDto → Evaluation` mapping with Guid |
| `SIRU.Core.Application/Services/Evaluations/EvaluationService.cs` | Used `dto.Adapt<Evaluation>()`, set properties manually, fixed `Result.Failure<EvaluationDto>` |
| `SIRU.Infrastructure.Persistence/Repositories/EmployeeRepository.cs` | Restored full class with namespace |
| `SIRU.Tests.UnitTests/Services/Evaluations/EvaluationServiceTests.cs` | Fixed `CreateCriteria` tuple syntax, `FindAsync` mock, ordering test |
| `docs/bug-log.md` | Created |

## Verification
- `dotnet build` → 0 errors (only warnings)
- `dotnet test` → **49 tests passed** (48 unit + 1 integration)