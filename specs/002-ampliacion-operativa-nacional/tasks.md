---

description: "Task list template for feature implementation"
---

# Tasks: Ampliación Operativa Nacional de SIGE

**Input**: Design documents from `/specs/002-ampliacion-operativa-nacional/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/rest-api.md, quickstart.md

**Tests**: Se incluyen tareas de prueba para las reglas críticas (autorización de los
7 roles, redacción de datos públicos, idempotencia del seed, alerta de bajo stock),
igual que en `001-sige-mvp` (Constitución, Principio VI) — no son opcionales.

**Organización**: Tareas agrupadas por historia de usuario (spec.md), en el mismo
orden de prioridad P1→P2→P3 ya justificado en cada "Why this priority".

## Formato: `[ID] [P?] [Story] Descripción`

- **[P]**: puede ejecutarse en paralelo (archivos distintos, sin dependencias pendientes)
- **[Story]**: US1–US9, según `spec.md`
- Rutas de archivo según `plan.md` (backend Jason Taylor extendido; frontend Angular + CoreUI extendido)

---

## Phase 1: Setup

**Purpose**: dependencias nuevas que no pertenecen a ninguna historia específica.

- [x] T001 [P] Agregar dependencia `@coreui/icons` en `frontend/package.json` y crear el registro de íconos en `frontend/src/app/icons/icon-subset.ts` (research.md §4) para los ejemplos de la sección 6 del documento de origen (fuego, cruz médica, vehículo, ola, montaña, alerta, avión/helicóptero, persona/rescate) — registrado en `app.ts` vía `IconSetService`
- [x] T002 [P] Agregar `SIGE_DEMO_PASSWORD` a `.env.example` (raíz del repo, sin valor real de producción — es una contraseña demo, no un secreto) con el default documentado `DemoSige#2026`; el código lo lee con `Environment.GetEnvironmentVariable("SIGE_DEMO_PASSWORD") ?? "DemoSige#2026"` (T027)

**Checkpoint**: dependencias nuevas instaladas; sin cambios de dominio todavía.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: modelo de datos ampliado, roles nuevos y migración del `Tipo` legado del
MVP. Bloquea todas las historias de usuario de esta ampliación.

**⚠️ CRITICAL**: ninguna historia de esta ampliación puede completarse sin esta fase.

- [x] T003 [P] Crear entidad `Institucion` en `backend/src/Domain/Entities/Institucion.cs` (`Nombre` string requerido único, `Activo` bool default true) — catálogo único compartido (data-model.md, CHK038)
- [x] T004 [P] Crear entidad `TipoEmergencia` en `backend/src/Domain/Entities/TipoEmergencia.cs` (`Nombre` string requerido único máx. 100 caracteres, `Ambito` enum Terrestre/Marítimo/Aéreo/Mixto, `Icono` string, `Color` string hex, `PrioridadPorDefecto` enum del MVP, `Activo` bool default true) — FR-101
- [x] T005 [P] Crear entidad `Personal` en `backend/src/Domain/Entities/Personal.cs` (`Nombres`/`Apellidos`/`Documento` requeridos, `InstitucionId` FK, `Especialidad`, `Funcion`, `Certificaciones` nullable, `Disponible` bool default true, `UnidadRespuestaId` FK) — FR-110
- [x] T006 [P] Crear entidad `Recurso` en `backend/src/Domain/Entities/Recurso.cs` (`Codigo`/`Nombre` requeridos, `Categoria` enum Equipos/Herramientas/Víveres/Líquidos/Estructuras/MaterialMédico/EquipoRescate, `UnidadMedida`, `Cantidad`, `CantidadDisponible`, `CantidadMinima`, `UnidadRespuestaId` FK) — FR-111, FR-112
- [x] T007 [P] Crear value object `Ubicacion` (owned type EF Core) en `backend/src/Domain/ValueObjects/Ubicacion.cs` (`Departamento`/`Provincia`/`Distrito`/`CentroPoblado`/`Direccion`/`Referencia` nullable, `Latitud`/`Longitud` requeridas rango -90..90/-180..180, `Ambito` enum **Terrestre/Marítimo/Aéreo/Mixto** (4 valores, igual que `TipoEmergencia.Ambito` — corregido tras Analyze I1), `+SinDireccionFormal` bool default false — corregido tras Analyze I2) — FR-104. Validador: Departamento/Provincia/Distrito requeridos **salvo que `SinDireccionFormal == true`** (no se infiere de `Ambito`)
- [x] T008 Extender entidad `Emergencia` en `backend/src/Domain/Entities/Emergencia.cs`: `+TipoEmergenciaId` FK (nullable durante migración), `+Ubicacion` (owned type de T007), `+Afectados`/`Heridos`/`Desaparecidos`/`Fallecidos`/`Evacuados` (int, default 0); conservar `Tipo` (texto libre) como columna legada de solo lectura (Principio IV, no se borra) — FR-105, FR-125
- [x] T009 Extender entidad `UnidadRespuesta` en `backend/src/Domain/Entities/UnidadRespuesta.cs`: `+Ubicacion` (owned type de T007), `+InstitucionId` FK, `+UsuarioId` FK nullable a `AspNetUsers`, `+ICollection<Personal>`, `+ICollection<Recurso>`
- [x] T010 Agregar los 5 roles nuevos (`Administrador`, `CoordinadorLogistico`, `JefeDeUnidad`, `UnidadDeRespuesta`, `Visualizador`) como constantes en `backend/src/Domain/Constants/Roles.cs`, junto a `Operador`/`Supervisor` ya existentes — FR-117
- [x] T011 Extender `ApplicationUser` (Identity) en `backend/src/Infrastructure/Identity/ApplicationUser.cs`: `+InstitucionId` FK nullable, `+UltimoAcceso` DateTime nullable, `+RequiereCambioPassword` bool default false, `+CreadoPor`/`ModificadoPor` string FK a `AspNetUsers.Id` — FR-118, FR-119
- [x] T012 Configurar EF Core para las entidades/owned types nuevos en `backend/src/Infrastructure/Data/Configurations/{Institucion,TipoEmergencia,Personal,Recurso}Configuration.cs`, y actualizar `EmergenciaConfiguration`/`UnidadRespuestaConfiguration` para mapear `Ubicacion` como owned type
- [x] T013 Generar y aplicar la migración EF Core `AmpliacionOperativaNacional` (`dotnet ef migrations add` en `backend/src/Infrastructure/Migrations`) contra el MySQL existente (`sige_app`), verificando el esquema resultante
- [x] T014 Sembrar los 5 roles nuevos de forma idempotente (`RoleManager.RoleExistsAsync` antes de crear) en `backend/src/Infrastructure/Data/ApplicationDbContextInitialiser.cs` — FR-117, FR-123
- [x] T015 Implementar la migración de datos de FR-103 en `ApplicationDbContextInitialiser`: para cada `Emergencia` con `TipoEmergenciaId == null`, buscar `TipoEmergencia` por nombre (case-insensitive) o crearlo **activo** por defecto, y vincular — algoritmo exacto en `data-model.md` §"Migración de datos"
- [x] T016 Condicionar la ejecución del seed de roles/usuarios/datos demo (T014, y los de US2) a `env.IsDevelopment() || env.EnvironmentName is "Demo" or "Testing"`, nunca en Production — FR-124
- [x] T016a [P] Sembrar el catálogo inicial de `Institucion` (bomberos, policía, salud, marina, etc.) de forma idempotente en `ApplicationDbContextInitialiser` — **agregado tras Analyze (hallazgo C2)**: `Personal`, `UnidadRespuesta` y `Usuario` referencian `InstitucionId` y necesitan filas existentes antes de poder crearse
- [x] T016b Query `ListarInstitucionesQuery` + endpoint `GET /api/Instituciones` (autenticado) en `backend/src/Application/Instituciones/Queries/` y `backend/src/Web/Endpoints/Instituciones.cs` — **agregado tras Analyze (hallazgo C2)**: sin esto, ningún selector de institución en el frontend (Usuario, Personal, Unidad) tiene de dónde leer las opciones

**Checkpoint**: esquema de base de datos migrado, roles nuevos sembrados de forma
idempotente, emergencias del MVP vinculadas al catálogo de tipos, catálogo de
instituciones disponible. A partir de aquí cada historia de usuario es independiente.

---

## Phase 3: User Story 1 - Administrar usuarios y roles ampliados (Priority: P1) 🎯

**Goal**: un Administrador crea, edita, bloquea/desbloquea usuarios y les asigna
roles ampliados.

**Independent Test**: iniciar sesión como Administrador, crear un usuario, asignarle
el rol Coordinador logístico, bloquearlo y desbloquearlo, sin que exista ninguna
emergencia ni unidad todavía.

### Tests para User Story 1

- [x] T017 [P] [US1] Prueba unitaria: solo `Administrador` puede ejecutar los Commands/Queries de `Usuarios` en `backend/tests/Application.UnitTests/Usuarios/AutorizacionUsuariosTests.cs` — FR-116
- [x] T018 [P] [US1] Prueba unitaria: un usuario con rol `Visualizador` es rechazado por los Commands de administración de usuarios y por los de Personal/Recursos en `backend/tests/Application.UnitTests/Usuarios/AutorizacionVisualizadorTests.cs` — FR-120a

### Implementación de User Story 1

- [x] T019 [US1] Commands `CrearUsuario`, `EditarUsuario`, `BloquearUsuario`, `DesbloquearUsuario`, `ForzarCambioPassword` (con `[Authorize(Roles = "Administrador")]`) en `backend/src/Application/Usuarios/Commands/`
- [x] T020 [US1] Query `ListarUsuariosQuery` (rol, estado, último acceso, intentos fallidos) en `backend/src/Application/Usuarios/Queries/ListarUsuariosQuery.cs` — FR-119
- [x] T021 [US1] Endpoint group `backend/src/Web/Endpoints/Usuarios.cs`: `POST /api/Usuarios`, `PATCH /api/Usuarios/{id}`, `POST /api/Usuarios/{id}/bloquear`, `POST /api/Usuarios/{id}/desbloquear`, `POST /api/Usuarios/{id}/forzar-cambio-password`, `GET /api/Usuarios`
- [x] T022 [US1] Registrar `UltimoAcceso` e incrementar `IntentosFallidos` (vía `SignInManager`/`AccessFailedCount` de Identity) en el flujo de login existente (`backend/src/Web/Endpoints/Users.cs`) — FR-119
- [x] T023 [P] [US1] Servicio Angular `UsuariosService` en `frontend/src/app/core/services/usuarios.service.ts`
- [x] T024 [P] [US1] Vista Angular de administración de usuarios (listar, crear, editar, bloquear/desbloquear, forzar cambio de password) en `frontend/src/app/views/usuarios/usuarios.component.ts`, reutilizando `app-form-field`
- [x] T025 [US1] Restringir el enlace "Usuarios" del layout/sidebar al rol `Administrador` en `frontend/src/app/layout/default-layout/`
- [x] T025a [P] [US1] Prueba unitaria: un usuario con rol `UnidadDeRespuesta` no puede cambiar el estado operativo de una unidad distinta a la suya (`UnidadRespuesta.UsuarioId != IUser.Id`) en `backend/tests/Application.UnitTests/Unidades/AutorizacionUnidadPropiaTests.cs` — **agregado tras Analyze (hallazgo C1)** — FR-120
- [x] T025b [US1] Implementar la restricción en `CambiarEstadoOperativoUnidadCommand` (`backend/src/Application/Unidades/Commands/CambiarEstadoOperativoUnidad/`): si el único rol del caller es `UnidadDeRespuesta`, exigir `UnidadRespuesta.UsuarioId == IUser.Id`; Operador/Supervisor no quedan sujetos a esta restricción — **agregado tras Analyze (hallazgo C1)** — FR-120
- [x] T025c [P] [US1] Frontend: vista mínima "mi unidad" (`frontend/src/app/views/mi-unidad/mi-unidad.component.ts`) restringida por guard de rol `UnidadDeRespuesta`, que solo permite ver/actualizar el estado operativo y la posición de la propia unidad — **agregado tras Analyze (hallazgo C1)** — FR-120

**Checkpoint**: US1 completamente funcional y probada de forma independiente,
incluida la restricción de acceso propio del rol `UnidadDeRespuesta` (FR-120).

---

## Phase 4: User Story 2 - Generar automáticamente usuarios y datos de demostración (Priority: P1)

**Goal**: al arrancar en Desarrollo/Demo/Pruebas, el sistema genera automáticamente
los 10 usuarios demo y un volumen realista de datos, de forma idempotente.

**Independent Test**: levantar el sistema desde cero en Desarrollo y verificar que,
sin ninguna acción manual, existen los usuarios demo documentados y datos variados.

### Tests para User Story 2

- [x] T026 [P] [US2] Prueba unitaria de idempotencia: ejecutar el seed dos veces contra una base en memoria y verificar que los conteos de usuarios/roles/datos no cambian en la segunda ejecución, en `backend/tests/Application.UnitTests/Seed/SeedIdempotenteTests.cs` — FR-123, SC-103

### Implementación de User Story 2

- [x] T027 [US2] Extender `ApplicationDbContextInitialiser` para crear, si no existen (`UserManager.FindByNameAsync` antes de `CreateAsync`), los 10 usuarios demo de la sección 19.1 del documento de origen (`admin.sige`, `supervisor.sige`, `operador.lima`, `operador.norte`, `logistica.sige`, `jefe.unidad01`, `unidad.maritima01`, `unidad.terrestre01`, `visor.sige`, `prueba.restringida`), con contraseña desde `SIGE_DEMO_PASSWORD` — FR-122
- [x] T028 [US2] Crear `backend/src/Infrastructure/Data/DemoDataSeeder.cs`: genera (solo si las tablas están vacías) 40–60 emergencias, 25–40 unidades, 60–100 personal y 100+ recursos variados, invocado desde `ApplicationDbContextInitialiser` — FR-122
- [x] T029 [P] [US2] Documentar las credenciales demo (usuario/rol) en un archivo local no expuesto en producción (por ejemplo `backend/docs/usuarios-demo.md`), acorde a la sección 19.4 del documento de origen

**Checkpoint**: US2 completamente funcional; conteos verificados antes/después de un
segundo arranque.

---

## Phase 5: User Story 3 - Registrar emergencias con catálogo nacional y cobertura geográfica completa (Priority: P1)

**Goal**: un Operador registra una emergencia con tipo/subtipo del catálogo y
ubicación en cualquier punto del país, incluidas zonas sin dirección formal.

**Independent Test**: un Administrador crea un tipo marítimo nuevo, y un Operador
registra una emergencia de ese tipo solo con coordenadas.

### Tests para User Story 3

- [x] T030 [P] [US3] Prueba unitaria del validador de `Ubicacion`: latitud/longitud siempre requeridas; departamento/provincia/distrito requeridos **salvo que `SinDireccionFormal == true`** (corregido tras Analyze I2 — no se infiere de `Ambito`) en `backend/tests/Application.UnitTests/Emergencias/UbicacionValidatorTests.cs` — FR-104

### Implementación de User Story 3

- [x] T031 [US3] Commands `CrearTipoEmergencia`, `EditarTipoEmergencia` (activar/desactivar) en `backend/src/Application/TiposEmergencia/Commands/` — `[Authorize(Roles = "Administrador")]` — FR-101, FR-102
- [x] T032 [US3] Query `ListarTiposEmergenciaQuery` (`?soloActivos=true`) en `backend/src/Application/TiposEmergencia/Queries/ListarTiposEmergenciaQuery.cs`
- [x] T033 [US3] Endpoint group `backend/src/Web/Endpoints/TiposEmergencia.cs`
- [x] T034 [US3] Actualizar `CrearEmergenciaCommand`/validador para aceptar `tipoEmergenciaId` y `Ubicacion` completa, rechazando tipos desactivados — FR-102, FR-104, FR-105 — adelantado durante Foundational: el cambio de esquema de `Emergencia` (T008) forzaba a actualizar este Command para poder compilar
- [x] T035 [US3] Actualizar `EmergenciaDto`/`ListarEmergenciasQuery`/`ObtenerEmergenciaPorIdQuery` para incluir el tipo del catálogo y la ubicación completa (coordenada exacta, solo autenticado — FR-115a) — adelantado durante Foundational, mismo motivo que T034
- [x] T036 [P] [US3] Frontend: selector de tipo/subtipo dependiente + campos de ubicación (departamento/provincia/distrito/centro poblado/dirección/referencia, ocultos/opcionales cuando se marca el checkbox "sin dirección formal" — corregido tras Analyze I2, no depende del ámbito) en `frontend/src/app/views/emergencias/emergencias.component.ts`
- [x] T037 [P] [US3] Frontend: administración simple del catálogo de tipos (crear/editar/activar/desactivar) en `frontend/src/app/views/tipos-emergencia/tipos-emergencia.component.ts`

**Checkpoint**: US3 completamente funcional. **Con US1+US2+US3 completas, el
alcance P1 de esta ampliación es demostrable de punta a punta.**

---

## Phase 6: User Story 4 - Explorar el mapa operativo avanzado (Priority: P2)

**Goal**: leyenda plegable con capas, iconografía por tipo, filtros y resumen rápido
por marcador.

**Independent Test**: con datos demo cargados, activar/desactivar capas, aplicar un
filtro por tipo y abrir el resumen de un marcador.

- [x] T038 [US4] Extender `ListarEmergenciasQuery`/`ListarUnidadesQuery` con filtros por tipo, subtipo, prioridad, estado, ubicación, ámbito, tipo de unidad, institución y disponibilidad — FR-108
- [x] T039 [P] [US4] Frontend: leyenda plegable con capas activables/desactivables (emergencias, unidades, bases, hospitales, puertos, rutas, histórico) en `frontend/src/app/views/mapa/leyenda/leyenda.component.ts` — FR-107
- [x] T040 [P] [US4] Frontend: iconografía distinta por tipo de emergencia/unidad (usando `@coreui/icons` de T001) en los marcadores Leaflet de `frontend/src/app/views/mapa/mapa.component.ts` — FR-106
- [x] T041 [P] [US4] Frontend: filtros del mapa conectados a los parámetros de T038 — FR-108
- [x] T042 [US4] Frontend: popup/modal de resumen al hacer clic en un marcador (emergencia o unidad), con botón "Ver detalle completo", sin cerrar el mapa — FR-109

**Checkpoint**: US4 completamente funcional y probada de forma independiente.

---

## Phase 7: User Story 5 - Gestionar personal y recursos de las unidades (Priority: P2)

**Goal**: registrar personal y recursos por unidad, con alerta de bajo stock.

**Independent Test**: registrar una unidad, asignarle personal y un recurso con
cantidad/mínimo, y verificar que se puede consultar al seleccionar la unidad.

### Tests para User Story 5

- [x] T043 [P] [US5] Prueba unitaria: un recurso con `CantidadDisponible < CantidadMinima` se marca `bajoStock = true` en `backend/tests/Application.UnitTests/Recursos/AlertaBajoStockTests.cs` — FR-112

### Implementación de User Story 5

- [x] T044 [US5] Commands `CrearPersonal`, `EditarPersonal` (`[Authorize(Roles = "CoordinadorLogistico,JefeDeUnidad")]`) en `backend/src/Application/PersonalUnidades/Commands/` — FR-110 — **carpeta/namespace renombrado a `PersonalUnidades`** (no `Personal`): la entidad `Sige.Domain.Entities.Personal` colisionaba en tiempo de compilación con un namespace `Sige.Application.Personal` (CS0118, "'Personal' es espacio de nombres pero se usa como tipo" en `IApplicationDbContext.cs`)
- [x] T045 [US5] Commands `CrearRecurso`, `ActualizarCantidadRecurso` en `backend/src/Application/Recursos/Commands/` — FR-111, FR-112
- [x] T046 [US5] Queries `ListarPersonalPorUnidadQuery`, `ListarRecursosPorUnidadQuery` (con `bajoStock` calculado) en `backend/src/Application/{Personal,Recursos}/Queries/`
- [x] T047 [US5] Endpoints `backend/src/Web/Endpoints/{Personal,Recursos}.cs` bajo `/api/Unidades/{unidadId}/personal` y `/api/Unidades/{unidadId}/recursos`
- [x] T048 [P] [US5] Frontend: sección de personal y recursos en el detalle de unidad, con indicador visual de bajo stock, en `frontend/src/app/views/unidades/detalle/unidad-detalle.component.ts`

**Checkpoint**: US5 completamente funcional y probada de forma independiente.

---

## Phase 8: User Story 6 - Completar formularios enriquecidos (Priority: P2)

**Goal**: formularios organizados en secciones, con campos dependientes y el mismo
estándar de validación/componente reutilizable del MVP.

**Independent Test**: abrir el formulario ampliado de "Nueva emergencia" y verificar
sus secciones y validaciones sin completar un flujo de negocio completo.

- [x] T049 [US6] Ampliar `CrearEmergenciaCommandValidator` con `Afectados`/`Heridos`/`Desaparecidos`/`Fallecidos`/`Evacuados` — FR-125 — ya completado durante Foundational (T008/T034 adelantados por necesidad de compilación)
- [x] T050 [P] [US6] Frontend: reorganizar el formulario de emergencia en secciones (datos generales, ubicación, afectados, observaciones — sin sección "evidencias": no existe una entidad Evidencia en `data-model.md`, fuera de alcance sin subir archivos), reutilizando `app-form-field` (FR-126) en `frontend/src/app/views/emergencias/emergencias.component.ts`
- [x] T051 [P] [US6] Frontend: campo dependiente al elegir el tipo — **sustituye "subtipo" por Prioridad y Ámbito**, ya que `data-model.md` no define una entidad Subtipo separada (simplificación documentada en la Etapa de Plan); al cambiar `tipoEmergenciaId` se autocompletan `Prioridad` (desde `TipoEmergencia.PrioridadPorDefecto`) y `Ambito` (desde `TipoEmergencia.Ambito`), ambos editables después — FR-125

**Checkpoint**: US6 completamente funcional y probada de forma independiente.

---

## Phase 9: User Story 7 - Consultar la vista pública sin autenticación (Priority: P3)

**Goal**: mapa y listado públicos sin datos sensibles, con línea de tiempo pública.

**Independent Test**: sin iniciar sesión, abrir la vista pública, filtrar y abrir el
resumen de una emergencia, verificando que ningún dato restringido aparece.

### Tests para User Story 7

- [x] T052 [P] [US7] Prueba: ausencia de todos los campos de FR-114, verificada de dos formas complementarias — (1) `backend/tests/Application.UnitTests/Publico/RedaccionDatosPublicosTests.cs` (InMemory: guard por reflexión sobre los DTOs + verificación de que la ubicación devuelta no es la exacta), en vez de `Application.FunctionalTests` porque esa infraestructura exige una base MySQL de pruebas real (`ConnectionStrings__SigeDbTest`) nunca aprovisionada ni en el MVP ni aquí; y (2) `curl` real sin `Authorization` contra el backend en ejecución, inspeccionando el **cuerpo completo** de la respuesta (`quickstart.md` Escenario 5) — SC-105

### Implementación de User Story 7

- [x] T053 [US7] DTOs propios `EmergenciaPublicaDto`/`EmergenciaPublicaDetalleDto` (nunca reutilizan el DTO interno) en `backend/src/Application/Publico/`
- [x] T054 [US7] Queries `ListarEmergenciasPublicasQuery`/`ObtenerEmergenciaPublicaQuery`, sin `[Authorize]`, en `backend/src/Application/Publico/Queries/` — **desviación de research.md §3**: la ubicación aproximada se implementó redondeando lat/lon a 1 decimal (~11 km) en memoria tras materializar la consulta, no con un centroide de distrito precalculado (no existe un catálogo de centroides en `data-model.md`); sigue cumpliendo FR-115 (nunca la coordenada exacta)
- [x] T055 [US7] Endpoint group anónimo `backend/src/Web/Endpoints/Publico.cs`: `GET /api/Publico/Emergencias`, `GET /api/Publico/Emergencias/{codigo}` (404 genérico si no existe o está fuera de alcance público)
- [x] T056 [P] [US7] Frontend: vista pública fuera del layout autenticado (mapa, listado, filtros, leyenda, timeline pública) en `frontend/src/app/views/publico/publico.component.ts`
- [x] T057 [US7] Registrar la ruta pública sin `authGuard` en `frontend/src/app/app.routes.ts`

**Checkpoint**: US7 completamente funcional; SC-105 verificado con `curl` real, no
solo con la pantalla.

---

## Phase 10: User Story 8 - Ver un dashboard operativo avanzado (Priority: P3)

**Goal**: indicadores por ámbito, tiempo promedio de atención, personal y recursos.

**Independent Test**: con datos demo cargados, verificar que los nuevos indicadores
coinciden con los datos reales.

- [x] T058 [US8] Extender `ObtenerIndicadoresDashboardQuery` con `emergenciasPorAmbito`, `tiempoPromedioAtencionMinutos`, `personalDesplegado`, `recursosMovilizados` — FR-127
- [x] T059 [P] [US8] Frontend: nuevos widgets del dashboard (desglose por ámbito, tiempo promedio) en `frontend/src/app/views/dashboard/dashboard.component.ts`

**Checkpoint**: US8 completamente funcional y probada de forma independiente.

---

## Phase 11: User Story 9 - Usar una interfaz operativa en tema oscuro (Priority: P3)

**Goal**: tema oscuro consistente en toda la aplicación, sin perder contraste.

**Independent Test**: recorrer las pantallas principales con el tema activo y
verificar consistencia y contraste.

- [x] T060 [US9] Activar el tema oscuro nativo de CoreUI (`data-coreui-theme="dark"`) con un toggle en `frontend/src/app/layout/default-layout/` — FR-128 — nuevo `ThemeService` (persistido en `localStorage`, aplicado desde `app.ts` para que también cubra login y vista pública, no solo el layout autenticado); toggle agregado en las 3 pantallas (layout, login, vista pública)
- [x] T061 [P] [US9] Revisar el contraste de los colores de prioridad/estado (`frontend/src/app/core/models/labels.ts`) sobre tema oscuro y ajustar si es necesario — FR-128 — se detectó y corrigió un riesgo real de contraste: el estado "Cerrada" usaba `text-dark`/`bg-dark` (texto casi negro sobre fondo oscuro), cambiado a `text-body-emphasis` (labels.ts) y `text-bg-dark` (leyenda del mapa), ambas utilidades de Bootstrap 5.3+/CoreUI 5 que se adaptan al tema activo; el resto de badges (secondary/info/warning/danger/success/primary) no presentan el mismo riesgo porque no invierten significado entre temas. **Pendiente**: verificación visual real en navegador (sin herramienta de captura en esta sesión, igual que T067 del MVP) — recomendado que Diego confirme visualmente antes de la demo

**Checkpoint**: US9 completamente funcional y probada de forma independiente.

---

## Phase 12: Polish & Cross-Cutting Concerns

**Purpose**: verificación final de no regresión y cierre de calidad antes de Analyze/Converge.

- [ ] T062 [P] Revalidar con `curl` (cuerpo completo de la respuesta, no solo el status) que `/api/Publico/*` sigue sin exponer ningún campo de FR-114 tras todos los cambios de Implement — SC-105
- [ ] T063 [P] Revalidar de punta a punta el flujo completo del MVP (registrar → validar → asignar → avanzar → cerrar → reabrir) sin ninguna regresión — SC-106, FR-129
- [ ] T064 [P] Actualizar `quickstart.md` si algún escenario cambió durante Implement
- [ ] T065 Ejecutar `dotnet test` (backend) y `ng test` (frontend) completos y documentar el resultado en la bitácora
- [ ] T066 Revisar los ítems restantes de `checklists/ampliacion-quality.md` (CHK001-CHK036) y dejar una disposición final explícita de cada uno (resuelto / aceptado fuera de alcance / diferido)
- [ ] T067 Recorrido manual en navegador (1024px) de las pantallas nuevas (usuarios, catálogo de tipos, mapa avanzado, personal/recursos, vista pública, dashboard, tema oscuro); incluir explícitamente la verificación de SC-104 (identificar el tipo de al menos 5 marcadores distintos solo por su ícono)
- [ ] T068 [P] Corregir hallazgos MEDIUM/LOW de `/speckit-analyze` no aplicados en Plan: citar `Recurso.CantidadDisponible <= Recurso.Cantidad` en el validador de T045 (U1); citar el formato de `Personal.Documento` en T044 (U2); corregir el typo "restablecimi.de" en `spec.md` US1 (A1); reescribir FR-120a con "MUST NOT" (A2)

---

## Dependencies & Execution Order

- **Setup (Phase 1)**: sin dependencias — puede iniciarse de inmediato.
- **Foundational (Phase 2)**: depende de Setup; **bloquea todas las historias de usuario**.
- **User Stories (Phases 3-11)**: cada una depende únicamente de Foundational, no entre sí — pueden implementarse en cualquier orden una vez completada la Fase 2, aunque se recomienda el orden P1→P2→P3 ya justificado en `spec.md`.
  - Excepción de orden práctico: US2 (seed) es más fácil de verificar después de US1 (roles ya administrables) y US3 (catálogo ya existente), aunque técnicamente el seed no depende de sus endpoints, solo de las entidades de Foundational.
- **Polish (Phase 12)**: depende de que todas las historias priorizadas para esta iteración estén completas.

## Parallel Example (Foundational)

```
T003 Institucion, T004 TipoEmergencia, T005 Personal, T006 Recurso, T007 Ubicacion
→ pueden crearse en paralelo (archivos de entidad distintos)
T008 Emergencia y T009 UnidadRespuesta → después de T007 (dependen del value object Ubicacion)
```

## Implementation Strategy

**MVP de esta ampliación** = Setup + Foundational + US1 (Administrar usuarios y
roles) — es la historia P1 más independiente y la que valida que el esquema de
roles ampliado funciona antes de construir el resto.

**Entrega incremental recomendada**: Foundational → US1 → US2 → US3 (con esto, el
alcance P1 completo es demostrable) → US4 → US5 → US6 (P2) → US7 → US8 → US9 (P3) →
Polish. Cada historia es un incremento independiente y verificable con su propio
"Independent Test" de `spec.md`.
