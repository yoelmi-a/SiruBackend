# Software Requirements Specification (SRS)
## SIRUS — Smart Intelligent Resource Optimization System
**Version:** 1.0  
**Standard:** ISO/IEC 12207 — Software Life Cycle Processes  
**Date:** 2025

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Overall Description](#2-overall-description)
3. [Functional Requirements Summary](#3-functional-requirements-summary)
4. [Non-Functional Requirements Summary](#4-non-functional-requirements-summary)
5. [External Interface Requirements](#5-external-interface-requirements)
6. [Software Life Cycle Processes (ISO/IEC 12207)](#6-software-life-cycle-processes-isoiec-12207)
7. [Constraints & Assumptions](#7-constraints--assumptions)
8. [Document Approval](#8-document-approval)

---

## 1. Introduction

### 1.1 Purpose

This Software Requirements Specification (SRS) defines the functional and non-functional requirements for the **SIRUS** (Smart Intelligent Resource Optimization System) REST API platform. It is aligned with the ISO/IEC 12207 standard and serves as the authoritative reference for the design, development, testing, and maintenance of the system.

All stakeholders — project owner, development team, and QA team — must use this document as the baseline for all system-related decisions.

### 1.2 Scope

The platform is a **RESTful API** built with ASP.NET Core 10 following Onion Architecture. It provides the following capabilities:

**Recruitment module:**
- Create and manage job vacancies
- Register candidates and accept PDF CV uploads
- Compute a compatibility score between each CV and the vacancy using NLP (TF-IDF cosine similarity via Microsoft.ML)
- Rank candidates by compatibility score
- Manage candidate application statuses through a defined lifecycle

**Employee Management module:**
- Register, update, and deactivate employee records
- Manage departments and positions
- Track each employee's internal work history

**Performance Evaluation module:**
- Define reusable evaluation criteria
- Create and store structured performance evaluations per employee
- Auto-compute overall average score per evaluation
- Retrieve chronological evaluation history per employee

**Reports module:**
- Average hiring time report
- Performance by department report
- General employee status report
- PDF export of all reports

**Out of scope:**
- Authentication and login (out of scope for this version)
- Native mobile applications
- JWT and OAuth 2.0
- Real payment processing
- Email notification delivery (infrastructure stub only)

### 1.3 Definitions, Acronyms, and Abbreviations

| Term | Definition |
|------|-----------|
| SRS | Software Requirements Specification |
| API | Application Programming Interface |
| REST | Representational State Transfer |
| EF Core | Entity Framework Core — ORM for .NET |
| PostgreSQL | Open-source relational database; production database for this project |
| Mapster | .NET object mapper library used for entity-to-DTO conversion |
| ML.NET | Microsoft.ML — .NET-native machine learning library |
| TF-IDF | Term Frequency–Inverse Document Frequency — text vectorization technique |
| NLP | Natural Language Processing |
| ULID | Universally Unique Lexicographically Sortable Identifier |
| Result | Return-type pattern encapsulating success/failure without throwing exceptions |
| Onion Architecture | Layered architecture where inner layers define abstractions and outer layers implement them |
| DI | Dependency Injection |
| SOLID | Five OO design principles |
| KISS | Keep It Simple, Stupid |
| DRY | Don't Repeat Yourself |
| ISO/IEC 12207 | International standard for software life cycle processes |
| FR | Functional Requirement |
| NFR | Non-Functional Requirement |
| BR | Business Rule |
| OOS | Out of Scope |
| DTO | Data Transfer Object |
| CRUD | Create, Read, Update, Delete |

### 1.4 References

| Document | Description |
|----------|-------------|
| `business-rules.md` | Complete business rules catalogue |
| `functional-requirements.md` | Detailed functional requirements |
| `non-functional-requirements.md` | Detailed non-functional requirements |
| `use-cases.md` | Use case descriptions |
| `architecture.md` | System architecture definition |
| `code-guidelines.md` | Coding standards, naming conventions, and design principles |
| ISO/IEC 12207:2017 | Software and systems engineering — Software life cycle processes |
| OWASP Top 10 | Common web application security risks |
| ML.NET Docs | https://learn.microsoft.com/en-us/dotnet/machine-learning/ |

---

## 2. Overall Description

### 2.1 Product Perspective

SIRUS is a standalone REST API serving a frontend client (web or mobile). It follows a **5-project Onion Architecture** organized into Core, Infrastructure, Presentation, and Test layers:

| Project | Layer | Technology |
|---------|-------|-----------|
| `SIRU.Core.Domain` | Core | Pure C# — entities, enums, interfaces, OperationResult |
| `SIRU.Core.Application` | Core | Services, DTOs, Mapster mappings, generic service interface |
| `SIRU.Infrastructure.Persistence` | Infrastructure | EF Core 10 + PostgreSQL |
| `SIRU.Infrastructure.Ranking` | Infrastructure | Microsoft.ML (ML.NET) + PdfPig |
| `SIRU.Infrastructure.Shared` | Infrastructure | QuestPDF, MailKit, file storage |
| `SIRU.Infrastructure.Auth` | Infrastructure | Placeholder — out of scope |
| `SIRU.Presentation.Api` | Presentation | ASP.NET Core 10 Web API |
| `SIRU.Tests` | Test | xUnit + Moq + Testcontainers |

### 2.2 User Classes

| User Class | Description | Access Level |
|-----------|-------------|-------------|
| **HR Administrator** | Internal staff managing recruitment, employees, evaluations, and reports | Full API access |
| **Supervisor** | Internal staff creating and consulting performance evaluations | Evaluation endpoints |

> Authentication to distinguish these roles is out of scope for this version.

### 2.3 NLP Ranking Model

The CV ranking module uses **Microsoft.ML (ML.NET)** with a TF-IDF vectorization pipeline:

1. On CV upload, `PdfPig` extracts full text from the PDF.
2. The vacancy's profile and description are concatenated into a single reference text.
3. `Microsoft.ML` computes TF-IDF feature vectors for both texts.
4. Cosine similarity between the two vectors produces a score in [0.0, 1.0].
5. The score is stored in `CandidateToVacancy.Score` asynchronously.
6. Future upgrade path: swap in `Microsoft.ML.TorchSharp` NAS-BERT sentence similarity for semantic matching, without changing the `IRankingService` interface.

### 2.4 Data Persistence

- **PostgreSQL** via `Npgsql.EntityFrameworkCore.PostgreSQL` is the production database.
- EF Core migrations are the sole mechanism for schema changes.
- String-keyed entities use ULID for globally unique, sortable identifiers.
- Int-keyed entities (`Department`, `Position`, `EvaluationCriterion`) use database-generated identity columns.

### 2.5 Object Mapping

All entity ↔ DTO transformations are handled exclusively by **Mapster**. A centralized `MappingConfig` class registers all type adapter configurations at application startup via `TypeAdapterConfig`.

### 2.6 Operating Environment

- **Runtime:** .NET 10 on Linux / macOS / Windows host.
- **Database:** PostgreSQL 16+ (external server or Docker container).
- **CV Storage:** Local file system path (configurable).
- **Platform:** REST API only; no server-rendered UI.

### 2.7 Design and Implementation Constraints

- Backend must be implemented in **C# / .NET 10**.
- Data access must use **Entity Framework Core 10** with the **PostgreSQL** provider.
- Object mapping must use **Mapster**.
- NLP ranking must use **Microsoft.ML**.
- Unit tests must use **xUnit**.
- Authentication is **out of scope**.
- Every method must have at least one success and one failure test.
- Service and repository methods must return `Result<T>` or `Result` for expected failure paths.
- No credentials or secrets may be committed to source control.
- All source code identifiers, XML doc comments, and inline comments must be written in **English**.

---

## 3. Functional Requirements Summary

Full descriptions are in `functional-requirements.md`.

| Module | Key Requirements |
|--------|----------------|
| Vacancies | CRUD, state machine (Open/Closed/Cancelled), filter by state |
| Candidates & Applications | Register with PDF CV, apply to vacancies, status lifecycle |
| CV Ranking (NLP) | TF-IDF + cosine similarity via ML.NET, async background scoring, manual recalculation |
| Employee Management | CRUD with `Cedula` uniqueness, deactivation, paginated listing |
| Positions & Departments | CRUD, employee assignment, work history, referential integrity |
| Performance Evaluation | Criteria CRUD, evaluation creation with auto-average, chronological history |
| Reports | Hiring time, performance by department, employee status, PDF export |
| Result Pattern | All service/repository methods return `Result<T>` or `Result` for expected failures |
| Generic Service + Mapster | `IGenericService<T>` contract, Mapster for all mappings |

---

## 4. Non-Functional Requirements Summary

Full descriptions are in `non-functional-requirements.md`.

| Category | Key Constraint |
|----------|---------------|
| Performance | API response < 500 ms; queries < 300 ms; NLP async, < 10 s per pair |
| Security | No secrets in code; HTTPS in production; file validation; no auth (OOS) |
| Maintainability | SOLID/KISS/DRY; XML docs; 80% test coverage; Result; EF migrations only |
| Scalability | Stateless API; interface-driven ranking; DB-agnostic repository layer |
| Compatibility | .NET 10; PostgreSQL; JSON responses; OpenAPI / Swagger |
| Usability (API) | Consistent error envelope; pagination on all lists; meaningful HTTP status codes |
| Legal | Educational project; ISO/IEC 12207 lifecycle alignment |

---

## 5. External Interface Requirements

### 5.1 User Interfaces

- The system exposes a REST API consumed by any HTTP client.
- OpenAPI specification is accessible at `/swagger` in non-production environments.
- All API responses use JSON with a consistent envelope format.

### 5.2 Hardware Interfaces

Not applicable. The system runs on any .NET 10-compatible host with access to a PostgreSQL instance.

### 5.3 Software Interfaces

| External System | Purpose | Notes |
|----------------|---------|-------|
| PostgreSQL | Relational data persistence | Via `Npgsql.EntityFrameworkCore.PostgreSQL` |
| Microsoft.ML (ML.NET) | TF-IDF text vectorization and cosine similarity | In-process; no external service |
| PdfPig | PDF text extraction | In-process; no external service |
| QuestPDF | PDF report generation | In-process; no external service |
| MailKit | Email notifications | Stub only in this version |

### 5.4 Communication Interfaces

- All communication between clients and the API uses standard **HTTP/HTTPS**.
- No external webhooks, message queues, or third-party integrations are used in this version.

---

## 6. Software Life Cycle Processes (ISO/IEC 12207)

### 6.1 Primary Processes

| Process | Implementation |
|---------|---------------|
| **Acquisition** | Developed as an educational solution for internal SME use. |
| **Supply** | Development team delivers incremental releases reviewed by stakeholders before final delivery. |
| **Development** | Agile methodology (Scrum); sprints aligned with the user stories in this SRS. |
| **Operation** | API runs locally or on a .NET 10-compatible host connected to a PostgreSQL database. |
| **Maintenance** | Corrective and evolutionary maintenance following each release cycle. |

### 6.2 Supporting Processes

| Process | Implementation |
|---------|---------------|
| **Documentation** | All deliverables include technical documentation in Markdown. Source code documented via XML comments in English. |
| **Configuration Management** | Version control via Git (GitFlow: `main`, `develop`, `feature/*`, `fix/*`). |
| **Quality Assurance** | xUnit tests for every method (success + failure); code reviews on all pull requests. |
| **Verification & Validation** | Acceptance testing by the project owner against requirements in this SRS. |
| **Problem Resolution** | Issues tracked in GitHub Issues or equivalent project management tool. |

### 6.3 Organizational Processes

| Process | Implementation |
|---------|---------------|
| **Project Management** | Sprint schedule with defined delivery milestones mapped to user story groups. |
| **Process Improvement** | Sprint retrospectives to identify and implement continuous improvements. |

---

## 7. Constraints & Assumptions

### 7.1 Constraints

- The system must be built with **.NET 10** and **C#**.
- Data access must use **Entity Framework Core 10** with the **PostgreSQL** provider.
- Object mapping must use **Mapster**.
- NLP ranking must use **Microsoft.ML (ML.NET)**.
- Unit tests must use **xUnit**.
- Every method must have at least one success test and one failure test.
- Service and repository methods must return `OperationResult<T>` or `Result` for expected failure paths.
- **Authentication is out of scope.**
- Source code identifiers and XML documentation must be written in **English**.
- No credentials or secrets may be committed to source control.

### 7.2 Assumptions

- A PostgreSQL 16+ database server will be available during development (Docker recommended).
- The development team has working knowledge of ASP.NET Core, EF Core, and xUnit.
- CV files will be provided in valid PDF format by API consumers.
- The frontend client is developed independently and consumes this API.

---

## 8. User Stories Reference

| ID | Story | Module |
|----|-------|--------|
| HU-01 | Create vacancy | Recruitment |
| HU-02 | Upload CVs | Recruitment |
| HU-03 | Extract CV information | Recruitment (NLP) |
| HU-04 | Calculate compatibility score | Recruitment (NLP) |
| HU-05 | View candidate ranking | Recruitment |
| HU-06 | Register employee | Employee Management |
| HU-07 | Edit employee information | Employee Management |
| HU-08 | List employees | Employee Management |
| HU-09 | View internal work history | Employee Management |
| HU-10 | Create evaluation criteria | Performance Evaluation |
| HU-11 | Register evaluation | Performance Evaluation |
| HU-12 | View evaluation history | Performance Evaluation |
| HU-13 | Generate automatic result | Performance Evaluation |
| HU-14 | Average hiring time report | Reports |
| HU-15 | Performance by department report | Reports |
| HU-16 | General employee report | Reports |
| HU-17 | Export reports as PDF | Reports |
| HU-18 | Login system | OOS |
| HU-19 | Database setup | Infrastructure |
| HU-20 | Field validation | Cross-cutting |

---

## 9. Document Approval

This document must be reviewed and approved by the project owner prior to the start of the design and development phase. It serves as the baseline for auditing system conformance with ISO/IEC 12207.

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Project Owner | | | |
| Lead Developer | | | |
| QA Lead | | | |
