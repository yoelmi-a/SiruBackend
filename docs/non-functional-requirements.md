# Non-Functional Requirements — SIRUS (Smart Intelligent Resource Optimization System)

## 1. Introduction

This document describes the non-functional requirements (NFRs) for the SIRUS platform. These requirements define quality, maintainability, and operational constraints that apply across all layers of the architecture.

> **Scope note:** Authentication and login are out of scope for this version. All NFRs relate to the REST API backend built on ASP.NET Core 10 with a PostgreSQL database.

---

## 2. Requirement Conventions

- **ID format:** `NFR-<CATEGORY>-<NNN>`
- **Priority:** High | Medium | Low
- **Categories:** PERF (Performance), SEC (Security), MAINT (Maintainability), SCALE (Scalability), COMPAT (Compatibility), USAB (Usability), LEGAL (Legal & Compliance)

---

## 3. Performance

| ID | Description | Priority |
|----|-------------|----------|
| NFR-PERF-001 | The API shall respond to standard CRUD requests in under **500 ms** under normal load conditions. | High |
| NFR-PERF-002 | Database queries for listing and filtering operations shall complete in under **300 ms**. | High |
| NFR-PERF-003 | The NLP ranking computation shall complete within **10 seconds** per CV-vacancy pair and shall not block any API response. | High |
| NFR-PERF-004 | The API shall support at least **50 concurrent requests** without noticeable degradation. | Medium |
| NFR-PERF-005 | PDF generation for reports shall complete in under **5 seconds** for typical report sizes. | Medium |

---

## 4. Security

| ID | Description | Priority |
|----|-------------|----------|
| NFR-SEC-001 | The application shall not expose sensitive configuration values (connection strings, API keys) in source code; they shall be read from environment variables or `appsettings.json` / user secrets. | High |
| NFR-SEC-002 | The API shall use **HTTPS** in production deployments. | High |
| NFR-SEC-003 | All input data received through API endpoints shall be validated and sanitized before processing to prevent injection attacks. | High |
| NFR-SEC-004 | Authentication and session management are **out of scope** for this version. | High |
| NFR-SEC-005 | Uploaded PDF files shall be validated for MIME type and maximum file size (configurable, default 10 MB) before being accepted. | High |
| NFR-SEC-006 | CV files shall be stored in a dedicated, non-publicly routable file storage path on the server; direct URL access to raw files shall not be exposed. | Medium |

---

## 5. Maintainability

| ID | Description | Priority |
|----|-------------|----------|
| NFR-MAINT-001 | The source code shall follow the **SOLID**, **KISS**, and **DRY** principles as defined in `code-guidelines.md`. | High |
| NFR-MAINT-002 | Every public and internal method shall have **XML documentation comments** (`<summary>`, `<param>`, `<returns>`). | High |
| NFR-MAINT-003 | **Every method** in the Application and Infrastructure layers shall have corresponding unit tests covering at least one **success scenario** and one **failure scenario**. | High |
| NFR-MAINT-004 | Unit tests shall use the **EF Core InMemory provider** or **Testcontainers (PostgreSQL)** for integration tests; no production database shall be targeted in automated tests. | High |
| NFR-MAINT-005 | All service, application, and repository methods that can fail for expected business reasons shall return an `Result<T>` instead of throwing exceptions. | High |
| NFR-MAINT-006 | Database schema changes shall be managed exclusively via **EF Core migrations**; manual SQL scripts in any environment are forbidden. | High |
| NFR-MAINT-007 | Version control shall use **Git** with a GitFlow-aligned branching strategy: `main`, `develop`, `feature/*`, `fix/*`. | High |
| NFR-MAINT-008 | A CI pipeline (e.g., GitHub Actions) shall build the project and run all tests automatically on every pull request targeting `develop` or `main`. | Medium |
| NFR-MAINT-009 | The minimum unit-test coverage target for the Application and Infrastructure layers is **80%** of all methods. | High |
| NFR-MAINT-010 | All entity-to-DTO mappings shall use **Mapster**; no manual property mapping is permitted in service or controller code. | High |

---

## 6. Scalability

| ID | Description | Priority |
|----|-------------|----------|
| NFR-SCALE-001 | The application architecture shall allow future replacement of the NLP ranking implementation without changes to the domain or application layers (interface-driven design). | High |
| NFR-SCALE-002 | The REST API shall be stateless (no in-process session state), enabling future horizontal scaling behind a load balancer. | High |
| NFR-SCALE-003 | The repository and service abstractions shall be database-agnostic at the interface level, supporting future migration to a different relational or NoSQL backend. | Medium |

---

## 7. Compatibility

| ID | Description | Priority |
|----|-------------|----------|
| NFR-COMPAT-001 | The backend shall target **.NET 10** and be compatible with Linux, macOS, and Windows hosting environments. | High |
| NFR-COMPAT-002 | The API shall follow **RESTful conventions** and produce **JSON** responses with consistent envelope format. | High |
| NFR-COMPAT-003 | The API shall expose an **OpenAPI / Swagger** specification at `/swagger` in non-production environments. | High |
| NFR-COMPAT-004 | The database provider shall be **PostgreSQL** via the `Npgsql.EntityFrameworkCore.PostgreSQL` EF Core provider. | High |
| NFR-COMPAT-005 | The platform is **API-only**; no server-rendered UI is in scope. | High |

---

## 8. Usability (API Design)

| ID | Description | Priority |
|----|-------------|----------|
| NFR-USAB-001 | All API error responses shall include a consistent error envelope with at minimum: `statusCode`, `message`, and `errors` fields. | High |
| NFR-USAB-002 | All list endpoints shall support **pagination** via `page` and `pageSize` query parameters with sensible defaults (default page size: 50). | High |
| NFR-USAB-003 | The API shall return meaningful HTTP status codes: `200 OK`, `201 Created`, `204 No Content`, `400 Bad Request`, `404 Not Found`, `409 Conflict`, `422 Unprocessable Entity`, `500 Internal Server Error`. | High |
| NFR-USAB-004 | The API documentation (Swagger) shall include descriptions for all endpoints, request parameters, and response schemas. | Medium |

---

## 9. Legal & Compliance

| ID | Description | Priority |
|----|-------------|----------|
| NFR-LEGAL-001 | The platform is intended for **educational and demonstration purposes**; it shall not process real financial transaction data. | High |
| NFR-LEGAL-002 | CV files and personal data shall be handled according to data minimization principles; no data shall be retained longer than necessary for the academic lifecycle of the project. | Medium |
| NFR-LEGAL-003 | The software development lifecycle shall be aligned with **ISO/IEC 12207** processes as documented in `srs.md`. | Medium |
