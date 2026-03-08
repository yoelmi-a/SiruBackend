# SIRU (Sistema de Información de Recursos Humanos) - Plan de Trabajo

Este proyecto es un sistema de gestión de recursos humanos y reclutamiento (SIRU), desarrollado bajo una **Arquitectura Limpia (Clean Architecture)** utilizando **.NET 10.0** y **PostgreSQL**.

## 🎯 Objetivo del Proyecto
Implementar un sistema integral que automatice el ciclo de vida del talento humano: desde el reclutamiento inteligente hasta la evaluación del desempeño y generación de reportes estratégicos.

---

## 🏗️ Detalles Técnicos Implementados

### Persistencia y Acceso a Datos (PostgreSQL)
- **DbContext:** `ApplicationDbContext.cs` configurado con EF Core y PostgreSQL.
- **Fluent API:** Configuraciones de entidades desacopladas en `SIRU.Infrastructure.Persistence\EntitiesConfiguration`.
- **Patrón Repositorio:** 
    *   **Interfaces:** Definidas en `SIRU.Core.Domain\Interfaces`.
    *   **Implementación:** `GenericRepository<T>` en `SIRU.Infrastructure.Persistence\Repositories`.
    *   **Nota:** Se prescinde del patrón Unit of Work; los repositorios gestionan la persistencia directamente.

### Capa de Aplicación y Lógica de Negocio
- **Patrón Result:** Implementado para el manejo de errores y estados de operación de forma semántica, utilizando `Result` y `Result<T>` en el Dominio.
- **Servicios:** Organizados por entidad en `SIRU.Core.Application\Services\{Entity}\`.
- **AutoMapper:** Configurado y registrado para el mapeo entre entidades y DTOs, organizados por entidad.
- **Pruebas Unitarias:** Implementadas con **xUnit** y **Moq** para validar la lógica de servicios. El módulo de Vacantes cuenta con cobertura completa (7 tests exitosos).

### API y Documentación
- **Controladores:** Implementado `VacantsController` con soporte completo de CRUD.
- **Swagger/OpenAPI:** Configurado a través de `ServiceRegistration` en la capa de API. Interfaz interactiva disponible en la raíz (`/`) del entorno de desarrollo.
- **IoC Modular:** Inyección de dependencias desacoplada por capas mediante métodos de extensión `Add{Layer}Layer`.

---

## 📋 Historias de Usuario (Backlog)
(Consultar versiones anteriores para el detalle completo)
- **Módulo 1:** Reclutamiento Inteligente (HU-01 a HU-05).
- **Módulo 2:** Gestión de Empleados (HU-06 a HU-09).
- **Módulo 3:** Evaluación de Desempeño (HU-10 a HU-13).
- **Módulo 4:** Reportes y Analítica (HU-14 a HU-17).
- **Técnicas:** Login (HU-18), Base de Datos (HU-19), Validaciones (HU-20).

---

## 🚀 Plan de Ejecución Actualizado

### Fase 1: Infraestructura (Completada - 100%) ✅
- [x] Configurar **EF Core** y PostgreSQL.
- [x] Mapear entidades con **Fluent API**.
- [x] Implementar **IoC Modular** (ServiceRegistration).
- [x] Implementar **Repositorio Genérico** en el Dominio.
- [ ] Generar migración inicial (Pendiente por el usuario).

### Fase 2: Módulo de Reclutamiento (En Proceso - 40%) 🏗️
- [x] Configurar **AutoMapper** y DTOs para Vacantes.
- [x] Implementar **`IVacantService`** y **`VacantService`** con Patrón Result (HU-01).
- [x] Validar lógica con **Pruebas Unitarias** (7 tests ✅).
- [x] Implementar **`VacantsController`** y documentar con **Swagger**.
- [ ] **HU-02: Carga de currículums** (Manejo de archivos).
- [ ] **HU-03: Extracción de información** (Parsing de CVs).
- [ ] **HU-04/05: Algoritmo de Matching** y Ranking de candidatos.

### Fase 3: Gestión de Empleados y Evaluaciones
- [ ] Implementar lógica de transición de Candidato a Empleado.
- [ ] Definición de criterios y registro de evaluaciones (HU-10/11).
...
