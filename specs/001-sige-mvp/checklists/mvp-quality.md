# Requirements Quality Checklist: MVP Sistema Integral de Gestión de Emergencias (SIGE)

**Purpose**: Validar que spec.md y plan.md están suficientemente claros, completos y
consistentes antes de descomponer en tareas (`/speckit-tasks`). Cubre los focos pedidos
por la guía: estados/transiciones, permisos por rol, asignación/liberación de unidades,
concurrencia, validaciones, auditoría, mapa, tiempo real, cierre/reapertura, errores y
casos borde, y verificabilidad del MVP en una demostración.
**Created**: 2026-09-26
**Feature**: [spec.md](../spec.md) | [plan.md](../plan.md)

**Note**: Checklist generado siguiendo `.claude/skills/speckit-checklist/SKILL.md`.
**Review Ownership**: artefacto de revisión propiedad del revisor. Marcar `[x]` solo
cuando se confirme que el criterio de calidad del requisito está satisfecho.
**Marker Semantics**: `[x]` no significa que la implementación esté hecha; significa
que el requisito, tal como está escrito, es claro/completo/consistente.

## Claridad de Estados y Transiciones

- [x] CHK001 - ¿Están definidas explícitamente todas las transiciones permitidas entre
  los 7 estados de una emergencia, incluyendo cuáles están prohibidas? [Clarity, Spec §FR-009] →
  Resuelto en Polish: FR-016 ahora prohíbe explícitamente retroceder o repetir el
  estado de una asignación; implementado y probado (`CambiarEstadoAsignacionTests
  .NoPermiteRetrocederOMantenerElMismoEstado`, `ConflictException TRANSICION_INVALIDA`).
- [x] CHK002 - ¿Se especifica cómo se deriva el estado general de la emergencia cuando
  cada unidad asignada progresa de forma independiente? [Completeness, Spec §FR-016]
- [x] CHK003 - ¿Es medible/verificable el momento exacto en que una emergencia pasa a
  "atendida" cuando tiene varias unidades? [Measurability, Spec §FR-016]

## Permisos por Rol

- [x] CHK004 - ¿Están especificadas las acciones permitidas para el rol Operador y
  cuáles le están explícitamente vedadas? [Completeness, Spec §FR-012, §FR-017]
- [x] CHK005 - ¿Es consistente la restricción de cierre/reapertura a Supervisor entre
  `spec.md` (Clarifications, FR-017) y los endpoints definidos en `plan.md`/`contracts`? [Consistency]
- [x] CHK006 - ¿Se definen los permisos del Supervisor sobre administración de unidades
  (alta, cambio de estado operativo)? [Gap, Spec §FR-004] → Resuelto: FR-004 ahora
  restringe alta y cambio de estado operativo al rol Supervisor.

## Reglas de Asignación y Liberación de Unidades

- [x] CHK007 - ¿Se especifica qué ocurre con la unidad y la asignación cuando una
  emergencia se cierra (se libera automáticamente o requiere acción manual)? [Gap] →
  Resuelto: FR-018 (liberación automática salvo Fuera de Servicio).
- [x] CHK008 - ¿Está definida la regla de incompatibilidad que impide asignar una unidad
  "no disponible" a otra emergencia? [Clarity, Spec §FR-007]
- [x] CHK009 - ¿Se especifica si una misma unidad puede asignarse a más de una
  emergencia simultáneamente en algún escenario válido, o está prohibido sin excepción? [Ambiguity, Spec §FR-006] →
  Resuelto: FR-006 aclara que una unidad ocupada no puede tener asignación simultánea.

## Concurrencia

- [x] CHK010 - ¿Está definido el comportamiento exacto del sistema cuando la
  revalidación optimista de disponibilidad falla al confirmar una asignación? [Completeness, Spec §FR-007, Clarifications]
- [x] CHK011 - ¿Es verificable/objetivo el mensaje o código de error que debe recibir el
  Operador en un conflicto de asignación? [Measurability, Contracts §Asignaciones]
- [x] CHK012 - ¿Se contempla la concurrencia entre un cambio de estado operativo de
  unidad (a "fuera de servicio") y una asignación en curso sobre esa misma unidad? [Coverage, Gap] →
  Revisado en Polish: la Assumption ("reasignar sin perder historial") ya es suficiente
  para el alcance del MVP — `CambiarEstadoOperativoUnidadCommand` no reasigna
  automáticamente (comportamiento correcto: el Operador decide manualmente),
  `EventoAuditoria` registra el cambio, y ninguna `Asignacion` se pierde ni se corrompe.
  No se requiere lógica adicional; se acepta explícitamente sin cambios de código.

## Validaciones

- [x] CHK013 - ¿Están listados explícitamente todos los campos obligatorios de una
  emergencia que el servidor debe validar? [Completeness, Spec §FR-001, §FR-015]
- [x] CHK014 - ¿Se especifica el comportamiento cuando se intenta registrar una
  emergencia sin ubicación geográfica válida? [Clarity, Spec Edge Cases, Assumptions]
- [x] CHK015 - ¿Son consistentes las reglas de validación descritas en `spec.md` con los
  contratos de la Fase 1 (`contracts/rest-api.md`)? [Consistency]

## Auditoría

- [x] CHK016 - ¿Está definida la lista completa de "cambios críticos" que deben generar
  un evento de auditoría? [Completeness, Spec §FR-010]
- [x] CHK017 - ¿Se especifican los campos mínimos que debe tener cada evento de
  auditoría (usuario, fecha/hora, estado anterior/nuevo)? [Clarity, Data Model §EventoAuditoria]
- [x] CHK018 - ¿Es verificable que el 100% de los cambios de estado queda en el timeline,
  tal como exige el criterio de éxito? [Measurability, Spec §SC-004]

## Comportamiento del Mapa

- [ ] CHK019 - ¿Se especifica qué debe mostrar el mapa cuando no hay emergencias activas
  o no hay unidades disponibles (estado vacío)? [Gap, Edge Case] → **Aceptado como fuera
  de alcance del MVP** (decisión final de Polish, 2026-09-26): la demo en aula siempre
  corre con datos precargados (seed); no se implementará un estado vacío dedicado salvo
  que el proyecto se extienda más allá del curso.
- [x] CHK020 - ¿Está definido el tiempo máximo aceptable entre el registro de una
  emergencia y su aparición en el mapa? [Measurability, Spec §SC-002]
- [ ] CHK021 - ¿Se especifica cómo se distinguen visualmente los distintos tipos de
  unidad y estados de emergencia en el mapa? [Gap] → **Aceptado parcialmente**
  (decisión final de Polish, 2026-09-26): el mapa ya distingue emergencias de unidades
  con un ícono propio por tipo de marcador (`MapaComponent`), pero no distingue entre
  sí los 3 tipos de unidad ni los 7 estados de emergencia con colores/íconos propios —
  se deja fuera de alcance del MVP; el color por estado/prioridad ya implementado
  (FR-023) en listado/detalle/dashboard cubre la necesidad principal de consistencia
  visual sin requerir además distinción en el mapa.

## Actualización en Tiempo Real

- [x] CHK022 - ¿Están enumerados todos los eventos SignalR necesarios para que mapa y
  dashboard se actualicen sin recarga manual? [Completeness, Contracts §Eventos SignalR]
- [ ] CHK023 - ¿Se define el comportamiento esperado si la conexión en tiempo real se
  pierde temporalmente (reconexión, estado "desactualizado")? [Gap, Exception Flow] →
  **Sigue diferido, no aplicable todavía** (decisión final de Polish, 2026-09-26): el
  cliente SignalR del frontend (T017) nunca se completó en esta sesión — el hub
  existe en el backend (`OperacionesHub`) pero el frontend no se suscribe a sus
  eventos, así que no hay conexión en vivo que pueda perderse. Se convierte en
  hallazgo real para `/speckit-converge`, no en un ítem de checklist resuelto.
- [x] CHK024 - ¿Es consistente el criterio de éxito SC-006 con los eventos
  `EmergenciaActualizada`/`UnidadActualizada` definidos en el plan? [Consistency]

## Criterios de Cierre y Reapertura

- [x] CHK025 - ¿Está especificada la precondición para poder cerrar una emergencia
  (todas las unidades en "atendida")? [Clarity, Spec §FR-016, User Story 4]
- [x] CHK026 - ¿Se define si existe un límite de tiempo o de veces que una emergencia
  puede reabrirse? [Gap] → Resuelto: FR-017 aclara que no hay límite de reaperturas y
  cada una audita.
- [x] CHK027 - ¿Es objetivamente verificable que una emergencia cerrada conserva su
  historial completo y deja de aparecer como activa? [Measurability, Spec §FR-013, US4]

## Errores y Casos Borde

- [x] CHK028 - ¿Están cubiertos en la especificación los casos borde de unidad "fuera de
  servicio" durante una atención en curso? [Coverage, Spec Edge Cases] → Resuelto en
  Polish: mismo hallazgo que CHK012, aceptado sin cambios de código (ver nota ahí).
- [x] CHK029 - ¿Se especifica el comportamiento del sistema ante datos de reportante
  incompletos (nombre o contacto vacío)? [Gap] → Resuelto: FR-019 (nombre obligatorio,
  contacto opcional).
- [x] CHK030 - ¿Hay algún requisito o suposición que dependa de "conectividad estable"
  sin definir qué ocurre si falla? [Assumption, Spec §Assumptions] → Revisado: es una
  suposición explícita aceptada para un MVP de demostración en aula, sin necesidad de
  un plan de fallback fuera de ese alcance.

## Verificabilidad del MVP en Demostración

- [x] CHK031 - ¿Puede un observador sin conocimiento técnico seguir el ciclo completo
  usando solo lo que se ve en pantalla, según exige SC-005? [Measurability, Spec §SC-005]
- [x] CHK032 - ¿Están todos los criterios de éxito (SC-001 a SC-006) redactados de forma
  medible y sin detalles de implementación? [Clarity, Spec §Success Criteria]
- [x] CHK033 - ¿Existe un recorrido único, documentado en `quickstart.md`, que cubra
  registro → asignación → transiciones → cierre → timeline? [Completeness, Quickstart]

## Notes

- Marcar `[x]` solo tras revisión que confirme que el criterio de calidad del requisito
  está satisfecho; no marca avance de implementación.
- `/speckit-implement` lee el estado de estos checkboxes como gate y no debe modificarlos.
- `checklists/requirements.md` es un checklist distinto, con ciclo de vida propio
  mantenido por `/speckit-specify` y `/speckit-clarify`.
- **Resultado inicial (tras Analyze, antes de Implement): 27/33 ítems pasan.**
- **Resultado final (Polish, 2026-09-26): 30/33 ítems pasan.** CHK001, CHK012 y CHK028
  se resolvieron durante Implement/Polish (ver notas en cada ítem). Quedan 3 sin marcar,
  con disposición final explícita en vez de quedar simplemente "diferidos":
  - **CHK019** y **CHK021**: aceptados como fuera de alcance del MVP (decisión de
    producto, no requieren código).
  - **CHK023**: sigue diferido porque depende de una tarea real no completada
    (cliente SignalR del frontend, T017) — se traslada como hallazgo a
    `/speckit-converge`, no se cierra aquí.
