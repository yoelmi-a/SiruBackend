# Business Rules — SIRUS (Smart Intelligent Resource Optimization System)

## 1. Overview

SIRUS is a Human Resources management platform designed to optimize recruitment, employee management, performance evaluation, and reporting processes for small and medium-sized enterprises (SMEs). This document describes the business rules that govern the platform's behavior and must be enforced at the domain and application layers.

> **Scope note:** Authentication and login are out of scope for this version. The platform exposes a RESTful API consumed by a frontend client. The ranking module uses NLP to compare CV text against job vacancy descriptions.

---

## 2. Users & Roles

| Rule ID | Rule Description |
|---------|-----------------|
| BR-U01 | There are two types of users: **HR Administrator** and **Supervisor**. |
| BR-U02 | An HR Administrator has full access to all modules: recruitment, employees, evaluations, and reports. |
| BR-U03 | A Supervisor can register and consult evaluations for employees under their charge. |
| BR-U04 | Authentication and session management are out of scope; all API endpoints are assumed to be pre-authorized in this version. |

---

## 3. Recruitment Module

### 3.1 Vacancies

| Rule ID | Rule Description |
|---------|-----------------|
| BR-REC-01 | A vacancy must have a title, a profile description, a full job description, and a publication date before it can be opened. |
| BR-REC-02 | A vacancy can be in one of three states: `Open`, `Closed`, or `Cancelled`. |
| BR-REC-03 | Only vacancies in `Open` state can receive new candidate applications. |
| BR-REC-04 | A vacancy transitions to `Closed` when a candidate is hired for that position; the `HireDate` must be recorded at that moment. |
| BR-REC-05 | A vacancy can be `Cancelled` at any time regardless of its current state, as long as no candidate has been marked `Hired`. |
| BR-REC-06 | Only HR Administrators can create, edit, or cancel vacancies. |

### 3.2 Candidates

| Rule ID | Rule Description |
|---------|-----------------|
| BR-REC-07 | A candidate must provide a first name, last name, email address, and a CV file (PDF format) to apply for a vacancy. |
| BR-REC-08 | A candidate's email must be unique within the system. |
| BR-REC-09 | A candidate can apply to multiple open vacancies. |
| BR-REC-10 | Each application (`CandidateToVacancy`) tracks the candidate's status within that specific vacancy process independently. |

### 3.3 Candidate Application Status

| Rule ID | Rule Description |
|---------|-----------------|
| BR-REC-11 | A candidate application starts with status `Pending` upon submission. |
| BR-REC-12 | The status transitions are: `Pending` → `UnderEvaluation` → `Hired` or `Rejected`. |
| BR-REC-13 | Only one candidate per vacancy can be set to `Hired`. When a candidate is hired, the vacancy status must automatically change to `Closed`. |
| BR-REC-14 | A `Hired` or `Rejected` application cannot be reverted to a previous status. |

### 3.4 CV Ranking (NLP)

| Rule ID | Rule Description |
|---------|-----------------|
| BR-REC-15 | When a CV is uploaded for a vacancy, the system must extract the full text from the PDF and store it for analysis. |
| BR-REC-16 | The system must calculate a compatibility score between the candidate's CV text and the vacancy's profile and description text. |
| BR-REC-17 | The compatibility score must be a float value between 0.0 and 1.0, where 1.0 represents maximum compatibility. |
| BR-REC-18 | The ranking of candidates for a vacancy must be sorted in descending order by compatibility score. |
| BR-REC-19 | Recalculation of scores must be possible at any time without altering the candidate's application status. |
| BR-REC-20 | The NLP scoring process must be asynchronous and must not block the CV upload response. |

---

## 4. Employee Management Module

| Rule ID | Rule Description |
|---------|-----------------|
| BR-EMP-01 | An employee record must include: first name, last name, address, national ID (`Cedula`), phone number, date of birth, and active status. |
| BR-EMP-02 | The `Cedula` (national ID) must be unique across all employee records. |
| BR-EMP-03 | An employee can be deactivated (set to inactive) without deleting their record from the system. |
| BR-EMP-04 | An employee must be assigned to at least one position (`Position`) in order to appear in payroll-related reports. |
| BR-EMP-05 | Only HR Administrators can register, edit, or deactivate employees. |

---

## 5. Positions & Departments

| Rule ID | Rule Description |
|---------|-----------------|
| BR-POS-01 | A position (`Position`) must belong to exactly one department (`Department`). |
| BR-POS-02 | A position must have a name and a defined salary (`Salary`). |
| BR-POS-03 | Salary must be a positive decimal value greater than zero. |
| BR-POS-04 | An `EmployeePosition` record tracks which position an employee holds and the date it was assigned. |
| BR-POS-05 | An employee's work history is derived from all `EmployeePosition` records associated with that employee. |
| BR-POS-06 | Only HR Administrators can create or modify positions and departments. |

---

## 6. Performance Evaluation Module

| Rule ID | Rule Description |
|---------|-----------------|
| BR-EVAL-01 | An evaluation is always associated with exactly one employee and must record the evaluation date. |
| BR-EVAL-02 | An evaluation must contain at least one `EvaluationCriterion` entry in order to be saved. |
| BR-EVAL-03 | Each criterion within an evaluation must have a score (`Score`) in the range of 0.0 to 5.0. |
| BR-EVAL-04 | An optional `Observation` field may be included per criterion. |
| BR-EVAL-05 | The system must automatically calculate the overall average score for each evaluation based on all criteria scores. |
| BR-EVAL-06 | Evaluation criteria (`EvaluationCriteria`) are defined globally and reused across multiple evaluations. |
| BR-EVAL-07 | Supervisors can create and submit evaluations. HR Administrators can view all evaluations. |
| BR-EVAL-08 | A submitted evaluation cannot be deleted; it can only be corrected by creating a new evaluation record. |

---

## 7. Reports Module

| Rule ID | Rule Description |
|---------|-----------------|
| BR-REP-01 | The system must provide a report showing the average time (in days) from vacancy publication to the hiring date for closed vacancies. |
| BR-REP-02 | The system must provide a performance report showing the average evaluation score grouped by department. |
| BR-REP-03 | The system must provide a general employee report listing all employees with their current status (active/inactive). |
| BR-REP-04 | All reports must be exportable in PDF format. |
| BR-REP-05 | Report data must always reflect the current state of the database at the time of generation; caching is not required in this version. |

---

## 8. Data Integrity

| Rule ID | Rule Description |
|---------|-----------------|
| BR-INT-01 | Deleting a department that has associated positions is not allowed. |
| BR-INT-02 | Deleting a position that has associated employee history records is not allowed. |
| BR-INT-03 | Deleting a vacancy that has associated candidate applications is not allowed; it must be cancelled instead. |
| BR-INT-04 | Deleting an evaluation criterion that is used in at least one evaluation is not allowed. |
| BR-INT-05 | All entity IDs of type `string` use `ULID` (Universally Unique Lexicographically Sortable Identifier) format for sortable, collision-free generation. |
