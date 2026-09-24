# Contrato REST: MVP SIGE (Fase 1)

**Prefijo real (actualizado en Implement)**: `/api/{NombreDeClase}` — convención de
`IEndpointGroup` de la plantilla Jason Taylor (Minimal API, no Controllers MVC), no
`/api/v1/...` como se planteó originalmente. Ej.: la clase `Emergencias` →
`/api/Emergencias`. Todos los endpoints (salvo login) requieren un access token de
ASP.NET Core Identity en `Authorization: Bearer <token>` (no un JWT hecho a mano — ver
`research.md` §3). Los marcados **[Supervisor]** exigen ese rol vía `[Authorize(Roles = "Supervisor")]`
en el Command de MediatR (Application), no en el endpoint.

## Autenticación

| Método | Ruta real | Descripción |
|---|---|---|
| POST | `/api/Users/login` | Recibe `{ email, password }`, devuelve `{ accessToken, refreshToken, expiresIn }` (Identity, `MapIdentityApi`). **Verificado funcionando.** |

## Emergencias (`/api/Emergencias`) — implementadas en US1

| Método | Ruta real | Descripción | Requisitos | Estado |
|---|---|---|---|---|
| POST | `/api/Emergencias` | Crea una emergencia (estado inicial `Reportada`); genera `EventoAuditoria` "EmergenciaCreada" y emite `EmergenciaActualizada`. | FR-001, FR-002, FR-010, FR-015 | ✅ Verificado |
| POST | `/api/Emergencias/{id}/validar` | Transición `Reportada` → `Validada`. Genera `EventoAuditoria` "EmergenciaValidada". | FR-009, FR-010 | ✅ Verificado |
| GET | `/api/Emergencias` | Lista todas las emergencias (sin filtro todavía). | FR-003, FR-011 | ✅ Verificado |
| GET | `/api/Emergencias/{id}` | Detalle con `asignaciones` y `timeline` completos. | US1, US4 | ✅ Verificado |
| POST | `/api/Emergencias/{id}/cerrar` | **[Supervisor]** Requiere estado "Atendida"; cierra y libera las unidades asignadas. | FR-017, FR-018, FR-010 | ✅ Verificado |
| POST | `/api/Emergencias/{id}/reabrir` | **[Supervisor]** Requiere estado "Cerrada"; vuelve a "Atendida". | FR-017, FR-010 | ✅ Verificado |

## Unidades (`/api/Unidades`) — implementadas en US2

| Método | Ruta real | Descripción | Requisitos | Estado |
|---|---|---|---|---|
| POST | `/api/Unidades` | **[Supervisor]** Crea una unidad de respuesta. | FR-004 | ✅ Verificado |
| GET | `/api/Unidades` | Lista unidades con tipo y estado operativo. | FR-004, FR-005 | ✅ Verificado |
| PATCH | `/api/Unidades/{id}/estado` | **[Supervisor]** Cambia el estado operativo; emite `UnidadActualizada`. | FR-004 | ✅ Verificado |
| GET | `/api/Users/me` | Rol del usuario autenticado (`{ id, roles }`) — agregado porque el access token de Identity es opaco y el frontend no puede leerlo. | FR-012 | ✅ Verificado |

## Asignaciones — implementadas en US3

| Método | Ruta real | Descripción | Requisitos | Estado |
|---|---|---|---|---|
| POST | `/api/Emergencias/{id}/asignaciones` | Body `{ unidadId }`. Asigna una unidad disponible (requiere `Validada`, no `Cerrada`); revalida disponibilidad con un `UPDATE` condicional atómico (no una transacción explícita — ver `research.md` §4 y `tasks.md` T038). Genera `EventoAuditoria` "UnidadAsignada"; primera asignación pasa la emergencia a "Despachada". | FR-006, FR-007, FR-009, FR-010 | ✅ Verificado |
| PATCH | `/api/Asignaciones/{id}/estado` | Avanza el estado de una asignación (solo hacia adelante); recalcula el estado de la Emergencia (FR-016). | FR-016 | ✅ Verificado |

**Errores de negocio verificados** (`409 Conflict`, cuerpo `{ codigo, mensaje }` —
implementado con `ConflictException` + `ProblemDetailsExceptionHandler`, T014):
- `EMERGENCIA_NO_VALIDADA`: la emergencia sigue en "Reportada".
- `EMERGENCIA_CERRADA`: la emergencia ya está cerrada.
- `UNIDAD_NO_DISPONIBLE`: la unidad ya fue asignada por otro operador (Edge Case de
  concurrencia en `spec.md`) — el frontend muestra `mensaje` directamente al Operador.
- `TRANSICION_INVALIDA`: se intentó retroceder o repetir el estado de una asignación.
- `EMERGENCIA_NO_ATENDIDA`: se intentó cerrar una emergencia que no está "Atendida".
- `EMERGENCIA_NO_CERRADA`: se intentó reabrir una emergencia que no está "Cerrada".

## Dashboard (`/api/Dashboard`) — pendiente (US5)

| Método | Ruta prevista | Descripción | Requisitos |
|---|---|---|---|
| GET | `/api/Dashboard/indicadores` | Conteo de emergencias activas por estado/prioridad y unidades disponibles vs. ocupadas. | FR-011 |

## Eventos SignalR (hub `/hubs/operaciones`)

| Evento | Payload (resumen) | Se emite cuando |
|---|---|---|
| `EmergenciaActualizada` | `{ emergenciaId, estado }` | Se crea una emergencia o cambia su estado/asignaciones. |
| `UnidadActualizada` | `{ unidadId, estadoOperativo }` | Cambia la disponibilidad de una unidad. |

Estos dos eventos son los que permiten cumplir SC-002 y SC-006 (mapa y dashboard
actualizados sin recarga manual) — ver `research.md` §2.
