# Functional Requirements — SIRUS (Smart Intelligent Resource Optimization System)

## 1. Introduction

This document lists the functional requirements for the SIRUS platform **REST API**. Each requirement is identified with a unique ID, a priority level (`High / Medium / Low`), and a reference to the business rule(s) it supports.

> **Scope note:** The platform exposes a RESTful API built with ASP.NET Core 10. Authentication and login are out of scope for this version. The database is PostgreSQL accessed via Entity Framework Core 10. Object mapping uses Mapster. The NLP ranking module uses **Microsoft.ML** (ML.NET) for text similarity scoring.

---

## 2. Requirement Conventions

- **ID format:** `FR-<MODULE>-<NNN>`
- **Priority:** High | Medium | Low

---

## 3. Recruitment — Vacancies

| ID | Description | Priority | BR Ref |
|----|-------------|----------|--------|
| FR-VAC-001 | The system shall allow creating a vacancy with title, profile, description, and publication date. | High | BR-REC-01 |
| FR-VAC-002 | The system shall enforce that a vacancy has one of three states: `Open`, `Closed`, or `Cancelled`. | High | BR-REC-02 |
| FR-VAC-003 | The system shall only accept new candidate applications for vacancies in `Open` state. | High | BR-REC-03 |
| FR-VAC-004 | The system shall automatically set a vacancy to `Closed` and record the `HireDate` when a candidate is hired. | High | BR-REC-04 |
| FR-VAC-005 | The system shall allow cancelling a vacancy at any point unless a candidate has already been hired for it. | Medium | BR-REC-05 |
| FR-VAC-006 | The system shall allow retrieving a paginated list of all vacancies, filterable by state. | High | BR-REC-06 |
| FR-VAC-007 | The system shall allow retrieving the full detail of a single vacancy by ID. | High | BR-REC-01 |

---

## 4. Recruitment — Candidates

| ID | Description | Priority | BR Ref |
|----|-------------|----------|--------|
| FR-CAN-001 | The system shall allow registering a candidate with first name, last name, email, and a CV file (PDF). | High | BR-REC-07 |
| FR-CAN-002 | The system shall enforce email uniqueness across all candidate records. | High | BR-REC-08 |
| FR-CAN-003 | The system shall allow a candidate to apply to multiple open vacancies. | High | BR-REC-09 |
| FR-CAN-004 | The system shall create a `CandidateToVacancy` record upon application, starting with status `Pending`. | High | BR-REC-11 |
| FR-CAN-005 | The system shall allow updating a candidate's application status following the defined lifecycle. | High | BR-REC-12 |
| FR-CAN-006 | The system shall prevent more than one candidate from being hired per vacancy. | High | BR-REC-13 |
| FR-CAN-007 | The system shall prevent reverting a `Hired` or `Rejected` status to any previous state. | High | BR-REC-14 |
| FR-CAN-008 | The system shall allow retrieving all candidates for a specific vacancy, sorted by compatibility score descending. | High | BR-REC-18 |

---

## 5. Recruitment — CV Ranking (NLP)

| ID | Description | Priority | BR Ref |
|----|-------------|----------|--------|
| FR-NLP-001 | The system shall extract full text from an uploaded PDF CV upon application submission. | High | BR-REC-15 |
| FR-NLP-002 | The system shall store the PDF CV file path as the `CvPath` field in the `CandidateToVacancy` record for subsequent analysis. | High | BR-REC-15 |
| FR-NLP-003 | The system shall compute a TF-IDF cosine similarity score between the extracted CV text and the concatenated vacancy profile and description texts using **Microsoft.ML** (ML.NET). | High | BR-REC-16 |
| FR-NLP-004 | The computed score shall be a `float` in the range [0.0, 1.0] stored in the `Score` field of `CandidateToVacancy`. | High | BR-REC-17 |
| FR-NLP-005 | The NLP scoring process shall be executed asynchronously as a background task after a CV is successfully uploaded; the upload endpoint must return a response before the score is computed. | High | BR-REC-20 |
| FR-NLP-006 | The system shall expose an endpoint to manually trigger score recalculation for all candidates of a given vacancy. | Medium | BR-REC-19 |

---

## 6. Employee Management

| ID | Description | Priority | BR Ref |
|----|-------------|----------|--------|
| FR-EMP-001 | The system shall allow creating an employee record with first name, last name, address, national ID (`Cedula`), phone number, date of birth, and active status. | High | BR-EMP-01 |
| FR-EMP-002 | The system shall enforce uniqueness of the `Cedula` field across all employee records. | High | BR-EMP-02 |
| FR-EMP-003 | The system shall allow deactivating an employee without deleting their record. | High | BR-EMP-03 |
| FR-EMP-004 | The system shall allow retrieving a paginated list of all employees, filterable by active status. | High | BR-EMP-05 |
| FR-EMP-005 | The system shall allow retrieving the full detail of a single employee by ID, including their current position. | High | BR-EMP-04 |
| FR-EMP-006 | The system shall allow updating an employee's personal information. | High | BR-EMP-05 |

---

## 7. Positions & Departments

| ID | Description | Priority | BR Ref |
|----|-------------|----------|--------|
| FR-POS-001 | The system shall allow creating a position with a name, salary, and department reference. | High | BR-POS-01, BR-POS-02 |
| FR-POS-002 | The system shall enforce that salary is a positive decimal value greater than zero. | High | BR-POS-03 |
| FR-POS-003 | The system shall allow assigning an employee to a position, recording the assignment date. | High | BR-POS-04 |
| FR-POS-004 | The system shall allow retrieving the full internal work history (all position assignments) of an employee. | High | BR-POS-05 |
| FR-POS-005 | The system shall allow creating a department with a unique name. | High | BR-POS-06 |
| FR-POS-006 | The system shall prevent deleting a department that has associated positions. | High | BR-INT-01 |
| FR-POS-007 | The system shall prevent deleting a position that has associated employee history records. | High | BR-INT-02 |

---

## 8. Performance Evaluation

| ID | Description | Priority | BR Ref |
|----|-------------|----------|--------|
| FR-EVAL-001 | The system shall allow creating an evaluation associated with a specific employee and an evaluation date. | High | BR-EVAL-01 |
| FR-EVAL-002 | The system shall require at least one criterion entry to create an evaluation. | High | BR-EVAL-02 |
| FR-EVAL-003 | The system shall enforce that each criterion score is a float between 0.0 and 5.0. | High | BR-EVAL-03 |
| FR-EVAL-004 | The system shall allow adding an optional observation text per criterion. | Medium | BR-EVAL-04 |
| FR-EVAL-005 | The system shall automatically calculate and store the overall average score for each evaluation upon creation. | High | BR-EVAL-05 |
| FR-EVAL-006 | The system shall allow CRUD operations on global evaluation criteria. | High | BR-EVAL-06 |
| FR-EVAL-007 | The system shall allow retrieving all evaluations for a specific employee in chronological order. | High | BR-EVAL-07 |
| FR-EVAL-008 | The system shall prevent deletion of submitted evaluations. | High | BR-EVAL-08 |
| FR-EVAL-009 | The system shall prevent deleting a criterion that is referenced in at least one evaluation. | High | BR-INT-04 |

---

## 9. Reports

| ID | Description | Priority | BR Ref |
|----|-------------|----------|--------|
| FR-REP-001 | The system shall provide an endpoint returning the average hiring time (in days) from vacancy publication to hire date, for all closed vacancies. | High | BR-REP-01 |
| FR-REP-002 | The system shall provide an endpoint returning the average evaluation score grouped by department. | High | BR-REP-02 |
| FR-REP-003 | The system shall provide an endpoint returning the full employee list with their active/inactive status. | High | BR-REP-03 |
| FR-REP-004 | The system shall allow exporting each of the three reports as a PDF file. | Medium | BR-REP-04 |

---

## 10. Result Pattern

| ID | Description | Priority | BR Ref |
|----|-------------|----------|--------|
| FR-ORP-001 | Every service and repository method that can succeed or fail for expected business reasons shall return an `Result<T>` (or non-generic `Result`) instead of throwing exceptions. | High | — |
| FR-ORP-002 | `Result<T>` shall expose at minimum: `bool IsSuccess`, `string? ErrorMessage`, and `T? Value`. | High | — |
| FR-ORP-003 | API controllers shall inspect the `Result` returned by service/application layer calls and produce appropriate HTTP responses (200, 400, 404, 409, etc.) based on `IsSuccess`. | High | — |
| FR-ORP-004 | Unexpected infrastructure exceptions shall propagate to the global error-handling middleware; `Result` is for **expected** business failures only. | High | — |

---

## 11. Generic Service with Mapster

| ID | Description | Priority | BR Ref |
|----|-------------|----------|--------|
| FR-MAP-001 | The application layer shall define a generic service interface `IGenericService<TEntity, TDto, TCreateDto, TUpdateDto, TKey>` providing CRUD operations. | High | — |
| FR-MAP-002 | All entity-to-DTO and DTO-to-entity mappings shall be handled exclusively by **Mapster**; no manual property assignment shall exist in service or controller code. | High | — |
| FR-MAP-003 | Mapster configuration shall be centralized in a static mapping configuration class registered at application startup. | High | — |
