# Frontend SRS — SIRUS SPA

**Version:** 1.0
**Tech Stack:** Angular 21, Tailwind CSS, daisyUI, jwtDecode
**Backend Reference:** `api-reference.md`
**Date:** 2025

---

## 1. Introduction

### 1.1 Purpose

This document defines the frontend architecture, technical decisions, and implementation guidelines for the SIRUS Single Page Application (SPA). It is the authoritative reference for Angular developers building the SIRUS client.

### 1.2 Scope

The SPA consumes the SIRUS REST API defined in `api-reference.md`. It provides the user interface for all modules: recruitment/vacancies, candidates, employees, positions/departments, performance evaluations, reports, and account management.

### 1.3 Tech Stack

| Technology | Purpose |
|------------|---------|
| **Angular 21** | SPA framework |
| **Tailwind CSS** | Utility-first CSS framework |
| **daisyUI** | Component library on top of Tailwind |
| **jwtDecode** | Decoding and parsing JWT tokens |
| **Angular Signals** | Reactive state management |
| **Angular Router** | Client-side routing with lazy loading |

---

## 2. Project Structure

```
src/
├── app/
│   ├── core/                          # Singleton services, interceptors, guards
│   │   ├── interceptors/
│   │   │   └── auth.interceptor.ts  # Attaches JWT to all HTTP requests
│   │   ├── guards/
│   │   │   ├── auth.guard.ts        # Redirects unauthenticated users to login
│   │   │   ├── admin.guard.ts       # Restricts to Admin role only
│   │   │   └── supervisor.guard.ts  # Restricts to Supervisor or Admin role
│   │   ├── models/                  # TypeScript interfaces matching backend DTOs
│   │   │   ├── vacancy.models.ts
│   │   │   ├── candidate.models.ts
│   │   │   ├── employee.models.ts
│   │   │   ├── evaluation.models.ts
│   │   │   ├── report.models.ts
│   │   │   ├── account.models.ts
│   │   │   └── auth.models.ts
│   │   └── services/
│   │       ├── api.service.ts       # Typed HttpClient wrapper
│   │       ├── auth.service.ts     # JWT management, login, logout, refresh
│   │       └── toast.service.ts    # daisyUI toast notifications
│   │
│   ├── shared/                      # Reusable components, pipes, directives
│   │   ├── components/
│   │   │   ├── data-table/          # Paginated table with daisyUI
│   │   │   ├── status-badge/       # Vacancy/candidate/evaluation status badge
│   │   │   ├── score-badge/        # Numeric score display (0.0–5.0)
│   │   │   ├── confirm-dialog/      # daisyUI modal for delete confirmations
│   │   │   └── form-field/         # Input wrapper with validation display
│   │   └── pipes/
│   │       └── date-format.pipe.ts  # Formats ISO dates for display
│   │
│   ├── features/                    # Lazy-loaded feature modules
│   │   ├── auth/                   # Login, logout (lazy)
│   │   ├── vacancies/              # Vacancy list, detail, CRUD (lazy)
│   │   ├── candidates/             # Candidate management (lazy)
│   │   ├── employees/              # Employee CRUD, history, evaluations (lazy)
│   │   ├── positions/              # Position & Department management (lazy)
│   │   ├── evaluations/            # Evaluation criteria, create/view evaluations (lazy)
│   │   └── reports/               # All reports + PDF export (lazy)
│   │
│   ├── layouts/
│   │   ├── main-layout.component.ts       # Authenticated layout (sidebar + header)
│   │   ├── public-layout.component.ts     # Public layout (header only)
│   │   └── auth-layout.component.ts      # Centered card layout for login
│   │
│   └── app.routes.ts              # Route definitions with lazy loading
│
├── environments/
│   ├── environment.ts             # Development: apiUrl = 'http://localhost:5000/api'
│   └── environment.prod.ts        # Production API URL
│
└── styles.css                     # Tailwind + daisyUI imports
```

---

## 3. Authentication Architecture

### 3.1 JWT Token Storage

- **Access token** is stored in `localStorage` under the key `sirus_access_token`
- **Refresh token** is stored in `localStorage` under the key `sirus_refresh_token`
- **Cookie-based refresh token** is also managed by the backend (HttpOnly cookie set on login)

### 3.2 JWT Claims

The backend `GenerateJwtToken` method (lines 357–389 in `AuthService.cs`) produces tokens with the following claims:

| Claim | Type | Description |
|-------|------|-------------|
| `sub` | `string` | Account ID |
| `uid` | `string` | Account ID (same as `sub`) |
| `jti` | `string` | Unique JWT ID (for token revocation tracking) |
| `email` | `string` | Account email |
| `fullName` | `string` | Full name (e.g., "Jane Smith") |
| `roles` | `string[]` | One claim per role — e.g., `["Admin"]` or `["Supervisor"]` |
| `exp` | `number` | Expiration Unix timestamp |

> **Note:** The `roles` claim is added once per role. If a user has multiple roles, there are multiple `roles` claims. Use `jwtDecode` to parse and access these.

### 3.3 AuthService

The `AuthService` (core/services/auth.service.ts) manages:

- `login(email, password)` — Calls `POST /api/v1/auth/login`, stores tokens in localStorage, sets refresh token cookie
- `logout()` — Calls `POST /api/v1/auth/logout`, clears localStorage and cookies, redirects to login
- `refreshToken()` — Calls `POST /api/v1/auth/refresh` with current access token + refresh token cookie
- `getCurrentUser()` — Decodes the stored access token with `jwtDecode`, returns a signal with the user claims
- `hasRole(role: string)` — Checks if the decoded token contains the specified role
- `isAuthenticated()` — Returns `true` if a valid (non-expired) token exists in localStorage

```typescript
// Example: AuthService signal-based user state
currentUser = signal<JwtPayload | null>(null);

login(email: string, password: string): Observable<boolean> { ... }
logout(): Observable<void> { ... }
refreshToken(): Observable<void> { ... }
hasRole(role: string): boolean {
  const payload = this.currentUser();
  return payload?.roles?.includes(role) ?? false;
}
```

### 3.4 AuthInterceptor

A functional HTTP interceptor (`auth.interceptor.ts`) attaches the JWT to all outgoing HTTP requests:

```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('sirus_access_token');
  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }
  return next(req);
};
```

The interceptor also handles `401 Unauthorized` responses globally:
- If a `401` is received, clear tokens and redirect to `/auth/login`

### 3.5 Route Guards

| Guard | Purpose |
|-------|---------|
| `AuthGuard` | Allows only authenticated users; redirects to `/auth/login` otherwise |
| `AdminGuard` | Allows only users with `Admin` role; redirects to `/` otherwise |
| `SupervisorGuard` | Allows users with `Admin` or `Supervisor` role; redirects to `/` otherwise |

---

## 4. Page Architecture & Access Control

### 4.1 Public Pages (No Authentication Required)

These pages are accessible without a JWT token. No route guard is applied.

| Route | Page | Description |
|-------|------|-------------|
| `/vacancies` | Vacancy List | Lists all open vacancies |
| `/vacancies/:id` | Vacancy Detail | Shows vacancy details and apply button |
| `/vacancies/:id/apply` | Candidate Application Form | Uploads CV and applies to a vacancy |

### 4.2 Supervisor Pages (Authenticated + Supervisor or Admin Role)

Accessible to users with `Supervisor` or `Admin` role.

| Route | Page | Description |
|-------|------|-------------|
| `/evaluations/create` | Create Evaluation | Register a new performance evaluation |
| `/evaluations/history` | Evaluation History | View own employees' evaluation history |
| `/auth/me/sessions` | My Sessions | View and revoke own active sessions |

### 4.3 HR Admin Pages (Authenticated + Admin Role Only)

Accessible only to users with `Admin` role.

| Route | Page | Description |
|-------|------|-------------|
| `/` | Dashboard | Overview with quick stats |
| `/vacancies/manage` | Manage Vacancies | CRUD for vacancies |
| `/candidates` | Candidates | View and manage all candidates |
| `/employees` | Employees | Employee CRUD + history + evaluations |
| `/positions` | Positions | Position and department management |
| `/evaluation-criteria` | Evaluation Criteria | CRUD for evaluation criteria |
| `/reports` | Reports | Hiring time, performance by department, employee report |
| `/reports/employees/export` | — | Downloads employee report PDF |
| `/reports/hiring-time/export` | — | Downloads hiring time report PDF |
| `/reports/performance-by-department/export` | — | Downloads performance by department report PDF |
| `/accounts` | Account Management | User account CRUD (Admin only) |

### 4.4 Auth Pages

| Route | Page | Description |
|-------|------|-------------|
| `/auth/login` | Login | Login form |
| `/auth/forgot-password` | Forgot Password | Request password reset email |
| `/auth/reset-password` | Reset Password | Set new password with token |

---

## 5. API Integration

### 5.1 Base URLs

| Environment | API Base URL | Auth Base URL |
|-------------|--------------|---------------|
| Development | `http://localhost:5000/api` | `http://localhost:5000/api/v1` |
| Production | Configure via environment variables | Configure via environment variables |

### 5.2 ApiService

A typed wrapper around `HttpClient` (`core/services/api.service.ts`) provides methods for each resource:

```typescript
@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  // Vacancies
  getVacancies(): Observable<VacantDto[]> { ... }
  getVacancy(id: string): Observable<VacantDto> { ... }
  createVacancy(dto: SaveVacantDto): Observable<VacantDto> { ... }
  updateVacancy(id: string, dto: UpdateVacantDto): Observable<void> { ... }
  deleteVacancy(id: string): Observable<void> { ... }
  applyToVacancy(vacancyId: string, dto: VacancyApplicationDto): Observable<VacancyApplicationResultDto> { ... }
  getVacancyApplications(vacancyId: string, page = 1, pageSize = 10): Observable<PaginatedResponse<VacancyApplicationResultDto>> { ... }
  recalculateScores(vacancyId: string): Observable<void> { ... }

  // Candidates
  getCandidates(): Observable<CandidateDto[]> { ... }
  getCandidate(id: string): Observable<CandidateDto> { ... }
  createCandidate(dto: CandidateInsertDto): Observable<CandidateDto> { ... }
  updateCandidate(id: string, dto: CandidateUpdateDto): Observable<void> { ... }
  deleteCandidate(id: string): Observable<void> { ... }

  // Employees
  getEmployees(page = 1, pageSize = 10, isActive?: boolean): Observable<PaginatedResponse<EmployeeListDto>> { ... }
  getEmployee(id: string): Observable<EmployeeDto> { ... }
  createEmployee(dto: EmployeeInsertDto): Observable<EmployeeDto> { ... }
  updateEmployee(id: string, dto: EmployeeUpdateDto): Observable<EmployeeDto> { ... }
  deleteEmployee(id: string): Observable<void> { ... }
  getEmployeeHistory(id: string): Observable<EmployeeHistoryDto[]> { ... }
  getEmployeeEvaluations(id: string): Observable<EvaluationHistoryDto[]> { ... }
  assignPosition(employeeId: string, dto: EmployeePositionInsertDto): Observable<EmployeePositionDto> { ... }

  // Positions
  getPositions(): Observable<PositionDto[]> { ... }
  getPosition(id: number): Observable<PositionDto> { ... }
  createPosition(dto: PositionInsertDto): Observable<PositionDto> { ... }
  updatePosition(id: number, dto: PositionUpdateDto): Observable<void> { ... }
  deletePosition(id: number): Observable<void> { ... }

  // Departments
  getDepartments(): Observable<DepartmentDto[]> { ... }
  getDepartment(id: number): Observable<DepartmentDto> { ... }
  createDepartment(dto: DepartmentInsertDto): Observable<DepartmentDto> { ... }
  updateDepartment(id: number, dto: DepartmentUpdateDto): Observable<void> { ... }
  deleteDepartment(id: number): Observable<void> { ... }

  // Evaluations
  createEvaluation(dto: EvaluationInsertDto): Observable<EvaluationDto> { ... }

  // Evaluation Criteria
  getCriteria(): Observable<CriterionDto[]> { ... }
  getCriterion(id: number): Observable<CriterionDto> { ... }
  createCriterion(dto: CriterionInsertDto): Observable<CriterionDto> { ... }
  updateCriterion(id: number, dto: CriterionUpdateDto): Observable<CriterionDto> { ... }
  deleteCriterion(id: number): Observable<void> { ... }

  // Reports
  getHiringTimeReport(): Observable<HiringTimeReportDto> { ... }
  getPerformanceByDepartment(): Observable<DepartmentPerformanceDto[]> { ... }
  getEmployeeReport(page = 1, pageSize = 10, isActive?: boolean): Observable<PaginatedResponse<EmployeeReportDto>> { ... }
  exportHiringTimePdf(): Observable<Blob> { ... }
  exportPerformanceByDepartmentPdf(): Observable<Blob> { ... }
  exportEmployeesPdf(): Observable<Blob> { ... }

  // Auth
  login(email: string, password: string): Observable<string> { ... }
  refreshToken(): Observable<string> { ... }
  logout(): Observable<void> { ... }
  forgotPassword(email: string, origin: string): Observable<void> { ... }
  resetPassword(dto: ResetPasswordDto): Observable<void> { ... }
  getMySessions(page = 1, pageSize = 10): Observable<PaginatedResponse<UserSessionDto>> { ... }
  revokeMySession(sessionId: string): Observable<void> { ... }
  getAccountMe(): Observable<GetAccountDto> { ... }
}
```

### 5.3 Error Handling

All HTTP errors are handled centrally. The interceptor catches errors and transforms them into a consistent format:

```typescript
// Error response shape (matches backend ProblemDetails)
interface ApiError {
  statusCode: number;
  title: string;
  detail: string;
  instance: string;
}
```

- `400 Bad Request` — Show validation error messages from `detail`
- `401 Unauthorized` — Clear tokens, redirect to login
- `403 Forbidden` — Show "Access denied" toast
- `404 Not Found` — Show "Resource not found" toast
- `409 Conflict` — Show business rule violation message from `detail`
- `500 Internal Server Error` — Show generic error toast

### 5.4 PDF Export

PDF endpoints return `Content-Type: application/pdf`. Use Angular's `HttpClient` with `responseType: 'blob'`:

```typescript
downloadPdf(endpoint: string, filename: string): Observable<void> {
  return this.http.get(`${this.base}${endpoint}`, {
    responseType: 'blob'
  }).pipe(
    tap(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = filename;
      a.click();
      window.URL.revokeObjectURL(url);
    })
  );
}
```

---

## 6. UI Component Inventory

### 6.1 Layout Components

| Component | Description |
|-----------|-------------|
| `MainLayoutComponent` | Authenticated layout with sidebar navigation and top header bar |
| `PublicLayoutComponent` | Public layout with minimal header (logo + nav links) |
| `AuthLayoutComponent` | Centered card layout for login/forgot-password pages |

### 6.2 Shared UI Components

| Component | Description |
|-----------|-------------|
| `DataTableComponent` | Generic paginated table with daisyUI styling, sorting, and row actions |
| `StatusBadgeComponent` | Color-coded badge for vacancy status (`Open`/`Closed`/`Cancelled`), candidate status, evaluation status |
| `ScoreBadgeComponent` | Displays a numeric score (0.0–5.0) with color gradient (red → yellow → green) |
| `ConfirmDialogComponent` | daisyUI modal wrapper for delete confirmation dialogs |
| `FormFieldComponent` | Input wrapper that displays backend validation error messages |
| `DateFormatPipe` | Converts ISO 8601 date strings to locale-appropriate display format |

### 6.3 Page Components

| Page | Route | Auth |
|------|-------|------|
| `VacancyListComponent` | `/vacancies` | Public |
| `VacancyDetailComponent` | `/vacancies/:id` | Public |
| `VacancyApplyComponent` | `/vacancies/:id/apply` | Public |
| `LoginComponent` | `/auth/login` | — |
| `DashboardComponent` | `/` | Admin |
| `VacancyManageListComponent` | `/vacancies/manage` | Admin |
| `VacancyFormComponent` | `/vacancies/manage/new`, `/vacancies/manage/:id/edit` | Admin |
| `CandidatesListComponent` | `/candidates` | Admin |
| `EmployeesListComponent` | `/employees` | Admin |
| `EmployeeDetailComponent` | `/employees/:id` | Admin |
| `EmployeeFormComponent` | `/employees/new`, `/employees/:id/edit` | Admin |
| `PositionsListComponent` | `/positions` | Admin |
| `DepartmentsListComponent` | `/departments` | Admin |
| `EvaluationCriteriaListComponent` | `/evaluation-criteria` | Admin |
| `EvaluationFormComponent` | `/evaluations/create` | Supervisor+ |
| `EvaluationHistoryComponent` | `/evaluations/history` | Supervisor+ |
| `ReportsListComponent` | `/reports` | Admin |
| `AccountsListComponent` | `/accounts` | Admin |
| `MySessionsComponent` | `/auth/me/sessions` | Auth |

---

## 7. Route Configuration

```typescript
// app.routes.ts
export const routes: Routes = [
  // Public routes
  { path: 'vacancies', loadChildren: () => import('./features/vacancies/routes'), canActivate: [] },
  { path: 'auth', loadChildren: () => import('./features/auth/routes'), canActivate: [] },

  // Authenticated routes (any valid role)
  { path: '', component: MainLayoutComponent, canActivate: [AuthGuard], children: [
    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component') },
    { path: 'evaluations/create', loadComponent: () => import('./features/evaluations/evaluation-form.component'), canActivate: [SupervisorGuard] },
    { path: 'evaluations/history', loadComponent: () => import('./features/evaluations/evaluation-history.component'), canActivate: [SupervisorGuard] },
    { path: 'auth/me/sessions', loadComponent: () => import('./features/auth/my-sessions.component') },
    { path: 'accounts', loadComponent: () => import('./features/accounts/accounts-list.component'), canActivate: [AdminGuard] },
    { path: 'reports', loadChildren: () => import('./features/reports/routes'), canActivate: [AdminGuard] },
    { path: 'vacancies/manage', loadChildren: () => import('./features/vacancies/manage/routes'), canActivate: [AdminGuard] },
    { path: 'candidates', loadChildren: () => import('./features/candidates/routes'), canActivate: [AdminGuard] },
    { path: 'employees', loadChildren: () => import('./features/employees/routes'), canActivate: [AdminGuard] },
    { path: 'positions', loadChildren: () => import('./features/positions/routes'), canActivate: [AdminGuard] },
    { path: 'evaluation-criteria', loadChildren: () => import('./features/evaluations/criteria/routes'), canActivate: [AdminGuard] },
  ]},

  // Redirects
  { path: '**', redirectTo: '/vacancies' }
];
```

---

## 8. Client-Side Validation

All forms must implement client-side validation mirroring the backend DTOs.

### 8.1 Form Validation Rules

| Field | Rule |
|-------|------|
| `email` | Required, valid email format |
| `password` | Required, min 8 chars, must include non-alphanumeric, digit, lowercase, uppercase |
| `firstName` / `lastName` (employee) | Required, non-empty string |
| `names` / `lastNames` (candidate) | Required, non-empty string |
| `cedula` | Required, unique (handled server-side) |
| `phoneNumber` | Optional or required depending on form |
| `salary` (position) | Required, decimal >= 1 |
| `evaluationDate` | Required, valid Date |
| `score` (evaluation criterion) | Required, float in [0.0, 5.0] |
| `cvFile` | Required, must be PDF, max 10 MB |

### 8.2 Reactive Forms Pattern

Use Angular Reactive Forms with `FormBuilder`:

```typescript
@Component({ ... })
export class LoginComponent {
  form = inject(FormBuilder).group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  onSubmit() {
    if (this.form.valid) {
      this.authService.login(this.form.value.email!, this.form.value.password!)
        .subscribe({ next: () => this.router.navigate(['/']), error: (err) => this.toast.error(err.detail) });
    }
  }
}
```

---

## 9. State Management

Angular Signals are used for reactive state. Each feature module has its own signal-based store service.

```typescript
// Example: VacancyStore
@Injectable({ providedIn: 'root' })
export class VacancyStore {
  private api = inject(ApiService);

  vacancies = signal<VacantDto[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  loadVacancies() {
    this.loading.set(true);
    this.api.getVacancies().subscribe({
      next: (data) => { this.vacancies.set(data); this.loading.set(false); },
      error: (err) => { this.error.set(err.detail); this.loading.set(false); }
    });
  }
}
```

---

## 10. Conventions

### 10.1 File Naming

- Components: `kebab-case.component.ts` (e.g., `vacancy-list.component.ts`)
- Services: `kebab-case.service.ts` (e.g., `auth.service.ts`)
- Models: `kebab-case.models.ts` (e.g., `vacancy.models.ts`)
- Guards: `kebab-case.guard.ts` (e.g., `admin.guard.ts`)

### 10.2 API URL Naming

All API endpoints use the naming conventions from `api-reference.md`. URLs are lowercase and hyphenated. DTO property names use camelCase in JSON.

### 10.3 Response Handling

- Always use typed observables (generics)
- Show success toast on create/update/delete
- Show error toast on failure with the `detail` message from the API error response
- Handle `204 No Content` responses with `observe: 'response'` or by returning `Observable<void>`

---

## 11. Revision History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-04-24 | Initial version |
