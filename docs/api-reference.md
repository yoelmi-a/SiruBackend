# API Reference — SIRUS (Smart Intelligent Resource Optimization System)

**Version:** 1.0
**Base URL:** `http://localhost:5000/api`
**Auth/Accounts Base URL:** `http://localhost:5000/api/v1`
**API Versioning:** Only auth and accounts endpoints are versioned (`/api/v1/`). All other controllers use the unversioned `/api/` path.
**Date:** 2025

---

> **Note:** The Auth (`/api/v1/auth`) and Accounts (`/api/v1/accounts`) controllers were added beyond the original SRS scope (which marked authentication as out of scope). All other endpoints document the originally specified functionality. Auth endpoints are marked with **[Beyond SRS]** throughout this document.

---

## 1. Introduction

This document is the authoritative API reference for the SIRUS REST API. It lists every endpoint, its request format, response format, authentication requirements, and possible error outcomes.

### 1.1 How to Read Endpoint Tables

| Column | Description |
|--------|-------------|
| **Method** | HTTP verb: `GET`, `POST`, `PUT`, `PATCH`, `DELETE` |
| **Path** | URL path relative to the base URL. Path parameters are written as `{param}` |
| **Auth** | `Public` — no authentication required. `Auth` — valid JWT required. `Admin` — valid JWT with Admin role required |
| **Request Body** | DTO class name used for the request; `None` if no body. File uploads note the `Content-Type` |
| **Query Params** | Pagination and filter parameters available on `GET` endpoints |
| **Success** | HTTP status code and the returned DTO type |
| **Errors** | All possible error status codes and their meaning |

### 1.2 Request Conventions

- All request bodies use `Content-Type: application/json` unless otherwise noted.
- File upload endpoints use `Content-Type: multipart/form-data`.
- Dates follow **ISO 8601** format: `YYYY-MM-DDTHH:mm:ssZ`.
- IDs are **strings** (ULID format for business entities, UUID format for auth entities).

### 1.3 Response Conventions

- All responses use `Content-Type: application/json` unless otherwise noted.
- Successful responses always return JSON data. Empty responses (`204 No Content`) return no body.
- All error responses use the **ProblemDetails** envelope described in Section 2.1.

---

## 2. Standard Conventions

### 2.1 Error Response Envelope

Every error response follows the **ProblemDetails** format (RFC 9457):

```json
{
  "statusCode": 404,
  "title": "Not Found",
  "detail": "Vacancy not found",
  "instance": "/api/vacancies/01HXXXXXXXXXXXX"
}
```

| Field | Type | Description |
|-------|------|-------------|
| `statusCode` | `int` | HTTP status code |
| `title` | `string` | Short, human-readable error title |
| `detail` | `string` | Detailed error message (may contain multiple comma-separated errors) |
| `instance` | `string` | The request path that generated the error |

### 2.2 HTTP Status Codes

| Code | Meaning |
|------|---------|
| `200 OK` | Request succeeded; response body contains data |
| `201 Created` | Resource created successfully; `Location` header points to new resource |
| `202 Accepted` | Request accepted for processing (async operation) |
| `204 No Content` | Request succeeded; no response body |
| `400 Bad Request` | Validation failure or bad request parameters |
| `401 Unauthorized` | Missing or invalid JWT token |
| `403 Forbidden` | Authenticated but lacks required role |
| `404 Not Found` | Resource not found |
| `409 Conflict` | Business rule violation (e.g., duplicate, integrity constraint) |
| `422 Unprocessable Entity` | Valid syntax but semantic errors |
| `423 Locked` | Account is locked |
| `500 Internal Server Error` | Unexpected server error |

### 2.3 Pagination

All list endpoints that return paginated data accept the following query parameters:

| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `page` | `int` | `1` | Page number (1-indexed) |
| `pageSize` | `int` | `10` | Number of items per page |
| `isActive` | `bool?` | `null` | Optional filter for active/inactive entities (employees only) |

Paginated responses return a **`PaginatedResponse<T>`** envelope:

```json
{
  "items": [ ... ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 42
  }
}
```

### 2.4 File Upload

File upload endpoints:
- Use `Content-Type: multipart/form-data`
- Maximum file size: **10 MB** (enforced by `[RequestSizeLimit]` attribute)
- Only **PDF** files are accepted for CV uploads
- The file field name is `cvFile`

```
POST /api/vacancies/{vacancyId}/applications
Content-Type: multipart/form-data

--boundary
Content-Disposition: form-data; name="candidateNames"
John

--boundary
Content-Disposition: form-data; name="candidateLastNames"
Doe

--boundary
Content-Disposition: form-data; name="candidateEmail"
john.doe@example.com

--boundary
Content-Disposition: form-data; name="candidatePhoneNumber"
12345678

--boundary
Content-Disposition: form-data; name="cvFile"; filename="john_doe_cv.pdf"
Content-Type: application/pdf
<binary PDF content>
```

### 2.5 Authentication (JWT)

Authenticated endpoints require a **Bearer JWT token** in the `Authorization` header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

The JWT payload contains:

| Claim | Description |
|-------|-------------|
| `sub` | Account ID |
| `uid` | Account ID (duplicate, used by the system) |
| `email` | Account email |
| `fullName` | Full name of the account |
| `roles` | Array of role strings (e.g., `["Admin"]`) |
| `exp` | Expiration timestamp |

Token validity: configurable via `TokenSettings.JwtDurationInMinutes` (default 60 minutes).

---

## 3. API Endpoints

---

### 3.1 Vacancies

**Base URL:** `/api/vacancies`

| Method | Path | Auth | Request Body | Query Params | Success | Errors |
|--------|------|------|--------------|--------------|---------|--------|
| `GET` | `/` | Public | None | — | `200` — `IEnumerable<VacantDto>` | — |
| `GET` | `/{id}` | Public | None | — | `200` — `VacantDto` | `404` |
| `POST` | `/` | Public | `SaveVacantDto` | — | `201` — `VacantDto` | `400` |
| `PUT` | `/{id}` | Public | `UpdateVacantDto` | — | `204` | `400`, `404` |
| `DELETE` | `/{id}` | Public | None | — | `204` | `404` |
| `POST` | `/{vacancyId}/applications` | Public | `VacancyApplicationDto` (multipart) | — | `201` — `VacancyApplicationResultDto` | `400`, `404`, `409` |
| `POST` | `/{vacancyId}/recalculate-scores` | Public | None | — | `202` | `404` |
| `GET` | `/{vacancyId}/applications` | Public | None | `page`, `pageSize` | `200` — `PaginatedResponse<VacancyApplicationResultDto>` | `404` |

---

#### `GET /api/vacancies`

Returns all vacancies (no filter).

**Response `200`:**
```json
[
  {
    "id": "01HXXXXXXXXXXXX",
    "title": "Software Engineer",
    "description": "Build APIs",
    "profile": "3+ years experience",
    "publicationDate": "2025-01-15T00:00:00Z",
    "hiringDate": null,
    "status": "Open"
  }
]
```

---

#### `GET /api/vacancies/{id}`

Returns a single vacancy by ID.

**Path parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `id` | `string` | ULID string identifier |

**Response `200`:**
```json
{
  "id": "01HXXXXXXXXXXXX",
  "title": "Software Engineer",
  "description": "Build APIs",
  "profile": "3+ years experience",
  "publicationDate": "2025-01-15T00:00:00Z",
  "hiringDate": null,
  "status": "Open"
}
```

**Response `404`:**
```json
{
  "statusCode": 404,
  "title": "Not Found",
  "detail": "Vacancy not found",
  "instance": "/api/vacancies/01HXXXXXXXXXXXX"
}
```

---

#### `POST /api/vacancies`

Creates a new vacancy. The vacancy is created with status `Open`.

**Request body — `SaveVacantDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `title` | `string` | Yes | Vacancy title |
| `description` | `string` | Yes | Full job description |
| `profile` | `string` | Yes | Ideal candidate profile |
| `positionId` | `int` | Yes | Associated position ID |

**Response `201`:**
```json
{
  "id": "01HXXXXXXXXXXXX",
  "title": "Software Engineer",
  "description": "Build APIs",
  "profile": "3+ years experience",
  "publicationDate": "2025-01-15T00:00:00Z",
  "hiringDate": null,
  "status": "Open"
}
```

**Response `400`:** Validation failure (missing or invalid fields).

---

#### `PUT /api/vacancies/{id}`

Updates an existing vacancy.

**Request body — `UpdateVacantDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `title` | `string` | Yes | Vacancy title |
| `description` | `string` | Yes | Full job description |
| `profile` | `string` | Yes | Ideal candidate profile |
| `status` | `string` | Yes | `Open`, `Closed`, or `Cancelled` |

**Response `204`:** Updated successfully (no body).

**Response `400`:** Validation failure.
**Response `404`:** Vacancy not found.

---

#### `DELETE /api/vacancies/{id}`

Deletes a vacancy.

**Path parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `id` | `string` | ULID of the vacancy |

**Response `204`:** Deleted successfully.
**Response `404`:** Vacancy not found.

---

#### `POST /api/vacancies/{vacancyId}/applications`

Uploads a CV and applies a candidate to a vacancy. File upload endpoint (10 MB limit).

**Path parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `vacancyId` | `string` | ULID of the vacancy |

**Request body — `VacancyApplicationDto`** (multipart/form-data):
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `candidateNames` | `string` | Yes | Candidate first name |
| `candidateLastNames` | `string` | Yes | Candidate last name |
| `candidateEmail` | `string` | Yes | Candidate email address |
| `candidatePhoneNumber` | `string` | Yes | Candidate phone number |
| `cvFile` | `file` | Yes | PDF CV file (max 10 MB) |

**Response `201`:**
```json
{
  "applicationId": "01HXXXXXXXXXXXX",
  "vacantId": "01HXXXXXXXXXXXX",
  "candidateId": "01HXXXXXXXXXXXX",
  "candidateFullName": "John Doe",
  "cvUrl": "/api/files/cv/01HXXXXXXXXXXXX.pdf",
  "status": "Pending",
  "score": 0.0
}
```

**Response `400`:** Validation failure (missing fields, file not PDF, file too large).
**Response `404`:** Vacancy not found.
**Response `409`:** Vacancy is not in `Open` state, or candidate already applied to this vacancy.

---

#### `POST /api/vacancies/{vacancyId}/recalculate-scores`

Manually triggers NLP score recalculation for all candidates of a vacancy. Runs asynchronously.

**Response `202`:** Request accepted for background processing (no body).
**Response `404`:** Vacancy not found.

---

#### `GET /api/vacancies/{vacancyId}/applications`

Returns a paginated list of all applications (candidates) for a vacancy, sorted by score descending.

**Query parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `page` | `int` | `1` | Page number |
| `pageSize` | `int` | `10` | Items per page |

**Response `200`:**
```json
{
  "items": [
    {
      "applicationId": "01HXXXXXXXXXXXX",
      "vacantId": "01HXXXXXXXXXXXX",
      "candidateId": "01HXXXXXXXXXXXX",
      "candidateFullName": "John Doe",
      "cvUrl": "/api/files/cv/01HXXXXXXXXXXXX.pdf",
      "status": "Pending",
      "score": 0.85
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 3
  }
}
```

**Response `404`:** Vacancy not found.

---

### 3.2 Candidates

**Base URL:** `/api/candidates`

| Method | Path | Auth | Request Body | Query Params | Success | Errors |
|--------|------|------|--------------|--------------|---------|--------|
| `GET` | `/` | Public | None | — | `200` — `IEnumerable<CandidateDto>` | — |
| `GET` | `/{id}` | Public | None | — | `200` — `CandidateDto` | `404` |
| `POST` | `/` | Public | `CandidateInsertDto` | — | `201` — `CandidateDto` | `400` |
| `PUT` | `/{id}` | Public | `CandidateUpdateDto` | — | `204` | `400`, `404` |
| `DELETE` | `/{id}` | Public | None | — | `204` | `404` |

---

#### `GET /api/candidates`

Returns all registered candidates.

**Response `200`:**
```json
[
  {
    "id": "01HXXXXXXXXXXXX",
    "names": "John",
    "lastNames": "Doe",
    "email": "john.doe@example.com",
    "phoneNumber": "12345678",
    "cvUrl": "/api/files/cv/01HXXXXXXXXXXXX.pdf"
  }
]
```

---

#### `GET /api/candidates/{id}`

**Response `200`:** `CandidateDto`
**Response `404`:** Candidate not found.

---

#### `POST /api/candidates`

**Request body — `CandidateInsertDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `names` | `string` | Yes | Candidate first name |
| `lastNames` | `string` | Yes | Candidate last name |
| `email` | `string` | Yes | Email (must be unique) |
| `phoneNumber` | `string` | Yes | Phone number |

**Response `201`:** `CandidateDto`
**Response `400`:** Validation failure or duplicate email (`409` internally).

---

#### `PUT /api/candidates/{id}`

**Request body — `CandidateUpdateDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `string` | Yes | Candidate ID |
| `names` | `string` | Yes | First name |
| `lastNames` | `string` | Yes | Last name |
| `email` | `string` | No | Email |
| `phoneNumber` | `string` | Yes | Phone number |

**Response `204`:** Updated successfully.
**Response `400`:** Validation failure.
**Response `404`:** Candidate not found.

---

#### `DELETE /api/candidates/{id}`

**Response `204`:** Deleted successfully.
**Response `404`:** Candidate not found.

---

### 3.3 Employees

**Base URL:** `/api/employees`

| Method | Path | Auth | Request Body | Query Params | Success | Errors |
|--------|------|------|--------------|--------------|---------|--------|
| `GET` | `/` | Public | None | `page`, `pageSize`, `isActive` | `200` — `PaginatedResponse<EmployeeListDto>` | — |
| `GET` | `/{id}` | Public | None | — | `200` — `EmployeeDto` | `404` |
| `POST` | `/` | Public | `EmployeeInsertDto` | — | `201` — `EmployeeDto` | `400`, `409` |
| `PUT` | `/{id}` | Public | `EmployeeUpdateDto` | — | `200` — `EmployeeDto` | `400`, `404`, `409` |
| `DELETE` | `/{id}` | Public | None | — | `204` | `404` |
| `GET` | `/{id}/history` | Public | None | — | `200` — `IEnumerable<EmployeeHistoryDto>` | `404` |
| `GET` | `/{id}/evaluations` | Public | None | — | `200` — `IEnumerable<EvaluationHistoryDto>` | `404` |
| `POST` | `/{id}/positions` | Public | `EmployeePositionInsertDto` | — | `201` — `EmployeePositionDto` | `400`, `404`, `409` |

---

#### `GET /api/employees`

**Query parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `page` | `int` | `1` | Page number |
| `pageSize` | `int` | `10` | Items per page |
| `isActive` | `bool?` | `null` | Filter by active status |

**Response `200`:**
```json
{
  "items": [
    {
      "id": "01HXXXXXXXXXXXX",
      "fullName": "Jane Smith",
      "cedula": "E-12345678",
      "phoneNumber": "12345678",
      "isActive": true
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 5
  }
}
```

---

#### `GET /api/employees/{id}`

Returns full employee detail.

**Response `200`:** `EmployeeDto`
**Response `404`:** Employee not found.

---

#### `POST /api/employees`

**Request body — `EmployeeInsertDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `firstName` | `string` | Yes | First name |
| `lastName` | `string` | Yes | Last name |
| `address` | `string` | Yes | Home address |
| `cedula` | `string` | Yes | National ID (unique) |
| `phoneNumber` | `string` | Yes | Phone number |
| `dateOfBirth` | `DateTime` | Yes | Date of birth (YYYY-MM-DD) |
| `email` | `string` | Yes | Email address |

**Response `201`:** `EmployeeDto`
**Response `400`:** Validation failure.
**Response `409`:** Duplicate `cedula`.

---

#### `PUT /api/employees/{id}`

**Request body — `EmployeeUpdateDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `firstName` | `string` | Yes | First name |
| `lastName` | `string` | Yes | Last name |
| `address` | `string` | Yes | Home address |
| `cedula` | `string` | Yes | National ID (unique) |
| `phoneNumber` | `string` | Yes | Phone number |
| `dateOfBirth` | `DateTime` | Yes | Date of birth |
| `email` | `string` | Yes | Email address |

**Response `200`:** Updated `EmployeeDto`.
**Response `400`:** Validation failure.
**Response `404`:** Employee not found.
**Response `409`:** Duplicate `cedula`.

---

#### `DELETE /api/employees/{id}`

Soft-deletes an employee (sets `IsActive = false`). Use `change-status` on accounts for hard deactivation.

**Response `204`:** Deactivated successfully.
**Response `404`:** Employee not found.

---

#### `GET /api/employees/{id}/history`

Returns the complete work history (position assignments) for an employee.

**Response `200`:**
```json
[
  {
    "positionName": "Software Engineer",
    "departmentName": "Engineering",
    "startDate": "2022-03-01T00:00:00Z",
    "endDate": null
  }
]
```

**Response `404`:** Employee not found.

---

#### `GET /api/employees/{id}/evaluations`

Returns all performance evaluations for an employee in chronological order.

**Response `200`:**
```json
[
  {
    "id": "01HXXXXXXXXXXXX",
    "date": "2024-06-01T00:00:00Z",
    "averageScore": 4.2,
    "positionName": "Software Engineer",
    "criteria": [
      {
        "name": "Teamwork",
        "score": 4.0,
        "observation": "Good collaboration"
      },
      {
        "name": "Responsibility",
        "score": 4.5,
        "observation": null
      }
    ]
  }
]
```

**Response `404`:** Employee not found.

---

#### `POST /api/employees/{id}/positions`

Assigns a position to an employee.

**Request body — `EmployeePositionInsertDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `employeeId` | `string` | Yes | Employee ID |
| `positionId` | `int` | Yes | Position ID |
| `startDate` | `DateTime` | No | Start date (defaults to `DateTime.UtcNow`) |

**Response `201`:** `EmployeePositionDto`
**Response `400`:** Validation failure.
**Response `404`:** Employee or position not found.
**Response `409`:** Employee already has an active assignment for this position.

---

### 3.4 Positions

**Base URL:** `/api/positions`

| Method | Path | Auth | Request Body | Query Params | Success | Errors |
|--------|------|------|--------------|--------------|---------|--------|
| `GET` | `/` | Public | None | — | `200` — `IEnumerable<PositionDto>` | — |
| `GET` | `/{id}` | Public | None | — | `200` — `PositionDto` | `404` |
| `POST` | `/` | Public | `PositionInsertDto` | — | `201` — `PositionDto` | `400`, `404`, `409` |
| `PUT` | `/{id}` | Public | `PositionUpdateDto` | — | `204` | `400`, `404`, `409` |
| `DELETE` | `/{id}` | Public | None | — | `204` | `404` |

---

#### `POST /api/positions`

**Request body — `PositionInsertDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | `string` | Yes | Position name |
| `salary` | `decimal` | Yes | Salary (must be >= 1) |
| `departmentId` | `int` | Yes | Department ID |

**Response `201`:** `PositionDto`
**Response `400`:** Validation failure or salary < 1.
**Response `404`:** Department not found.
**Response `409`:** A position with the same name already exists in this department.

---

#### `PUT /api/positions/{id}`

**Request body — `PositionUpdateDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | `string` | Yes | Position name |
| `salary` | `decimal` | Yes | Salary (must be >= 0) |
| `departmentId` | `int` | Yes | Department ID |

**Response `204`:** Updated successfully.
**Response `400`:** Validation failure.
**Response `404`:** Position or department not found.
**Response `409`:** Name conflict.

---

#### `DELETE /api/positions/{id}`

**Response `204`:** Deleted successfully.
**Response `404`:** Position not found.

---

### 3.5 Departments

**Base URL:** `/api/departments`

| Method | Path | Auth | Request Body | Query Params | Success | Errors |
|--------|------|------|--------------|--------------|---------|--------|
| `GET` | `/` | Public | None | — | `200` — `IEnumerable<DepartmentDto>` | — |
| `GET` | `/{id}` | Public | None | — | `200` — `DepartmentDto` | `404` |
| `POST` | `/` | Public | `DepartmentInsertDto` | — | `201` — `DepartmentDto` | `400`, `409` |
| `PUT` | `/{id}` | Public | `DepartmentUpdateDto` | — | `204` | `400`, `404`, `409` |
| `DELETE` | `/{id}` | Public | None | — | `204` | `404` |

---

#### `POST /api/departments`

**Request body — `DepartmentInsertDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | `string` | Yes | Department name (unique) |

**Response `201`:** `DepartmentDto`
**Response `400`:** Validation failure.
**Response `409`:** Department name already exists.

---

#### `PUT /api/departments/{id}`

Updates a department.

**Path parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `id` | `int` | Department ID |

**Request body — `DepartmentUpdateDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | `string` | Yes | Department name (unique) |

**Response `204`:** Updated successfully.
**Response `400`:** Validation failure.
**Response `404`:** Department not found.
**Response `409`:** Name conflict.

---

#### `DELETE /api/departments/{id}`

**Response `204`:** Deleted successfully.
**Response `404`:** Department not found.

---

### 3.6 Evaluations

**Base URL:** `/api/evaluations`

| Method | Path | Auth | Request Body | Query Params | Success | Errors |
|--------|------|------|--------------|--------------|---------|--------|
| `POST` | `/` | Public | `EvaluationInsertDto` | — | `201` — `EvaluationDto` | `400`, `404` |

---

#### `POST /api/evaluations`

Creates a performance evaluation for an employee.

**Request body — `EvaluationInsertDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `employeeId` | `string` | Yes | Employee ID |
| `evaluationDate` | `DateTime` | Yes | Evaluation date |
| `criteria` | `List<EvaluationCriterionInsertDto>` | Yes | At least one criterion entry |

**`EvaluationCriterionInsertDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `criterionId` | `int` | Yes | Criterion ID |
| `score` | `float` | Yes | Score in range [0.0, 5.0] |
| `observation` | `string?` | No | Optional observation |

**Response `201`:**
```json
{
  "id": "01HXXXXXXXXXXXX",
  "employeeId": "01HXXXXXXXXXXXX",
  "employeeFullName": "Jane Smith",
  "positionName": "Software Engineer",
  "date": "2024-06-01T00:00:00Z",
  "averageScore": 4.2,
  "criteria": [
    {
      "criterionId": 1,
      "criterionName": "Teamwork",
      "score": 4.0,
      "observation": "Good collaboration"
    }
  ]
}
```

**Response `400`:** Empty criteria list, or a score outside [0.0, 5.0].
**Response `404`:** Employee not found.

---

### 3.7 Evaluation Criteria

**Base URL:** `/api/evaluation-criteria`

| Method | Path | Auth | Request Body | Query Params | Success | Errors |
|--------|------|------|--------------|--------------|---------|--------|
| `GET` | `/` | Public | None | — | `200` — `IEnumerable<CriterionDto>` | — |
| `GET` | `/{id}` | Public | None | — | `200` — `CriterionDto` | `404` |
| `POST` | `/` | Public | `CriterionInsertDto` | — | `201` — `CriterionDto` | `400`, `409` |
| `PUT` | `/{id}` | Public | `CriterionUpdateDto` | — | `200` — `CriterionDto` | `400`, `404`, `409` |
| `DELETE` | `/{id}` | Public | None | — | `204` | `404`, `409` |

---

#### `POST /api/evaluation-criteria`

**Request body — `CriterionInsertDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | `string` | Yes | Criterion name (unique) |

**Response `201`:** `CriterionDto`
**Response `400`:** Validation failure.
**Response `409`:** Criterion with same name already exists.

---

#### `PUT /api/evaluation-criteria/{id}`

**Request body — `CriterionUpdateDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | `string` | Yes | Criterion name (unique) |

**Response `200`:** Updated `CriterionDto`
**Response `400`:** Validation failure.
**Response `404`:** Criterion not found.
**Response `409`:** Name conflict.

---

#### `DELETE /api/evaluation-criteria/{id}`

**Response `204`:** Deleted successfully.
**Response `404`:** Criterion not found.
**Response `409`:** Criterion is referenced in at least one evaluation.

---

### 3.8 Reports

**Base URL:** `/api/reports`

| Method | Path | Auth | Request Body | Query Params | Success | Errors |
|--------|------|------|--------------|--------------|---------|--------|
| `GET` | `/hiring-time` | Public | None | — | `200` — `HiringTimeReportDto` | — |
| `GET` | `/performance-by-department` | Public | None | — | `200` — `IEnumerable<DepartmentPerformanceDto>` | `500` |
| `GET` | `/employees` | Public | None | `page`, `pageSize`, `isActive` | `200` — `PaginatedResponse<EmployeeReportDto>` | — |
| `GET` | `/hiring-time/export` | Public | None | — | `200` — PDF file (`application/pdf`) | `500` |
| `GET` | `/performance-by-department/export` | Public | None | — | `200` — PDF file (`application/pdf`) | `500` |
| `GET` | `/employees/export` | Public | None | — | `200` — PDF file (`application/pdf`) | `500` |

---

#### `GET /api/reports/hiring-time`

**Response `200`:**
```json
{
  "averageDays": 18.5,
  "totalClosedVacancies": 12
}
```

---

#### `GET /api/reports/performance-by-department`

**Response `200`:**
```json
[
  {
    "departmentName": "Engineering",
    "averageScore": 4.2,
    "employeeCount": 8
  }
]
```

**Response `500`:**
```json
{
  "message": "An error occurred while retrieving performance data."
}
```

---

#### `GET /api/reports/employees`

**Query parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `page` | `int` | `1` | Page number |
| `pageSize` | `int` | `10` | Items per page |
| `isActive` | `bool?` | `null` | Filter by active status |

**Response `200`:**
```json
{
  "items": [
    {
      "id": "01HXXXXXXXXXXXX",
      "fullName": "Jane Smith",
      "cedula": "E-12345678",
      "position": "Software Engineer",
      "department": "Engineering",
      "isActive": true
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 5
  }
}
```

---

#### `GET /api/reports/hiring-time/export`

Returns a **PDF file** (`Content-Type: application/pdf`).

**Response `200`:**
```
Content-Type: application/pdf
Content-Disposition: attachment; filename="hiring-time-report.pdf"
<binary PDF content>
```

**Response `500`:**
```json
{
  "message": "PDF generation failed. Please try again later."
}
```

---

#### `GET /api/reports/performance-by-department/export`

Returns a **PDF file** (`Content-Type: application/pdf`).

**Response `200`:**
```
Content-Type: application/pdf
Content-Disposition: attachment; filename="performance-by-department-report.pdf"
<binary PDF content>
```

**Response `500`:**
```json
{
  "message": "PDF generation failed. Please try again later."
}
```

---

#### `GET /api/reports/employees/export`

Returns a **PDF file** (`Content-Type: application/pdf`).

**Response `200`:**
```
Content-Type: application/pdf
Content-Disposition: attachment; filename="employees-report.pdf"
<binary PDF content>
```

**Response `500`:**
```json
{
  "message": "PDF generation failed. Please try again later."
}
```

---

### 3.9 Auth **[Beyond SRS]**

**Base URL:** `/api/v1/auth`

> Authentication and session management were marked **out of scope** in the original SRS. This controller was added beyond the original specification.

| Method | Path | Auth | Request Body | Query Params | Success | Errors |
|--------|------|------|--------------|--------------|---------|--------|
| `POST` | `/login` | Public | `AuthDto` | — | `200` — JWT string | `400`, `401`, `423` |
| `POST` | `/refresh` | Public* | `RefreshRequest` | Cookie: `refresh-token` | `200` — JWT string | `400` |
| `POST` | `/forgot-password` | Public | `ForgotPasswordDto` | — | `204` | `400` |
| `POST` | `/reset-password` | Public | `ResetPasswordDto` | — | `204` | `400` |
| `POST` | `/revoke` | Admin | `RevokeAccessRequestDto` | — | `204` | — |
| `POST` | `/me/revoke` | Auth | `RevokeSessionRequest` | — | `204` | `401` |
| `GET` | `/account-{accountId}/sessions` | Admin | None | `page`, `pageSize` | `200` — `PaginatedResponse<UserSessionDto>` | `404` |
| `GET` | `/me/sessions` | Auth | None | `page`, `pageSize` | `200` — `PaginatedResponse<UserSessionDto>` | `401`, `404` |
| `POST` | `/logout` | Auth | None | Cookie: `refresh-token` | `200` | `400` |

\*`/refresh` does not require a pre-existing JWT, but requires the `refresh-token` cookie from a prior login.

---

#### `POST /api/v1/auth/login`

Authenticates an account and returns a JWT access token. A refresh token is also issued and stored as an HTTP-only cookie.

**Request body — `AuthDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `email` | `string` | Yes | Account email |
| `password` | `string` | Yes | Account password |

**Response `200`:**
```
The response body is a raw JWT string (not JSON):
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

A `refresh-token` cookie is also set with the response:
```
Set-Cookie: refresh-token=<refresh-token-value>; HttpOnly; Path=/api/v1/Auth
```

**Error `400`:** Validation failure.
**Error `401`:** Invalid credentials or inactive account.
**Error `423`:** Account is locked (too many failed attempts).

---

#### `POST /api/v1/auth/refresh`

Refreshes an expired (or expiring) JWT using the refresh token from the cookie.

**Request body — `RefreshRequest`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `jwtToken` | `string` | Yes | The expired JWT to refresh |

**Also requires:** `refresh-token` HTTP-only cookie from a prior login.

**Response `200`:**
```
The response body is a raw JWT string (not JSON):
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response `400`:** Missing `jwtToken` body field or `refresh-token` cookie.

---

#### `POST /api/v1/auth/forgot-password`

Sends a password reset email to the account's email address. All active sessions for that account are revoked.

**Request body — `ForgotPasswordDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `email` | `string` | Yes | Account email |
| `origin` | `string` | Yes | Base URL for constructing the reset link |

**Response `204`:** Email sent successfully (no body).
**Response `400`:** Validation failure or account not found / inactive.

---

#### `POST /api/v1/auth/reset-password`

Resets the account password using a token sent via the forgot-password email.

**Request body — `ResetPasswordDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `password` | `string` | Yes | New password |
| `accountId` | `string` | Yes | Account ID (from reset email link) |
| `token` | `string` | Yes | Base64-encoded reset token (from reset email link) |

**Response `204`:** Password reset successfully (no body).
**Response `400`:** Invalid token, invalid account ID, or password does not meet complexity requirements.

---

#### `POST /api/v1/auth/revoke` (Admin)

Revokes any user's active session by session ID. Admin-only.

**Request body — `RevokeAccessRequestDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `sessionId` | `string` | Yes | ID of the session to revoke |
| `actionMadeByAccountId` | `string` | Yes | Admin account ID performing the action |

**Response `204`:** Session revoked successfully (no body).

---

#### `POST /api/v1/auth/me/revoke` (Auth)

Revokes the authenticated user's own session by session ID.

**Request body — `RevokeSessionRequest`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `sessionId` | `string` | Yes | ID of the session to revoke |

**Response `204`:** Session revoked successfully (no body).
**Response `401`:** Token missing `uid` claim (invalid token).

---

#### `GET /api/v1/auth/account-{accountId}/sessions` (Admin)

Returns all active sessions for a specific account. Admin-only.

**Query parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `page` | `int` | `1` | Page number |
| `pageSize` | `int` | `10` | Items per page |

**Response `200`:**
```json
{
  "items": [
    {
      "id": "01HXXXXXXXXXXXX",
      "deviceInfo": "Chrome/WinNT"
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 2
  }
}
```

**Response `404`:** No active sessions found for this account.

---

#### `GET /api/v1/auth/me/sessions` (Auth)

Returns all active sessions for the authenticated user.

**Query parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `page` | `int` | `1` | Page number |
| `pageSize` | `int` | `10` | Items per page |

**Response `200`:**
```json
{
  "items": [
    {
      "id": "01HXXXXXXXXXXXX",
      "deviceInfo": "Chrome/WinNT"
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1
  }
}
```

**Response `401`:** Token missing `uid` claim.
**Response `404`:** No active sessions found.

---

#### `POST /api/v1/auth/logout` (Auth)

Logs out the authenticated user by revoking the refresh token session. Requires the `refresh-token` cookie.

**Also requires:** `refresh-token` HTTP-only cookie.

**Response `200`:** Logged out successfully (no body).
**Response `400`:** Missing `refresh-token` cookie.

---

### 3.10 Accounts **[Beyond SRS]**

**Base URL:** `/api/v1/accounts`

> User and account management were marked **out of scope** in the original SRS. This controller was added beyond the original specification.

| Method | Path | Auth | Request Body | Query Params | Success | Errors |
|--------|------|------|--------------|--------------|---------|--------|
| `POST` | `/register` | Admin | `SaveAccountDto` | — | `201` | `400`, `409` |
| `PATCH` | `/edit` | Auth | `EditAccountDto` | — | `204` | `400`, `404` |
| `PATCH` | `/change-status` | Admin | `ChangeStatusDto` | — | `204` | `400`, `404` |
| `GET` | `/all` | Admin | None | `page`, `pageSize` | `200` — `PaginatedResponse<GetAccountDto>` | — |
| `GET` | `/{accountId}` | Admin | None | — | `200` — `GetAccountDto` | `404` |
| `GET` | `/me` | Auth | None | — | `200` — `GetAccountDto` | `401`, `404` |

---

#### `POST /api/v1/accounts/register` (Admin)

Registers a new user account.

**Request body — `SaveAccountDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | `string` | Yes | First name |
| `lastName` | `string` | Yes | Last name |
| `email` | `string` | Yes | Email (unique) |
| `password` | `string` | Yes | Password (min 8 chars, requires non-alphanumeric, digit, lower, upper) |
| `idCard` | `string` | Yes | National ID |
| `role` | `string` | Yes | `Admin` or `Supervisor` |
| `phoneNumber` | `string` | No | Phone number |

**Response `201`:** Created (no body).
**Response `400`:** Validation failure or invalid role.
**Response `409`:** Email already registered.

---

#### `PATCH /api/v1/accounts/edit` (Auth)

Updates the authenticated user's own account data.

**Request body — `EditAccountDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `string` | Yes | Account ID |
| `name` | `string` | Yes | First name |
| `lastName` | `string` | Yes | Last name |
| `email` | `string` | Yes | Email |
| `idCard` | `string` | Yes | National ID |
| `role` | `string` | Yes | `Admin` or `Supervisor` |
| `phoneNumber` | `string` | No | Phone number |

**Response `204`:** Updated successfully (no body).
**Response `400`:** Validation failure or email already in use by another account.
**Response `404`:** Account not found.

---

#### `PATCH /api/v1/accounts/change-status` (Admin)

Toggles an account's active/inactive status. Admin-only.

**Request body — `ChangeStatusDto`:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `accountId` | `string` | Yes | ID of the account to toggle |

**Response `204`:** Status changed successfully (no body).
**Response `400`:** Validation failure.
**Response `404`:** Account not found.

---

#### `GET /api/v1/accounts/all` (Admin)

Returns a paginated list of all accounts.

**Query parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `page` | `int` | `1` | Page number |
| `pageSize` | `int` | `10` | Items per page |

**Response `200`:**
```json
{
  "items": [
    {
      "id": "01HXXXXXXXXXXXX",
      "name": "Jane",
      "lastName": "Smith",
      "email": "jane.smith@example.com",
      "idCard": "E-12345678",
      "isVerified": true,
      "role": "Admin",
      "phoneNumber": "12345678"
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 3
  }
}
```

---

#### `GET /api/v1/accounts/{accountId}` (Admin)

**Response `200`:** `GetAccountDto`
**Response `404`:** Account not found.

---

#### `GET /api/v1/accounts/me` (Auth)

Returns the authenticated user's own account data.

**Response `200`:** `GetAccountDto`
**Response `401`:** Token missing `uid` claim.
**Response `404`:** Account not found.

---

## 4. Appendix: DTO Reference

This section lists all DTOs that appear as request or response bodies in the API endpoints.

### 4.1 Request DTOs

---

#### `SaveVacantDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Title` | `string` | Yes | Vacancy title |
| `Description` | `string` | Yes | Full job description |
| `Profile` | `string` | Yes | Ideal candidate profile |
| `PositionId` | `int` | Yes | Associated position ID |

---

#### `UpdateVacantDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Title` | `string` | Yes | Vacancy title |
| `Description` | `string` | Yes | Full job description |
| `Profile` | `string` | Yes | Ideal candidate profile |
| `Status` | `string` | Yes | `Open`, `Closed`, or `Cancelled` |

---

#### `VacancyApplicationDto` (multipart/form-data)

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `CandidateNames` | `string` | Yes | Candidate first name |
| `CandidateLastNames` | `string` | Yes | Candidate last name |
| `CandidateEmail` | `string` | Yes | Candidate email |
| `CandidatePhoneNumber` | `string` | Yes | Candidate phone |
| `CvFile` | `file` | Yes | PDF CV (max 10 MB) |

---

#### `CandidateInsertDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Names` | `string` | Yes | First name |
| `LastNames` | `string` | Yes | Last name |
| `Email` | `string` | Yes | Email (unique) |
| `PhoneNumber` | `string` | Yes | Phone number |

---

#### `CandidateUpdateDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `string` | Yes | Candidate ID |
| `Names` | `string` | Yes | First name |
| `LastNames` | `string` | Yes | Last name |
| `Email` | `string` | No | Email |
| `PhoneNumber` | `string` | Yes | Phone number |

---

#### `EmployeeInsertDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `FirstName` | `string` | Yes | First name |
| `LastName` | `string` | Yes | Last name |
| `Address` | `string` | Yes | Home address |
| `Cedula` | `string` | Yes | National ID (unique) |
| `PhoneNumber` | `string` | Yes | Phone number |
| `DateOfBirth` | `DateTime` | Yes | Date of birth (YYYY-MM-DD) |
| `Email` | `string` | Yes | Email address |

---

#### `EmployeeUpdateDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `FirstName` | `string` | Yes | First name |
| `LastName` | `string` | Yes | Last name |
| `Address` | `string` | Yes | Home address |
| `Cedula` | `string` | Yes | National ID (unique) |
| `PhoneNumber` | `string` | Yes | Phone number |
| `DateOfBirth` | `DateTime` | Yes | Date of birth |
| `Email` | `string` | Yes | Email address |

---

#### `EmployeePositionInsertDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `EmployeeId` | `string` | Yes | Employee ID |
| `PositionId` | `int` | Yes | Position ID to assign |
| `StartDate` | `DateTime` | No | Start date (defaults to `DateTime.UtcNow`) |

---

#### `PositionInsertDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Name` | `string` | Yes | Position name |
| `Salary` | `decimal` | Yes | Salary (>= 1) |
| `DepartmentId` | `int` | Yes | Department ID |

---

#### `PositionUpdateDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Name` | `string` | Yes | Position name |
| `Salary` | `decimal` | Yes | Salary (>= 0) |
| `DepartmentId` | `int` | Yes | Department ID |

---

#### `DepartmentInsertDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Name` | `string` | Yes | Department name (unique) |

---

#### `DepartmentUpdateDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Name` | `string` | Yes | Department name (unique) |

---

#### `EvaluationInsertDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `EmployeeId` | `string` | Yes | Employee ID |
| `EvaluationDate` | `DateTime` | Yes | Evaluation date |
| `Criteria` | `List<EvaluationCriterionInsertDto>` | Yes | At least one entry |

---

#### `EvaluationCriterionInsertDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `CriterionId` | `int` | Yes | Criterion ID |
| `Score` | `float` | Yes | Score in [0.0, 5.0] |
| `Observation` | `string?` | No | Optional observation |

---

#### `CriterionInsertDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Name` | `string` | Yes | Criterion name (unique) |

---

#### `CriterionUpdateDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Name` | `string` | Yes | Criterion name (unique) |

---

#### `AuthDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Email` | `string` | Yes | Account email |
| `Password` | `string` | Yes | Account password |

---

#### `RefreshRequest`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `JwtToken` | `string` | Yes | Expired JWT to refresh |

---

#### `ForgotPasswordDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Email` | `string` | Yes | Account email |
| `Origin` | `string` | Yes | Base URL for reset link |

---

#### `ResetPasswordDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Password` | `string` | Yes | New password |
| `AccountId` | `string` | Yes | Account ID |
| `Token` | `string` | Yes | Base64-encoded reset token |

---

#### `RevokeAccessRequestDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `SessionId` | `string` | Yes | Session ID to revoke |
| `ActionMadeByAccountId` | `string` | Yes | Admin account ID |

---

#### `RevokeSessionRequest`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `SessionId` | `string` | Yes | Session ID to revoke |

---

#### `SaveAccountDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Name` | `string` | Yes | First name |
| `LastName` | `string` | Yes | Last name |
| `Email` | `string` | Yes | Email (unique) |
| `Password` | `string` | Yes | Password |
| `IdCard` | `string` | Yes | National ID |
| `Role` | `string` | Yes | `Admin` or `Supervisor` |
| `PhoneNumber` | `string` | No | Phone number |

---

#### `EditAccountDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `string` | Yes | Account ID |
| `Name` | `string` | Yes | First name |
| `LastName` | `string` | Yes | Last name |
| `Email` | `string` | Yes | Email |
| `IdCard` | `string` | Yes | National ID |
| `Role` | `string` | Yes | `Admin` or `Supervisor` |
| `PhoneNumber` | `string` | No | Phone number |

---

#### `ChangeStatusDto`

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `AccountId` | `string` | Yes | Account ID to toggle |

---

### 4.2 Response DTOs

---

#### `VacantDto`

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | ULID identifier |
| `Title` | `string` | Vacancy title |
| `Description` | `string` | Full job description |
| `Profile` | `string` | Ideal candidate profile |
| `PublicationDate` | `DateTime` | Publication date |
| `HiringDate` | `DateTime?` | Hiring date (null if not hired yet) |
| `Status` | `string` | `Open`, `Closed`, or `Cancelled` |

---

#### `VacancyApplicationResultDto`

| Property | Type | Description |
|----------|------|-------------|
| `ApplicationId` | `string` | Application record ID |
| `VacantId` | `string` | Vacancy ID |
| `CandidateId` | `string` | Candidate ID |
| `CandidateFullName` | `string` | Candidate full name |
| `CvUrl` | `string` | URL to the uploaded CV |
| `Status` | `string` | Application status |
| `Score` | `float` | NLP compatibility score [0.0, 1.0] |

---

#### `CandidateDto`

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Candidate ID |
| `Names` | `string` | First name |
| `LastNames` | `string` | Last name |
| `Email` | `string` | Email address |
| `PhoneNumber` | `string` | Phone number |
| `CvUrl` | `string` | URL to the uploaded CV |

---

#### `EmployeeDto`

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Employee ID |
| `FirstName` | `string` | First name |
| `LastName` | `string` | Last name |
| `Address` | `string` | Home address |
| `Cedula` | `string` | National ID |
| `PhoneNumber` | `string` | Phone number |
| `DateOfBirth` | `DateTime` | Date of birth |
| `Email` | `string?` | Email address |
| `IsActive` | `bool` | Active status |

---

#### `EmployeeListDto`

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Employee ID |
| `FullName` | `string` | Full name |
| `Cedula` | `string` | National ID |
| `PhoneNumber` | `string` | Phone number |
| `IsActive` | `bool` | Active status |

---

#### `EmployeeHistoryDto`

| Property | Type | Description |
|----------|------|-------------|
| `PositionName` | `string` | Position name |
| `DepartmentName` | `string` | Department name |
| `StartDate` | `DateTime` | Start date of assignment |
| `EndDate` | `DateTime?` | End date (null if still active) |

---

#### `EmployeePositionDto`

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `int` | Assignment record ID |
| `EmployeeId` | `string` | Employee ID |
| `EmployeeFullName` | `string` | Employee full name |
| `PositionId` | `int` | Position ID |
| `PositionName` | `string` | Position name |
| `DepartmentName` | `string` | Department name |
| `StartDate` | `DateTime` | Start date |
| `EndDate` | `DateTime?` | End date (null if still active) |

---

#### `PositionDto`

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `int` | Position ID |
| `Name` | `string` | Position name |
| `Salary` | `decimal` | Salary |
| `DepartmentId` | `int` | Department ID |
| `DepartmentName` | `string` | Department name |

---

#### `DepartmentDto`

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `int` | Department ID |
| `Name` | `string` | Department name |

---

#### `EvaluationDto`

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Evaluation ID |
| `EmployeeId` | `string` | Employee ID |
| `EmployeeFullName` | `string` | Employee full name |
| `PositionName` | `string` | Position name at time of evaluation |
| `Date` | `DateTime` | Evaluation date |
| `AverageScore` | `float` | Average score across criteria |
| `Criteria` | `List<EvaluationDetailDto>` | Criterion entries |

---

#### `EvaluationDetailDto`

| Property | Type | Description |
|----------|------|-------------|
| `CriterionId` | `int` | Criterion ID |
| `CriterionName` | `string` | Criterion name |
| `Score` | `float` | Score [0.0, 5.0] |
| `Observation` | `string?` | Observation |

---

#### `EvaluationHistoryDto`

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Evaluation ID |
| `Date` | `DateTime` | Evaluation date |
| `AverageScore` | `float` | Average score |
| `PositionName` | `string` | Position name at time of evaluation |
| `Criteria` | `List<EvaluationHistoryCriterionDto>` | All criterion entries |

---

#### `EvaluationHistoryCriterionDto`

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Criterion name |
| `Score` | `float` | Score [0.0, 5.0] |
| `Observation` | `string?` | Observation |

---

#### `CriterionDto`

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `int` | Criterion ID |
| `Name` | `string` | Criterion name |

---

#### `HiringTimeReportDto`

| Property | Type | Description |
|----------|------|-------------|
| `AverageDays` | `float` | Average days from publication to hire |
| `TotalClosedVacancies` | `int` | Number of closed vacancies |

---

#### `DepartmentPerformanceDto`

| Property | Type | Description |
|----------|------|-------------|
| `DepartmentName` | `string` | Department name |
| `AverageScore` | `float` | Average evaluation score |
| `EmployeeCount` | `int` | Number of employees with evaluations |

---

#### `EmployeeReportDto`

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Employee ID |
| `FullName` | `string` | Full name |
| `Cedula` | `string` | National ID |
| `Position` | `string` | Current position name |
| `Department` | `string` | Department name |
| `IsActive` | `bool` | Active status |

---

#### `PaginatedResponse<T>`

| Property | Type | Description |
|----------|------|-------------|
| `Items` | `IEnumerable<T>` | List of items for the current page |
| `Pagination` | `Pagination` | Pagination metadata |

---

#### `Pagination`

| Property | Type | Description |
|----------|------|-------------|
| `PageNumber` | `int` | Current page number (1-indexed) |
| `PageSize` | `int` | Items per page |
| `TotalCount` | `int` | Total number of items |

---

#### `UserSessionDto`

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Session ID |
| `DeviceInfo` | `string` | Device/browser info string |

---

#### `GetAccountDto`

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Account ID |
| `Name` | `string` | First name |
| `LastName` | `string` | Last name |
| `Email` | `string` | Email address |
| `IdCard` | `string` | National ID |
| `IsVerified` | `bool` | Whether the account is verified/active |
| `Role` | `string` | Role (`Admin` or `Supervisor`) |
| `PhoneNumber` | `string?` | Phone number |

---

## 5. Revision History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-04-24 | Initial version — all 10 controllers documented |
