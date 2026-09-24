# Contrato REST: MVP SIGE (Fase 1)

Base path sugerido: `/api/v1`. Todos los endpoints (salvo login) requieren JWT en
`Authorization: Bearer <token>`. Los marcados **[Supervisor]** exigen ese rol.

## Autenticación

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/auth/login` | Recibe usuario/contraseña, devuelve JWT + rol. |

## Emergencias

| Método | Ruta | Descripción | Requisitos |
|---|---|---|---|
| POST | `/emergencias` | Crea una emergencia (estado inicial `reportada`). | FR-001, FR-002, FR-015 |
| GET | `/emergencias` | Lista emergencias, filtrable por estado/prioridad (para mapa y dashboard). | FR-003, FR-011 |
| GET | `/emergencias/{id}` | Detalle: datos, unidades asignadas, timeline. | US1, US4 |
| PATCH | `/emergencias/{id}/estado` | Cambia el estado (validada, despachada, etc.) de la emergencia o de una asignación específica. | FR-009, FR-016 |
| POST | `/emergencias/{id}/cerrar` | **[Supervisor]** Cierra la emergencia y libera automáticamente sus unidades asignadas (pasan a Disponible, salvo Fuera de Servicio). | FR-017, FR-018 |
| POST | `/emergencias/{id}/reabrir` | **[Supervisor]** Reabre una emergencia cerrada. | FR-017 |
| GET | `/emergencias/{id}/timeline` | Devuelve los `EventoAuditoria` asociados, ordenados por fecha. | FR-008, FR-010 |

## Unidades

| Método | Ruta | Descripción | Requisitos |
|---|---|---|---|
| POST | `/unidades` | **[Supervisor]** Crea una unidad de respuesta. | FR-004 |
| GET | `/unidades` | Lista unidades con tipo y estado operativo (para mapa y asignación). | FR-004, FR-005 |
| PATCH | `/unidades/{id}/estado` | **[Supervisor]** Cambia el estado operativo (ej. a Fuera de Servicio). | FR-004 |

## Asignaciones

| Método | Ruta | Descripción | Requisitos |
|---|---|---|---|
| POST | `/emergencias/{id}/asignaciones` | Asigna una unidad disponible a la emergencia; revalida disponibilidad en la misma transacción. | FR-006, FR-007 |
| PATCH | `/asignaciones/{id}/estado` | Avanza el estado de esa asignación específica (despachada → en ruta → en el lugar → atendida). | FR-016 |

**Error de negocio esperado** en `POST /emergencias/{id}/asignaciones` cuando la unidad
ya no está disponible: `409 Conflict` con cuerpo
`{ "codigo": "UNIDAD_NO_DISPONIBLE", "mensaje": "..." }` — el frontend lo traduce en el
mensaje al Operador (Edge Case de concurrencia en `spec.md`).

## Dashboard

| Método | Ruta | Descripción | Requisitos |
|---|---|---|---|
| GET | `/dashboard/indicadores` | Conteo de emergencias activas por estado/prioridad y unidades disponibles vs. ocupadas. | FR-011 |

## Eventos SignalR (hub `/hubs/operaciones`)

| Evento | Payload (resumen) | Se emite cuando |
|---|---|---|
| `EmergenciaActualizada` | `{ emergenciaId, estado }` | Se crea una emergencia o cambia su estado/asignaciones. |
| `UnidadActualizada` | `{ unidadId, estadoOperativo }` | Cambia la disponibilidad de una unidad. |

Estos dos eventos son los que permiten cumplir SC-002 y SC-006 (mapa y dashboard
actualizados sin recarga manual) — ver `research.md` §2.
