# User Stories — SIRUS (Smart Intelligent Resource Optimization System)

## 1. Overview

This document contains the full user story backlog for the SIRUS platform. Stories are organized by module and include acceptance criteria, priority, and linked technical requirements.

> **Note:** Authentication (HU-18) is out of scope for this API version. HU-19 (database setup) and HU-20 (field validation) are cross-cutting technical stories included for planning completeness.

---

## 2. Story Conventions

- **ID:** `HU-NN`
- **Priority:** High | Medium | Low
- **Estimation:** Story points (Fibonacci: 1, 2, 3, 5, 8, 13)
- **Status:** Done | In Progress | Done

---

## 3. Module 1 — Intelligent Recruitment

---

### HU-01 — Create Vacancy

**As** an HR Administrator,
**I want** to register a new vacancy with requirements, profile, and description,
**So that** I can initiate a formal recruitment process.

**Priority:** High
**Estimate:** 3 SP
**Status:** Done

**Acceptance Criteria:**
- [x] `POST /api/vacancies` accepts `title`, `profile`, `description`, and `publicationDate`.
- [x] All four fields are required; missing any field returns `400 Bad Request`.
- [x] The newly created vacancy has status `Open` by default.
- [x] The response returns `201 Created` with the vacancy DTO including the generated ID.
- [x] The service method returns `Result<VacancyDto>.Success(...)` on valid input.

**Linked Requirements:** FR-VAC-001, BR-REC-01, BR-REC-02

---

### HU-02 — Upload CVs

**As** a recruiter and a candidate,
**I want** to upload CVs in PDF format for a specific vacancy,
**So that** they are stored in the system and linked to the application.

**Priority:** High
**Estimate:** 5 SP
**Status:** Done

**Acceptance Criteria:**
- [x] `POST /api/vacancies/{vacancyId}/applications` accepts multipart form data with candidate info and a PDF file.
- [x] Only PDF files are accepted; other types return `400 Bad Request`.
- [x] Files exceeding the configured maximum size (default 10 MB) are rejected with `400 Bad Request`.
- [x] A `CandidateToVacancy` record is created with `Status = Pending` and `Score = 0.0`.
- [x] The CV file is saved to the configured storage path.
- [x] The endpoint returns `201 Created` before the NLP ranking process completes.
- [x] Uploading to a vacancy not in `Open` status returns `409 Conflict`.

**Linked Requirements:** FR-CAN-001 to FR-CAN-004, FR-NLP-001, FR-NLP-002, FR-NLP-005, BR-REC-03, BR-REC-07

---

### HU-03 — Extract Basic CV Information

**As** a recruiter,
**I want** the system to identify experience, education, and skills from the CV,
**So that** I don't have to manually review each document.

**Priority:** High
**Estimate:** 8 SP
**Status:** Done

**Acceptance Criteria:**
- [x] When a CV is uploaded, the system extracts full text from the PDF using `PdfPig`.
- [x] The extracted text is stored and used for the ranking computation.
- [x] If text extraction fails (e.g., scanned image PDF with no text layer), the system logs the error and sets `Score = 0.0`; it does not block the upload response.
- [x] The text extraction runs asynchronously after the upload response is returned.

**Linked Requirements:** FR-NLP-001, FR-NLP-002, BR-REC-15

---

### HU-04 — Calculate Compatibility Score

**As** a recruiter,
**I want** the system to compare the candidate's profile with the vacancy,
**So that** I get a compatibility score for each application.

**Priority:** High
**Estimate:** 8 SP
**Status:** Done

**Acceptance Criteria:**
- [x] The system computes a TF-IDF cosine similarity score between CV text and the concatenation of the vacancy's `Profile` and `Description` using **Microsoft.ML**.
- [x] The score is a `float` in the range [0.0, 1.0] stored in `CandidateToVacancy.Score`.
- [x] The computation runs asynchronously via `RankingBackgroundService`.
- [x] `POST /api/vacancies/{vacancyId}/recalculate-scores` triggers recalculation for all candidates; returns `202 Accepted`.
- [x] The service returns `Result<float>.Success(score)` on success and `Result<float>.Failure(...)` on error.

**Linked Requirements:** FR-NLP-003 to FR-NLP-006, BR-REC-16 to BR-REC-20

---

### HU-05 — View Candidate Ranking

**As** a recruiter,
**I want** to see a list ordered by score,
**So that** I can quickly identify the best profiles.

**Priority:** High
**Estimate:** 3 SP
**Status:** Done

**Acceptance Criteria:**
- [x] `GET /api/vacancies/{vacancyId}/applications` returns a paginated list of candidates for the vacancy.
- [x] The list is sorted by `Score` descending by default.
- [x] Each entry includes: candidate name, email, score, status, and application ID.
- [x] If the vacancy does not exist, the endpoint returns `404 Not Found`.

**Linked Requirements:** FR-CAN-008, BR-REC-18

---

## 4. Module 2 — Employee Management

---

### HU-06 — Register Employee

**As** an HR Administrator,
**I want** to register the personal and employment data of an employee,
**So that** the information is organized in the system.

**Priority:** High
**Estimate:** 3 SP
**Status:** Done

**Acceptance Criteria:**
- [x] `POST /api/employees` accepts `firstName`, `lastName`, `address`, `cedula`, `phoneNumber`, `dateOfBirth`.
- [x] All fields are required; missing fields return `400 Bad Request`.
- [x] `Cedula` must be unique; a duplicate returns `409 Conflict`.
- [x] The employee is created with `IsActive = true`.
- [x] Response returns `201 Created` with the employee DTO.

**Linked Requirements:** FR-EMP-001, FR-EMP-002, BR-EMP-01, BR-EMP-02

---

### HU-07 — Edit Employee Information

**As** an HR Administrator,
**I want** to update an employee's data,
**So that** the information remains current.

**Priority:** High
**Estimate:** 2 SP
**Status:** Done

**Acceptance Criteria:**
- [x] `POST /api/evaluation-criteria` accepts a `name` field.
- [x] Criterion name must be unique; duplicate returns `409 Conflict`.
- [x] Returns `201 Created` with the criterion DTO.
- [x] `GET /api/evaluation-criteria` returns the full list of active criteria.
- [x] `DELETE /api/evaluation-criteria/{id}` is blocked if the criterion is used in any evaluation; returns `409 Conflict`.

**Linked Requirements:** FR-EVAL-006, FR-EVAL-009, BR-EVAL-06, BR-INT-04

---

### HU-08 — List All Employees

**As** an HR administrator,  
**I want** to view all registered employees,  
**So that** I have general personnel control.

**Priority:** High  
**Estimate:** 2 SP
**Status:** Done

**Acceptance Criteria:**
- [x] `GET /api/employees` returns a paginated list with `page` and `pageSize` parameters.
- [x] Supports optional `isActive` filter (`true` / `false` / omit for all).
- [x] Each item includes: ID, full name, `Cedula`, phone, active status.
- [x] Returns `200 OK` with a pagination envelope (`totalCount`, `page`, `pageSize`, `items`).

**Linked Requirements:** FR-EMP-004, BR-EMP-05

---

### HU-09 — View Internal Work History

**As** an HR administrator,  
**I want** to consult the positions held by an employee,  
**So that** I can understand their career path within the company.

**Priority:** Medium  
**Estimate:** 3 SP
**Status:** Done

**Acceptance Criteria:**
- [x] `GET /api/employees/{id}/history` returns all `EmployeePosition` records for the employee.
- [x] Each entry includes: position name, department name, assigned date.
- [x] Results are sorted by assignment date descending.
- [x] Returns `404 Not Found` if the employee does not exist.

**Linked Requirements:** FR-POS-004, BR-POS-05

---

## 5. Module 3 — Performance Evaluation

---

### HU-10 — Create Evaluation Criteria

**As** an HR administrator,  
**I want** to define evaluation criteria (responsibility, teamwork, etc.),  
**So that** evaluations are structured and consistent.

**Priority:** High  
**Estimate:** 2 SP
**Status:** Done

**Acceptance Criteria:**
- [x] `POST /api/evaluation-criteria` accepts a `name` field.
- [x] Criterion name must be unique; duplicate returns `409 Conflict`.
- [x] Returns `201 Created` with the criterion DTO.
- [x] `GET /api/evaluation-criteria` returns the full list of active criteria.
- [x] `DELETE /api/evaluation-criteria/{id}` is blocked if the criterion is used in any evaluation; returns `409 Conflict`.

**Linked Requirements:** FR-EVAL-006, FR-EVAL-009, BR-EVAL-06, BR-INT-04

---

### HU-11 — Register Evaluation

**As** a supervisor,
**I want** to register scores and observations for an employee,
**So that** their performance is measured.

**Priority:** High
**Estimate:** 5 SP
**Status:** Done

**Acceptance Criteria:**
- [x] `POST /api/evaluations` accepts `employeeId`, `evaluationDate`, and a list of `{ criterionId, score, observation? }` entries.
- [x] At least one criterion entry is required; empty list returns `400 Bad Request`.
- [x] Each `score` must be in [0.0, 5.0]; out-of-range values return `400 Bad Request`.
- [x] The system automatically computes and stores the average score across all criteria.
- [x] Returns `201 Created` with the full evaluation DTO including the computed average.
- [x] Returns `404 Not Found` if `employeeId` does not exist.

**Linked Requirements:** FR-EVAL-001 to FR-EVAL-005, BR-EVAL-01 to BR-EVAL-05

---

### HU-12 — View Evaluation History

**As** an HR administrator,
**I want** to see previous evaluations for an employee,
**So that** I can analyze their evolution over time.

**Priority:** Medium
**Estimate:** 2 SP
**Status:** Done

**Acceptance Criteria:**
- [x] `GET /api/employees/{id}/evaluations` returns all evaluations for the employee.
- [x] Each item includes: evaluation date, average score, list of criteria with scores and observations.
- [x] Results are sorted by evaluation date descending.
- [x] Returns `404 Not Found` if the employee does not exist.

**Linked Requirements:** FR-EVAL-007, BR-EVAL-07

---

### HU-13 — Generate Automatic Result

**As** a supervisor,
**I want** the system to calculate a performance average,
**So that** I can make objective decisions.

**Priority:** High
**Estimate:** 2 SP
**Status:** Done

**Acceptance Criteria:**
- [x] The average is computed as `sum(scores) / count(criteria)` and rounded to two decimal places.
- [x] The average is returned in the evaluation DTO as `averageScore`.
- [x] Recomputing scores by modifying an evaluation is not allowed (immutable once created).

**Linked Requirements:** FR-EVAL-005, BR-EVAL-05

---

## 6. Module 4 — Reports

---

### HU-14 — Average Hiring Time Report

**As** a manager,
**I want** to see the average time from publication to hiring,
**So that** I can evaluate recruitment efficiency.

**Priority:** Medium
**Estimate:** 3 SP
**Status:** Done

**Acceptance Criteria:**
- [x] `GET /api/reports/hiring-time` returns the average number of days between `PublicationDate` and `HireDate` across all closed vacancies.
- [x] Result includes: `averageDays` (float), `totalClosedVacancies` (int).
- [x] Returns `200 OK` with the report data.
- [x] If no closed vacancies exist, returns `averageDays = 0` with `totalClosedVacancies = 0`.

**Linked Requirements:** FR-REP-001, BR-REP-01

---

### HU-15 — Performance by Department Report

**As** a manager,
**I want** to visualize the average performance score by department,
**So that** I can identify underperforming areas.

**Priority:** Medium
**Estimate:** 3 SP
**Status:** Done

**Acceptance Criteria:**
- [x] `GET /api/reports/performance-by-department` returns a list of departments with their average evaluation score.
- [x] Each entry includes: `departmentName`, `averageScore` (float), `employeeCount` (int).
- [x] Only departments with at least one evaluation are included.
- [x] Results are sorted by `averageScore` descending.

**Linked Requirements:** FR-REP-002, BR-REP-02

---

### HU-16 — General Employee Report

**As** a manager,
**I want** to view the general status of all personnel,
**So that** I have organizational control.

**Priority:** Medium
**Estimate:** 2 SP
**Status:** Done

**Acceptance Criteria:**
- [x] `GET /api/reports/employees` returns all employees with their status.
- [x] Each entry includes: full name, `Cedula`, current position, department, active status.
- [x] Employees without a current position show `"Unassigned"` for position and department.

**Linked Requirements:** FR-REP-003, BR-REP-03

---

### HU-17 — Export Reports as PDF

**As** an HR administrator,
**I want** to export reports as PDF files,
**So that** I can share them with management.

**Priority:** Medium
**Estimate:** 5 SP
**Status:** Done

**Acceptance Criteria:**
- [x] `GET /api/reports/hiring-time/export` returns a PDF file with `Content-Type: application/pdf`.
- [x] `GET /api/reports/performance-by-department/export` returns a PDF file.
- [x] `GET /api/reports/employees/export` returns a PDF file.
- [x] PDFs are generated using **QuestPDF** and include the report title, generation date, and tabular data.
- [x] PDF generation failure returns `500 Internal Server Error` with a descriptive message (no stack trace).

**Linked Requirements:** FR-REP-004, BR-REP-04

---

## 7. Technical Stories

---

### HU-18 — Implement Login System for Administrator

**Status:** Out of Scope
**Note:** Authentication and session management are explicitly out of scope for this version. All endpoints are treated as pre-authorized. This story is deferred to a future iteration where JWT or ASP.NET Core Identity will be introduced.

---

### HU-19 — Implement Database for Information Storage

**As** the development team,
**I want** to configure PostgreSQL with EF Core migrations,
**So that** all data is persisted correctly.

**Priority:** High
**Estimate:** 3 SP
**Status:** In Progress (Entities configured, no migrations created)

**Acceptance Criteria:**
- [ ] `AppDbContext` is configured with the `Npgsql.EntityFrameworkCore.PostgreSQL` provider.
- [ ] All entities have corresponding `IEntityTypeConfiguration<T>` classes with explicit column types, constraints, and indexes.
- [ ] The initial EF Core migration is generated and applies successfully against a clean PostgreSQL instance.
- [ ] `dotnet ef database update` runs without errors in the development environment.
- [ ] The connection string is read from environment variables or user secrets; it is never hardcoded.

**Linked Requirements:** NFR-COMPAT-004, NFR-MAINT-006, NFR-SEC-001

---

### HU-20 — Validate Required Fields in All Endpoints

**As** the development team,
**I want** all API endpoints to validate input data consistently,
**So that** invalid requests are rejected with clear, actionable error messages.

**Priority:** High
**Estimate:** 2 SP
**Status:** Done

**Acceptance Criteria:**
- [x] All request DTOs use Data Annotations (`[Required]`, `[Range]`, `[StringLength]`, etc.) or FluentValidation.
- [x] Invalid requests return `400 Bad Request` with a JSON error body listing all validation failures.
- [x] The global exception middleware catches unhandled exceptions and returns `500 Internal Server Error` with a generic message (no stack traces in production).
- [x] Validation errors follow the standard error envelope: `{ "statusCode": 400, "message": "Validation failed", "errors": [...] }`.

**Linked Requirements:** NFR-USAB-001, NFR-USAB-003, NFR-SEC-003

---

## 8. Sprint Planning Suggestion

| Sprint | Stories | Theme |
|--------|---------|-------|
| **Sprint 1** | HU-19, HU-20, HU-01, HU-06 | Foundation + Core Entities |
| **Sprint 2** | HU-02, HU-03, HU-04, HU-07, HU-08 | CV Upload + NLP + Employee CRUD |
| **Sprint 3** | HU-05, HU-09, HU-10, HU-11 | Ranking + History + Evaluations |
| **Sprint 4** | HU-12, HU-13, HU-14, HU-15, HU-16, HU-17 | Reports + PDF Export |