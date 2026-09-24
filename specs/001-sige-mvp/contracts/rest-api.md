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
| POST | `/api/Emergencias/{id}/cerrar` | **[Supervisor]** Cierra y libera unidades. | FR-017, FR-018, FR-010 | ⏳ Pendiente (US4) |
| POST | `/api/Emergencias/{id}/reabrir` | **[Supervisor]** Reabre una emergencia cerrada. | FR-017, FR-010 | ⏳ Pendiente (US4) |
| PATCH | `/api/Asignaciones/{id}/estado` | Avanza el estado de una asignación (despachada→…→atendida). | FR-016 | ⏳ Pendiente (US4) |

## Unidades (`/api/Unidades`) — pendiente (US2)

| Método | Ruta prevista | Descripción | Requisitos |
|---|---|---|---|
| POST | `/api/Unidades` | **[Supervisor]** Crea una unidad de respuesta. | FR-004 |
| GET | `/api/Unidades` | Lista unidades con tipo y estado operativo. | FR-004, FR-005 |
| PATCH | `/api/Unidades/{id}/estado` | **[Supervisor]** Cambia el estado operativo. | FR-004 |

## Asignaciones (`/api/Asignaciones`) — pendiente (US3)

| Método | Ruta prevista | Descripción | Requisitos |
|---|---|---|---|
| POST | `/api/Emergencias/{id}/asignaciones` | Asigna una unidad disponible (requiere `Validada`); revalida disponibilidad en la misma transacción. Genera `EventoAuditoria` "UnidadAsignada". | FR-006, FR-007, FR-009, FR-010 |

**Error de negocio esperado** en `POST /emergencias/{id}/asignaciones` cuando la unidad
ya no está disponible: `409 Conflict` con cuerpo
`{ "codigo": "UNIDAD_NO_DISPONIBLE", "mensaje": "..." }` — el frontend lo traduce en el
mensaje al Operador (Edge Case de concurrencia en `spec.md`).

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
