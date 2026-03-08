# SIRU FrontEnd - Contexto de Desarrollo para IA

Este documento proporciona el contexto técnico necesario para que un modelo de lenguaje (LLM) pueda desarrollar, estructurar e integrar el FrontEnd del sistema **SIRU** (Sistema de Información de Recursos Humanos).

## 🎯 Visión General del Proyecto
SIRU es una plataforma de gestión de talento que automatiza el reclutamiento mediante análisis inteligente de CVs y evaluación de desempeño. 

**Arquitectura del Backend:** Clean Architecture (C# .NET 10).
**Base de Datos:** PostgreSQL.
**Documentación API:** Swagger interactivo disponible en la raíz del backend (`/`).

---

## 🏗️ Guía de Integración de la API

### 1. Estándar de Comunicación
- **Base URL:** Se define en el entorno (ej: `https://localhost:7001/api`).
- **Formato:** JSON (`application/json`).
- **Seguridad:** JWT Bearer Token (pendiente de implementación en el backend).

### 2. Manejo de Respuestas (Patrón Result)
El backend utiliza un objeto `Result` interno, pero la API expone los datos de la siguiente manera:
- **Éxito (200 OK / 201 Created):** Devuelve directamente el objeto o la lista de objetos solicitados.
- **Error (400 BadRequest / 404 NotFound):** Devuelve un objeto con la lista de errores encontrados:
  ```json
  {
    "errors": ["Mensaje de error 1", "Mensaje de error 2"]
  }
  ```

### 3. Contratos de Datos (Módulo de Vacantes)

#### `VacantDto` (Lectura)
```typescript
interface VacantDto {
  id: string;
  title: string;
  description: string;
  profile: string;
  publicationDate: string; // ISO Date
  hiringDate?: string;      // ISO Date (Opcional)
  status: number;          // Enum (1: Abierta, 2: Cerrada, etc.)
}
```

#### `SaveVacantDto` (Escritura - POST/PUT)
```typescript
interface SaveVacantDto {
  id?: string;             // Solo para actualizaciones
  title: string;
  description: string;
  profile: string;
  status: number;
}
```

---

## 🛠️ Reglas de Estructuración del FrontEnd

Para mantener la consistencia con el Backend (Clean Architecture), se recomienda la siguiente estructura en el FrontEnd (preferiblemente React o Angular con TypeScript):

1. **Capa de Servicios (API Client):** Centralizar todas las peticiones a la API en servicios dedicados por entidad (ej: `vacantService.ts`).
2. **Capa de Componentes:** 
   - **Contenedores/Páginas:** Gestionan el estado y la lógica de negocio.
   - **Componentes UI:** Presentacionales y reutilizables (Atómicos).
3. **Manejo de Estado:** Utilizar Context API, Redux o Signals según la complejidad.
4. **Validaciones:** Duplicar validaciones básicas en el cliente para mejorar la UX antes de enviar datos al servidor.

---

## 📋 Endpoints Disponibles (v1)

| Método | Endpoint | Descripción |
| :--- | :--- | :--- |
| GET | `/api/Vacants` | Obtener todas las vacantes. |
| GET | `/api/Vacants/{id}` | Obtener detalle de una vacante. |
| POST | `/api/Vacants` | Crear una nueva vacante. |
| PUT | `/api/Vacants/{id}` | Actualizar una vacante existente. |
| DELETE | `/api/Vacants/{id}` | Eliminar una vacante. |

---

## 💡 Instrucciones para el Modelo de IA
- **Tipado:** Generar siempre interfaces TypeScript que coincidan exactamente con los DTOs del backend.
- **Async/Await:** Utilizar patrones asíncronos limpios para las llamadas a la API.
- **Feedback:** Implementar estados de carga (`loading`) y manejo de errores visuales basados en el array `errors` que devuelve la API.
- **Estética:** Priorizar una interfaz limpia, profesional y orientada a productividad (Dashboard de RRHH).
