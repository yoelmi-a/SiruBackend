# Use Cases — SIRUS (Smart Intelligent Resource Optimization System)

## 1. Actors

| Actor | Description |
|-------|-------------|
| **HR Administrator** | Internal staff with full management access to all modules via the API. |
| **Supervisor** | Internal staff who can register and consult performance evaluations. |
| **RankingService** | Internal background service that performs NLP scoring on CV-vacancy pairs. |
| **System (Background)** | The application process that executes asynchronous tasks. |
| **Candidate** | External person interested in vacancies. |

---

## 2. Use Case Index

| ID | Use Case | Primary Actor |
|----|----------|--------------|
| UC-01 | Create Vacancy | HR Administrator |
| UC-02 | Update / Cancel Vacancy | HR Administrator |
| UC-03 | Apply to Vacancy (Upload CV) | HR Administrator / Candidate |
| UC-04 | Rank Candidates for Vacancy | RankingService / HR Administrator |
| UC-05 | Advance Candidate Application Status | HR Administrator |
| UC-06 | Register Employee | HR Administrator |
| UC-07 | Update Employee Information | HR Administrator |
| UC-08 | List Employees | HR Administrator |
| UC-09 | View Employee Work History | HR Administrator |
| UC-10 | Manage Positions & Departments | HR Administrator |
| UC-11 | Create Performance Evaluation | Supervisor |
| UC-12 | View Evaluation History | HR Administrator / Supervisor |
| UC-13 | Manage Evaluation Criteria | HR Administrator |
| UC-14 | Generate Hiring Time Report | HR Administrator |
| UC-15 | Generate Performance by Department Report | HR Administrator |
| UC-16 | Generate General Employee Report | HR Administrator |
| UC-17 | Export Report as PDF | HR Administrator |

---

## 3. Use Case Descriptions

---

### UC-01 — Create Vacancy

**Actor:** HR Administrator  
**Preconditions:** None.  
**Postconditions:** A new vacancy record is created with `Open` status and a system-generated ULID.

**Main Flow:**
1. The client sends a `POST /api/vacancies` request with title, profile, description, and publication date.
2. The application layer validates all required fields via the command DTO.
3. The vacancy service returns `Result<VacancyDto>` with the created record.
4. The controller returns `201 Created` with the new vacancy in the response body.

**Exception Flows:**
- Missing required field → service returns `Result` failure; controller responds with `400 Bad Request`.

**Related Requirements:** FR-VAC-001, BR-REC-01, BR-REC-02

---

### UC-02 — Update / Cancel Vacancy

**Actor:** HR Administrator  
**Preconditions:** The vacancy exists.  
**Postconditions:** The vacancy is updated with the new values or its status is set to `Cancelled`.

**Main Flow (Update):**
1. The client sends `PUT /api/vacancies/{id}` with updated fields.
2. The application layer validates and applies the changes.
3. Service returns `Result<VacancyDto>` success; controller returns `200 OK`.

**Main Flow (Cancel):**
1. The client sends `PATCH /api/vacancies/{id}/cancel`.
2. The vacancy service validates that no candidate has been hired for this vacancy.
3. Status is set to `Cancelled`; service returns `Result` success; controller returns `200 OK`.

**Exception Flows:**
- Vacancy not found → `Result` failure; controller returns `404 Not Found`.
- Attempting to cancel a vacancy with a hired candidate → `Result` failure; controller returns `409 Conflict`.

**Related Requirements:** FR-VAC-005, BR-REC-05

---

### UC-03 — Apply to Vacancy (Upload CV)

**Actor:** Candidate  
**Preconditions:** The vacancy is in `Open` state; the candidate record exists or is created in the same request.  
**Postconditions:** A `CandidateToVacancy` record is created with `Pending` status; the CV PDF is stored; NLP scoring is triggered asynchronously.

**Main Flow:**
1. The client sends `POST /api/vacancies/{vacancyId}/applications` with candidate info and a PDF file.
2. The application layer validates the vacancy is `Open` and the candidate email is unique (or retrieves the existing candidate).
3. The PDF is saved to file storage; the file path is recorded in `CvPath`.
4. A `CandidateToVacancy` record is created with `Score = 0.0` and `Status = Pending`.
5. The service enqueues an asynchronous background task for NLP scoring.
6. Service returns `Result<ApplicationDto>` success; controller returns `201 Created`.
7. The background task (UC-04) runs independently after the response is sent.

**Exception Flows:**
- Vacancy not in `Open` state → `Result` failure; `409 Conflict`.
- Uploaded file is not a PDF or exceeds maximum size → `400 Bad Request`.
- Candidate email already linked to this vacancy → `409 Conflict`.

**Related Requirements:** FR-CAN-001 to FR-CAN-004, FR-NLP-001, FR-NLP-002, FR-NLP-005, BR-REC-03, BR-REC-07 to BR-REC-11

---

### UC-04 — Rank Candidates for Vacancy

**Actor:** RankingService (Background)  
**Preconditions:** A `CandidateToVacancy` record exists with a stored CV path; the vacancy has a profile and description.  
**Postconditions:** The `Score` field on `CandidateToVacancy` is updated with the computed TF-IDF cosine similarity value.

**Main Flow (Asynchronous — triggered from UC-03):**
1. The `IRankingService` background task receives the `candidateToVacancyId`.
2. The service reads the CV PDF file and extracts full text using `PdfPig`.
3. The service loads the vacancy's profile and description text from the database.
4. Microsoft.ML computes a TF-IDF cosine similarity score between the CV text and the vacancy text.
5. The score (float [0.0, 1.0]) is written to the `Score` field of the `CandidateToVacancy` record.
6. The service returns `Result` success and logs the result.

**Main Flow (Manual Trigger):**
1. The client sends `POST /api/vacancies/{vacancyId}/recalculate-scores`.
2. The application layer enqueues recalculation for all candidates of the given vacancy.
3. Controller returns `202 Accepted`.

**Exception Flows:**
- CV PDF file not found on disk → `Result` failure; error is logged; score remains at previous value.
- ML computation error → `Result` failure; error is logged.

**Related Requirements:** FR-NLP-003 to FR-NLP-006, BR-REC-15 to BR-REC-20

---

### UC-05 — Advance Candidate Application Status

**Actor:** HR Administrator  
**Preconditions:** The `CandidateToVacancy` record exists.  
**Postconditions:** The application status is updated; if set to `Hired`, the vacancy automatically closes.

**Main Flow:**
1. The client sends `PATCH /api/applications/{id}/status` with the new status value.
2. The service validates the transition is allowed per the lifecycle rules.
3. If the new status is `Hired`, the service verifies no other candidate is already hired for the same vacancy, then closes the vacancy.
4. Service returns `Result` success; controller returns `200 OK`.

**Exception Flows:**
- Invalid status transition → `Result` failure; `422 Unprocessable Entity`.
- Another candidate is already hired for the vacancy → `Result` failure; `409 Conflict`.

**Related Requirements:** FR-CAN-005 to FR-CAN-007, BR-REC-12 to BR-REC-14

---

### UC-06 — Register Employee

**Actor:** HR Administrator  
**Preconditions:** None.  
**Postconditions:** A new employee record is created and active.

**Main Flow:**
1. The client sends `POST /api/employees` with all required personal and employment fields.
2. The service validates that `Cedula` is unique.
3. Service returns `Result<EmployeeDto>` success; controller returns `201 Created`.

**Exception Flows:**
- Duplicate `Cedula` → `Result` failure; `409 Conflict`.
- Missing required fields → `400 Bad Request`.

**Related Requirements:** FR-EMP-001, FR-EMP-002, BR-EMP-01, BR-EMP-02

---

### UC-07 — Update Employee Information

**Actor:** HR Administrator  
**Preconditions:** Employee record exists.  
**Postconditions:** Employee record is updated with new values.

**Main Flow:**
1. The client sends `PUT /api/employees/{id}` with updated fields.
2. Service validates and updates the record.
3. Returns `200 OK` with the updated DTO.

**Exception Flows:**
- Employee not found → `404 Not Found`.
- Duplicate `Cedula` after edit → `409 Conflict`.

**Related Requirements:** FR-EMP-006, BR-EMP-05

---

### UC-08 — List Employees

**Actor:** HR Administrator  
**Preconditions:** None.  
**Postconditions:** A paginated employee list is returned, optionally filtered by status.

**Main Flow:**
1. The client sends `GET /api/employees?page=1&pageSize=20&isActive=true`.
2. The repository applies filters and pagination.
3. Returns `200 OK` with a paginated result envelope.

**Related Requirements:** FR-EMP-004, BR-EMP-05

---

### UC-09 — View Employee Work History

**Actor:** HR Administrator  
**Preconditions:** Employee record exists.  
**Postconditions:** A list of all `EmployeePosition` records for the employee is returned.

**Main Flow:**
1. The client sends `GET /api/employees/{id}/history`.
2. The repository retrieves all position assignments for the employee, including position and department names.
3. Returns `200 OK` with the history list sorted by date descending.

**Related Requirements:** FR-POS-004, BR-POS-05

---

### UC-10 — Manage Positions & Departments

**Actor:** HR Administrator  
**Preconditions:** None.  
**Postconditions:** Position or department is created, updated, or deleted (with integrity checks).

**Main Flow (Department):**
1. `POST /api/departments` — Create a department.
2. `DELETE /api/departments/{id}` — Delete if no positions exist for it.

**Main Flow (Position):**
1. `POST /api/positions` — Create a position linked to a department.
2. `POST /api/employees/{id}/positions` — Assign a position to an employee with a date.
3. `DELETE /api/positions/{id}` — Delete if no employee history exists for it.

**Exception Flows:**
- Deleting a department with existing positions → `Result` failure; `409 Conflict`.
- Deleting a position with existing employee history → `Result` failure; `409 Conflict`.

**Related Requirements:** FR-POS-001 to FR-POS-007, BR-POS-01 to BR-POS-06, BR-INT-01, BR-INT-02

---

### UC-11 — Create Performance Evaluation

**Actor:** Supervisor  
**Preconditions:** The employee exists; at least one evaluation criterion is configured.  
**Postconditions:** An evaluation record is created with criteria scores and the computed average.

**Main Flow:**
1. The client sends `POST /api/evaluations` with employee ID, evaluation date, and a list of criterion-score-observation entries.
2. The service validates that at least one criterion is present and all scores are in [0.0, 5.0].
3. The service computes the average score and stores it.
4. Service returns `Result<EvaluationDto>` success; controller returns `201 Created`.

**Exception Flows:**
- Empty criteria list → `Result` failure; `400 Bad Request`.
- Score out of [0.0, 5.0] range → `400 Bad Request`.
- Employee not found → `404 Not Found`.

**Related Requirements:** FR-EVAL-001 to FR-EVAL-005, BR-EVAL-01 to BR-EVAL-05

---

### UC-12 — View Evaluation History

**Actor:** HR Administrator / Supervisor  
**Preconditions:** Employee has at least one evaluation.  
**Postconditions:** A chronological list of evaluations for the employee is returned.

**Main Flow:**
1. The client sends `GET /api/employees/{id}/evaluations`.
2. The repository returns all evaluations with criteria details.
3. Returns `200 OK` sorted by date ascending.

**Related Requirements:** FR-EVAL-007, BR-EVAL-07

---

### UC-13 — Manage Evaluation Criteria

**Actor:** HR Administrator  
**Preconditions:** None.  
**Postconditions:** Criterion is created, updated, or deleted (with integrity check).

**Main Flow:**
1. `POST /api/evaluation-criteria` — Create a new criterion.
2. `PUT /api/evaluation-criteria/{id}` — Update criterion name.
3. `DELETE /api/evaluation-criteria/{id}` — Delete if not referenced in any evaluation.

**Exception Flows:**
- Deleting a used criterion → `Result` failure; `409 Conflict`.

**Related Requirements:** FR-EVAL-006, FR-EVAL-009, BR-EVAL-06, BR-INT-04

---

### UC-14 — Generate Hiring Time Report

**Actor:** HR Administrator  
**Preconditions:** At least one vacancy is in `Closed` state.  
**Postconditions:** Average days from publication to hire date is returned.

**Main Flow:**
1. Client sends `GET /api/reports/hiring-time`.
2. Service queries all closed vacancies and computes the average difference in days between `PublicationDate` and `HireDate`.
3. Returns `200 OK` with the report data.

**Related Requirements:** FR-REP-001, BR-REP-01

---

### UC-15 — Generate Performance by Department Report

**Actor:** HR Administrator  
**Preconditions:** At least one evaluation exists.  
**Postconditions:** Average evaluation score grouped by department is returned.

**Main Flow:**
1. Client sends `GET /api/reports/performance-by-department`.
2. Service joins evaluations → employees → positions → departments and computes average scores per department.
3. Returns `200 OK` with the aggregated data.

**Related Requirements:** FR-REP-002, BR-REP-02

---

### UC-16 — Generate General Employee Report

**Actor:** HR Administrator  
**Preconditions:** None.  
**Postconditions:** Full employee list with status is returned.

**Main Flow:**
1. Client sends `GET /api/reports/employees`.
2. Service retrieves all employees with active/inactive status and their current position.
3. Returns `200 OK`.

**Related Requirements:** FR-REP-003, BR-REP-03

---

### UC-17 — Export Report as PDF

**Actor:** HR Administrator  
**Preconditions:** Report data is available.  
**Postconditions:** A downloadable PDF file is returned.

**Main Flow:**
1. Client sends `GET /api/reports/{report-type}/export` with `Accept: application/pdf` header (or a dedicated export endpoint).
2. The application layer fetches the report data and renders it into a PDF via the PDF generation service (e.g., `QuestPDF`).
3. Returns `200 OK` with `Content-Type: application/pdf` and the file bytes.

**Exception Flows:**
- Report type not found → `404 Not Found`.
- PDF generation failure → `500 Internal Server Error` with a descriptive error message.

**Related Requirements:** FR-REP-004, BR-REP-04
