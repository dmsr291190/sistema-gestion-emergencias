# Data Model: MVP SIGE (Fase 1)

Extraído de `spec.md` (Key Entities) y de las decisiones de `research.md`.

## Emergencia

| Campo | Tipo | Notas |
|---|---|---|
| Id | Guid | PK |
| Tipo | string | Ej. médica, incendio, seguridad (catálogo simple) |
| Descripcion | string | Obligatorio (FR-001, FR-015) |
| Latitud / Longitud | double | Obligatorios; bloquear guardado si faltan (Assumptions) |
| Prioridad | enum (Baja/Media/Alta/Crítica) | Obligatorio |
| FechaHoraReporte | datetime (UTC) | Se asigna al crear |
| ReportanteNombre | string | Obligatorio (FR-019) |
| ReportanteContacto | string (nullable) | Opcional (FR-019) |
| Estado | enum | reportada → validada → despachada → en ruta → en el lugar → atendida → cerrada (FR-009) — ver regla de derivación en FR-016 cuando hay múltiples unidades |
| CreadoPor | FK Usuario | Operador que la registró |

**Reglas de estado**:
- Estado inicial siempre `reportada` (FR-002).
- Con una sola unidad asignada, el estado de la Emergencia sigue el avance de esa
  Asignación.
- Con varias unidades, cada `Asignacion` progresa su propio `EstadoAsignacion`; el
  campo `Estado` de la Emergencia pasa a `atendida` solo cuando **todas** las
  asignaciones activas están en `atendida` (FR-016).
- `cerrada` y la reapertura (`cerrada` → estado anterior) solo las puede ejecutar un
  Usuario con rol `Supervisor` (FR-017), sin límite de reaperturas; cada una genera su
  propio `EventoAuditoria`.
- Al cerrar, todas las `UnidadRespuesta` con una `Asignacion` activa hacia esa
  Emergencia MUST volver a `EstadoOperativo = Disponible`, salvo que ya estén
  `FueraDeServicio` (FR-018). Al reabrir, esas unidades NO se reasignan
  automáticamente.
- Nunca se elimina físicamente (Principio IV constitution; FR-013).

## UnidadRespuesta

| Campo | Tipo | Notas |
|---|---|---|
| Id | Guid | PK |
| Tipo | enum (Ambulancia/Bomberos/Patrullero) | FR-004 |
| Identificador | string | Código visible de la unidad (ej. "AMB-03") |
| EstadoOperativo | enum (Disponible/Ocupada/FueraDeServicio) | FR-004, FR-005 |
| Latitud / Longitud | double | Para mostrarla en el mapa junto a las emergencias |
| RowVersion | byte[] / token de concurrencia | Soporta la revalidación optimista de FR-007 |

## Asignacion

| Campo | Tipo | Notas |
|---|---|---|
| Id | Guid | PK |
| EmergenciaId | FK Emergencia | |
| UnidadId | FK UnidadRespuesta | |
| EstadoAsignacion | enum | despachada → en ruta → en el lugar → atendida (progreso independiente por unidad, FR-016) |
| AsignadoPor | FK Usuario | Quién confirmó la asignación (FR-010) |
| FechaHoraAsignacion | datetime (UTC) | |

**Regla de unicidad/disponibilidad**: al confirmar una `Asignacion`, el backend
revalida en la misma transacción que `UnidadRespuesta.EstadoOperativo == Disponible`;
si no, rechaza con error de negocio (FR-007) — ver `research.md` §4.

## Usuario

| Campo | Tipo | Notas |
|---|---|---|
| Id | Guid | PK |
| NombreUsuario | string | Login |
| PasswordHash | string | Nunca en texto plano |
| Rol | enum (Operador/Supervisor) | FR-012, FR-017 |

## EventoAuditoria (Timeline)

| Campo | Tipo | Notas |
|---|---|---|
| Id | Guid | PK |
| EmergenciaId | FK Emergencia | Nullable si el evento es sobre una Unidad sin emergencia asociada |
| UnidadId | FK UnidadRespuesta (nullable) | |
| TipoEvento | string | Ej. "EmergenciaCreada", "UnidadAsignada", "CambioEstado", "EmergenciaCerrada", "EmergenciaReabierta" |
| EstadoAnterior / EstadoNuevo | string (nullable) | Para eventos de cambio de estado |
| UsuarioId | FK Usuario | Quién ejecutó el cambio (FR-010) |
| FechaHora | datetime (UTC) | |

Todo cambio crítico (FR-010) — creación, asignación, cambio de estado, cierre,
reapertura — MUST generar un `EventoAuditoria`. Esta tabla es la fuente de la línea de
tiempo mostrada en el detalle de la emergencia (User Story 4).

## Relaciones

```text
Usuario 1---N Emergencia (CreadoPor)
Usuario 1---N Asignacion (AsignadoPor)
Usuario 1---N EventoAuditoria (UsuarioId)
Emergencia 1---N Asignacion
UnidadRespuesta 1---N Asignacion
Emergencia 1---N EventoAuditoria
UnidadRespuesta 1---N EventoAuditoria
```
