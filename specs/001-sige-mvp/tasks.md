---

description: "Task list template for feature implementation"
---

# Tasks: MVP Sistema Integral de Gestión de Emergencias (SIGE)

**Input**: Design documents from `/specs/001-sige-mvp/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/rest-api.md, quickstart.md

**Tests**: Se incluyen tareas de prueba (unitarias e integración) porque la constitución
(Principio VI) y el plan las exigen para las reglas críticas y los endpoints
principales — no son opcionales en este proyecto.

**Organización**: Tareas agrupadas por historia de usuario (spec.md), siguiendo además
el recorrido vertical de la guía (estructura → dominio → auth → emergencias → unidades
→ asignación → timeline → dashboard → pruebas/datos demo → endurecimiento).

## Formato: `[ID] [P?] [Story] Descripción`

- **[P]**: puede ejecutarse en paralelo (archivos distintos, sin dependencias pendientes)
- **[Story]**: US1–US5, según `spec.md`
- Rutas de archivo según `plan.md` (backend con plantilla Jason Taylor; frontend Angular + CoreUI)

---

## Phase 1: Setup

**Purpose**: estructura base y ejecución local (punto 1 del recorrido vertical, guía §12)

- [x] T001 Escafoldar backend con la plantilla Jason Taylor (`dotnet new ca-sln -o backend`), verificando que se generan `backend/src/Domain`, `backend/src/Application`, `backend/src/Infrastructure`, `backend/src/Web` y sus proyectos de test — se usó `--client-framework None --database sqlserver` (más cercano a MySQL) y se eliminó el AppHost/TestAppHost de Aspire (orquestaba su propio SQL Server; no aplica porque MySQL ya corre externamente)
- [x] T002 Escafoldar frontend Angular e instalar CoreUI (`ng new frontend`, `ng add @coreui/angular`) — `DefaultLayoutComponent` creado en `frontend/src/app/layout/default-layout/`; en vez de un `c-sidebar` + `_nav.ts` se usó una barra superior (`c-header`+`c-nav`) para evitar apostar a APIs de CoreUI no verificadas; vistas placeholder creadas para las 5 pantallas
- [x] T003 Crear `docker-compose.yml` en la raíz que orquesta `backend` (con Dockerfile propio) y se conecta al contenedor MySQL ya existente vía `host.docker.internal`; el servicio `frontend` queda comentado hasta dockerizarlo
- [x] T004 [P] Crear `.env.example` en la raíz con la cadena de conexión a MySQL y demás variables, sin secretos reales
- [ ] T005 [P] Configurar linting/formato: `dotnet format` para backend, ESLint + Prettier para frontend — pendiente; el backend solo tiene el `.editorconfig` de la plantilla

**Checkpoint**: backend verificado end-to-end contra MySQL real (ver bitácora); `docker compose up --build` del backend pendiente de probar (falta el servicio frontend).

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: modelo de dominio y persistencia + autenticación/roles mínimos (puntos 2–3
del recorrido vertical). Bloquea todas las historias de usuario.

- [x] T006 [P] Crear entidad `Emergencia` en `backend/src/Domain/Entities/Emergencia.cs` con los campos de `data-model.md` (Tipo, Descripcion, Latitud, Longitud, Prioridad, FechaHoraReporte, ReportanteNombre obligatorio, ReportanteContacto opcional — FR-019, Estado, CreadoPor)
- [x] T007 [P] Crear entidad `UnidadRespuesta` en `backend/src/Domain/Entities/UnidadRespuesta.cs` (Tipo: Ambulancia/Bomberos/Patrullero, Identificador, EstadoOperativo: Disponible/Ocupada/FueraDeServicio, Latitud, Longitud) — **sin columna `RowVersion`**: MySQL no tiene un equivalente directo al rowversion de SQL Server; FR-007 se implementará con relectura transaccional en T038, no con un token de concurrencia dedicado (ver `data-model.md`)
- [x] T008 [P] Crear entidad `Asignacion` en `backend/src/Domain/Entities/Asignacion.cs` (EmergenciaId, UnidadId, EstadoAsignacion: despachada/en ruta/en el lugar/atendida — FR-016, AsignadoPor, FechaHoraAsignacion)
- [x] T009 [P] Crear entidad `EventoAuditoria` en `backend/src/Domain/Entities/EventoAuditoria.cs` (EmergenciaId/UnidadId nullable, TipoEvento, EstadoAnterior/EstadoNuevo, UsuarioId, FechaHora — FR-010). "Usuario" no es una entidad propia: se usa `ApplicationUser` de ASP.NET Core Identity (ya incluido en la plantilla) con roles `Operador`/`Supervisor` (`Domain/Constants/Roles.cs`)
- [x] T010 Configurar `DbContext` y migraciones EF Core con proveedor `Pomelo.EntityFrameworkCore.MySql` en `backend/src/Infrastructure/Data` (nombre de carpeta de la plantilla; no "Persistence") — migración `InitialCreate` generada y aplicada con `Database.MigrateAsync()`, verificada contra MySQL real (tabla `__EFMigrationsHistory` presente)
- [x] T011 Autenticación configurada vía **ASP.NET Core Identity Bearer Tokens** (`AddBearerToken`, ya en la plantilla) en vez de un servicio JWT hecho a mano; roles `Operador`/`Supervisor` sembrados. Falta aplicar `[Authorize(Roles = "Supervisor")]` a endpoints concretos — se hace junto con cada endpoint en las fases US2/US4
- [ ] T012 Login: la plantilla ya expone `POST /api/Users/login` vía `MapIdentityApi<ApplicationUser>()` (`Web/Endpoints/Users.cs`) — **no se creó un `AuthController` propio ni se probó este endpoint todavía**; queda pendiente de verificación end-to-end
- [ ] T013 Hub SignalR `OperacionesHub` — **no creado todavía**
- [ ] T014 Middleware de error de negocio → `409 Conflict` (`UNIDAD_NO_DISPONIBLE`) — **no creado todavía**; la plantilla solo tiene el `ProblemDetailsExceptionHandler` genérico
- [x] T015 [P] Seed de datos demo en `backend/src/Infrastructure/Data/ApplicationDbContextInitialiser.cs`: 1 Operador, 1 Supervisor, 3 unidades (AMB-01, BOM-01, PAT-01) disponibles — **verificado con una consulta directa a MySQL**, ver bitácora
- [ ] T016 [P] Cliente HTTP + interceptor JWT + guard de rol en `frontend/src/app/core` — **no creado todavía**
- [ ] T017 [P] Cliente SignalR compartido en frontend — **no creado todavía**
- [ ] T018 [US*] Pantalla de Login: existe un placeholder estático en `frontend/src/app/views/auth/login/login.component.ts`, **sin conectar** a T012

**Checkpoint parcial**: modelo de datos, persistencia y seed verificados end-to-end contra MySQL real. Login, SignalR y los guards de rol del frontend quedan pendientes antes de poder dar por cerrada la Fase 2 por completo.

---

## Phase 3: User Story 1 - Registrar y visualizar una emergencia (Priority: P1) 🎯 MVP

**Goal**: un Operador registra una emergencia y la ve inmediatamente en el mapa operativo.

**Independent Test**: registrar una emergencia con datos válidos y confirmar que aparece en el mapa y en su propio detalle, sin necesidad de unidades ni asignaciones.

### Tests para User Story 1

- [x] T019 [P] [US1] Prueba unitaria de validación de `CrearEmergenciaCommand` (campos obligatorios de FR-001/FR-015/FR-019) en `backend/tests/Application.UnitTests/Emergencias/CrearEmergenciaValidatorTests.cs` — 5 casos, 9/9 pruebas del proyecto en verde
- [ ] T020 [P] [US1] Prueba de integración de `POST /emergencias` y `GET /emergencias/{id}` — **no automatizada**; se verificó manualmente end-to-end con `curl` (login → crear → listar → detalle), ver bitácora. Falta escribirla como prueba real en `Application.FunctionalTests`
- [ ] T020b [P] [US1] Prueba de integración de la revalidación de "validada" antes de asignar — **no automatizada** (depende de US3/T038, que todavía no existe); verificado manualmente que `/validar` exige estado "Reportada"

### Implementación de User Story 1

- [x] T021 [P] [US1] Command `CrearEmergencia` + `CrearEmergenciaCommandValidator` (FluentValidation) en `backend/src/Application/Emergencias/Commands/CrearEmergencia/`
- [x] T022 [US1] Endpoint `POST /api/Emergencias` en `backend/src/Web/Endpoints/Emergencias.cs` — **Minimal API (`IEndpointGroup`), no un `Controller` MVC**: la plantilla Jason Taylor no usa controladores; prefijo real `/api/Emergencias`, no `/emergencias` (contrato actualizado). Asigna "Reportada", genera `EventoAuditoria` "EmergenciaCreada" y emite `EmergenciaActualizada` por SignalR. **Verificado con `curl`**: 401 sin token, 201 con token, 400 si falta un campo obligatorio
- [x] T023 [P] [US1] Queries `ListarEmergenciasQuery` y `ObtenerEmergenciaPorIdQuery` en `backend/src/Application/Emergencias/Queries/`
- [x] T024 [US1] Endpoints `GET /api/Emergencias` y `GET /api/Emergencias/{id}` en `Emergencias.cs` — verificados con `curl`, el detalle incluye `timeline` y `asignaciones`
- [x] T025 [P] [US1] Componente Angular "Nueva emergencia" — **incluido dentro de `emergencias.component.ts`** (formulario + listado en la misma vista) en lugar de un componente separado `nueva-emergencia/`, por simplicidad en esta iteración. **Sin selector de ubicación en el mapa**: los campos lat/lng son inputs numéricos manuales, no un picker interactivo
- [x] T026 [P] [US1] Componente Angular "Mapa operativo" (Leaflet + OSM) en `frontend/src/app/views/mapa/mapa.component.ts`, carga las emergencias reales vía `GET /api/Emergencias` — **sin suscripción a SignalR todavía** (T017 sigue pendiente); por ahora se recarga al entrar a la vista, no en vivo
- [x] T027 [US1] Componente Angular "Detalle de emergencia" en `frontend/src/app/views/emergencias/detalle/emergencia-detalle.component.ts`, muestra la línea de tiempo completa
- [x] T028 [US1] Servicio Angular `EmergenciasService` en `frontend/src/app/core/services/emergencias.service.ts` — **sin integración SignalR** (T017 pendiente); además se creó `AuthService` + `authInterceptor` + `authGuard` (parte de T016, no completo: falta guard específico por rol, solo hay guard de autenticación)
- [x] T028b [US1] Command `ValidarEmergencia` en `backend/src/Application/Emergencias/Commands/ValidarEmergencia/` — agregado tras `/speckit-analyze` (hallazgo C1)
- [x] T028c [US1] Endpoint `POST /api/Emergencias/{id}/validar` en `Emergencias.cs` (depende de T028b); genera `EventoAuditoria` "EmergenciaValidada" y emite `EmergenciaActualizada` — verificado con `curl` (204, timeline con 2 eventos)
- [x] T028d [US1] Botón "Validar" en el detalle de emergencia, visible solo si `estado === Reportada`

**Checkpoint**: User Story 1 funcional y **verificada end-to-end contra MySQL real** (backend con `curl`; frontend compila y sirve, sin verificación visual en navegador — no hay herramienta de captura de pantalla disponible en esta sesión). SC-001/SC-002 pendientes de medir con un cronómetro real en una demo.

---

## Phase 4: User Story 2 - Administrar unidades y conocer su disponibilidad (Priority: P2)

**Goal**: Operador/Supervisor consulta unidades y su disponibilidad; Supervisor las administra (FR-004).

**Independent Test**: dar de alta unidades de cada tipo y verificar que su estado se refleja en listado y mapa, sin que exista todavía una emergencia.

### Tests para User Story 2

- [ ] T029 [P] [US2] Prueba unitaria: solo rol Supervisor puede crear/cambiar estado de una unidad (FR-004) en `backend/tests/Application.UnitTests/Unidades/AutorizacionUnidadesTests.cs`

### Implementación de User Story 2

- [ ] T030 [P] [US2] Commands `CrearUnidad` y `CambiarEstadoOperativoUnidad` (con `[Authorize(Roles = "Supervisor")]`) en `backend/src/Application/Unidades/Commands/`
- [ ] T031 [US2] Endpoints `POST /unidades` y `PATCH /unidades/{id}/estado` en `backend/src/Web/Controllers/UnidadesController.cs` (depende de T030); emite `UnidadActualizada` por SignalR
- [ ] T032 [P] [US2] Query `ListarUnidades` en `backend/src/Application/Unidades/Queries/ListarUnidades.cs`
- [ ] T033 [US2] Endpoint `GET /unidades` en `UnidadesController.cs` (depende de T032)
- [ ] T034 [P] [US2] Componente Angular "Unidades" (listado + alta + cambio de estado, visible según rol) en `frontend/src/app/views/unidades`
- [ ] T035 [US2] Mostrar unidades y su estado operativo en el mapa operativo (integración con T026)

**Checkpoint**: User Stories 1 y 2 funcionan de forma independiente.

---

## Phase 5: User Story 3 - Asignar una o varias unidades a una emergencia (Priority: P1)

**Goal**: asignar unidades disponibles a una emergencia, con revalidación de disponibilidad (FR-006, FR-007).

**Independent Test**: con una emergencia y al menos una unidad disponible, asignarla y confirmar que la unidad pasa a "ocupada" y que una segunda asignación concurrente sobre la misma unidad es rechazada.

### Tests para User Story 3

- [ ] T036 [P] [US3] Prueba unitaria de la revalidación optimista de disponibilidad al confirmar una asignación (FR-007) en `backend/tests/Application.UnitTests/Asignaciones/AsignarUnidadCommandTests.cs`
- [ ] T037 [P] [US3] Prueba de integración simulando dos asignaciones concurrentes sobre la misma unidad, verificando `409 Conflict` con código `UNIDAD_NO_DISPONIBLE` en `backend/tests/Application.FunctionalTests/Asignaciones/ConcurrenciaAsignacionTests.cs` (escenario de `quickstart.md`)

### Implementación de User Story 3

- [ ] T038 [US3] Command `AsignarUnidad` con revalidación optimista dentro de la transacción (FR-007) en `backend/src/Application/Asignaciones/Commands/AsignarUnidad/` (depende de T006–T008, T028b — la emergencia debe estar "validada", FR-009)
- [ ] T039 [US3] Endpoint `POST /emergencias/{id}/asignaciones` en `backend/src/Web/Controllers/AsignacionesController.cs` (depende de T038); marca la unidad como "ocupada", genera `EventoAuditoria` "UnidadAsignada" (FR-010) y emite `EmergenciaActualizada` + `UnidadActualizada`
- [ ] T040 [P] [US3] Componente Angular "Centro de despacho" (selección de unidades disponibles + confirmación de asignación) en `frontend/src/app/views/despacho`
- [ ] T041 [US3] Manejo en frontend del error `409 UNIDAD_NO_DISPONIBLE` con mensaje claro al Operador (depende de T040)

**Checkpoint**: recorrido registrar → asignar ya es demostrable de punta a punta (P1 completo).

---

## Phase 6: User Story 4 - Seguir el estado de la emergencia hasta el cierre (Priority: P2)

**Goal**: avanzar el estado de cada asignación de forma independiente, derivar el estado de la emergencia (FR-016), y cerrar/reabrir solo como Supervisor (FR-017, FR-018).

**Independent Test**: tomar una emergencia ya despachada, avanzar sus asignaciones una por una y verificar el timeline; cerrarla como Supervisor y confirmar liberación de unidades.

### Tests para User Story 4

- [ ] T042 [P] [US4] Prueba unitaria: la emergencia pasa a "atendida" solo cuando todas sus asignaciones activas llegan a "atendida" (FR-016) en `backend/tests/Application.UnitTests/Emergencias/DerivarEstadoEmergenciaTests.cs`
- [ ] T043 [P] [US4] Prueba unitaria: cerrar libera automáticamente las unidades asignadas salvo "fuera de servicio" (FR-018) en `backend/tests/Application.UnitTests/Emergencias/CerrarEmergenciaTests.cs`
- [ ] T044 [P] [US4] Prueba unitaria: solo Supervisor puede cerrar/reabrir (FR-017) en `backend/tests/Application.UnitTests/Emergencias/AutorizacionCierreTests.cs`

### Implementación de User Story 4

- [ ] T045 [US4] Command `CambiarEstadoAsignacion` (avanza despachada→en ruta→en el lugar→atendida y recalcula el estado de la Emergencia) en `backend/src/Application/Asignaciones/Commands/CambiarEstadoAsignacion/`
- [ ] T046 [US4] Endpoint `PATCH /asignaciones/{id}/estado` en `AsignacionesController.cs` (depende de T045); genera `EventoAuditoria` (FR-010)
- [ ] T047 [US4] Commands `CerrarEmergencia` y `ReabrirEmergencia` (`[Authorize(Roles = "Supervisor")]`, libera unidades en cierre — FR-018) en `backend/src/Application/Emergencias/Commands/`
- [ ] T048 [US4] Endpoints `POST /emergencias/{id}/cerrar` y `POST /emergencias/{id}/reabrir` en `EmergenciasController.cs` (depende de T047); generan `EventoAuditoria` "EmergenciaCerrada"/"EmergenciaReabierta" respectivamente (FR-010)
- [ ] T049 [US4] Endpoint `GET /emergencias/{id}/timeline` (lee `EventoAuditoria` ordenado por fecha) en `EmergenciasController.cs`
- [ ] T050 [US4] Componente Angular "Línea de tiempo" dentro del detalle de emergencia, mostrando cada evento con usuario y fecha/hora (depende de T027, T049)
- [ ] T051 [US4] Botones de avance de estado, cierre y reapertura en el detalle de emergencia, visibles/habilitados según rol (depende de T050)

**Checkpoint**: ciclo completo reportar → asignar → avanzar estados → cerrar → timeline es demostrable (recorrido mínimo, guía §16).

---

## Phase 7: User Story 5 - Ver un dashboard con indicadores operativos (Priority: P3)

**Goal**: Supervisor (y Operador) ve indicadores básicos actualizados en tiempo real (FR-011, SC-006).

**Independent Test**: con datos demo cargados, abrir el dashboard y verificar que los conteos coinciden con los datos reales, y que cambian sin recargar la página.

### Tests para User Story 5

- [ ] T052 [P] [US5] Prueba de integración de `GET /dashboard/indicadores` contra datos demo conocidos en `backend/tests/Application.FunctionalTests/Dashboard/IndicadoresTests.cs`

### Implementación de User Story 5

- [ ] T053 [US5] Query `ObtenerIndicadoresDashboard` (conteo por estado/prioridad de emergencias, disponibles vs. ocupadas de unidades) en `backend/src/Application/Dashboard/Queries/`
- [ ] T054 [US5] Endpoint `GET /dashboard/indicadores` en `backend/src/Web/Controllers/DashboardController.cs` (depende de T053)
- [ ] T055 [US5] Componente Angular "Dashboard" con widgets/gráficos de CoreUI en `frontend/src/app/views/dashboard`, suscrito a `EmergenciaActualizada`/`UnidadActualizada` para refresco sin recarga manual (FR-014, SC-006)

**Checkpoint**: las 5 historias de usuario funcionan de forma independiente y en conjunto.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: pruebas y datos demo adicionales + endurecimiento y correcciones finales
(puntos 11–12 del recorrido vertical, guía §12)

- [ ] T056 Ejecutar manualmente el escenario de `quickstart.md` de punta a punta y registrar el resultado en la Bitácora
- [ ] T057 [P] Revisar que ningún secreto quede versionado (`.env`, cadena de conexión MySQL) — Principio "Restricciones Técnicas y de Calidad" de `constitution.md`
- [ ] T058 [P] Revisar accesibilidad/responsividad básica del layout CoreUI para la demo en aula (Principio VII)
- [ ] T059 Resolver los hallazgos diferidos del checklist `mvp-quality.md` que sigan abiertos tras `/speckit-analyze` (CHK001, CHK012/CHK028, CHK019, CHK021, CHK023)
- [ ] T060 Ejecutar toda la suite de pruebas (`dotnet test`, pruebas Angular) y corregir fallos antes de `/speckit-converge`

---

## Dependencies & Execution Order

### Dependencias entre fases

- **Setup (Fase 1)**: sin dependencias.
- **Foundational (Fase 2)**: depende de Setup; bloquea todas las historias de usuario.
- **US1 (Fase 3)**: depende solo de Foundational. Es el MVP mínimo demostrable.
- **US2 (Fase 4)**: depende de Foundational; independiente de US1 en código, pero
  necesaria antes de que US3 tenga sentido en una demo real.
- **US3 (Fase 5)**: depende de Foundational + entidades de US1/US2 ya creadas (usa
  Emergencia y UnidadRespuesta); es la segunda historia P1.
- **US4 (Fase 6)**: depende de US3 (necesita asignaciones existentes para avanzar estado).
- **US5 (Fase 7)**: depende de Foundational; en la práctica requiere datos de US1–US4
  para ser demostrable con sentido.
- **Polish (Fase 8)**: depende de que las historias que se vayan a demostrar estén completas.

### Oportunidades de paralelización

- Todas las tareas `[P]` de una misma fase pueden ejecutarse en paralelo (archivos distintos).
- Backend y frontend de una misma historia pueden avanzar en paralelo una vez existen
  los contratos de API (`contracts/rest-api.md`).
- US1 y US2 pueden desarrollarse en paralelo por personas distintas una vez cerrada la
  Fase 2 (Foundational); US3 debe esperar a que ambas tengan al menos sus modelos y
  endpoints base.

## Implementation Strategy

### MVP mínimo (alineado con guía §16)

1. Fase 1 (Setup) + Fase 2 (Foundational).
2. Fase 3 (US1): registrar y ver en el mapa.
3. Fase 5 (US3): asignar unidad — **nota**: requiere al menos los modelos de Fase 4
   (UnidadRespuesta) aunque no se complete toda la UI de administración de US2.
4. Fase 6 (US4): avanzar estados, cerrar, timeline.
5. **DETENERSE Y VALIDAR** con `quickstart.md` — este es el recorrido mínimo demostrable.

### Entrega incremental

1. Setup + Foundational → base lista.
2. + US1 → demo parcial (registrar/ver en mapa).
3. + US2 → unidades administrables.
4. + US3 → despacho funcional (MVP funcional según guía §16).
5. + US4 → ciclo completo con cierre y timeline.
6. + US5 → dashboard.
7. Polish → endurecimiento antes de `/speckit-converge`.
