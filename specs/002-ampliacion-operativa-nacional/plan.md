# Implementation Plan: Ampliación Operativa Nacional de SIGE

**Branch**: `feature/ampliacion-operativa-nacional` (feature id `002-ampliacion-operativa-nacional`) | **Date**: 2026-09-26 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-ampliacion-operativa-nacional/spec.md`

## Summary

Evolucionar el MVP cerrado (`001-sige-mvp`, tag `mvp-sdd-demo`) hacia una plataforma
nacional de monitoreo y coordinación de emergencias, sin reemplazar lo ya construido:
catálogo administrable de tipos/subtipos de emergencia (terrestre/marítimo/aéreo),
cobertura geográfica de todo el Perú, mapa avanzado (leyenda, capas, iconografía por
tipo, filtros, resumen en popup/modal), vista pública sin autenticación con redacción
estricta de datos sensibles, personal y recursos logísticos por unidad, formularios
enriquecidos reutilizando el patrón `app-form-field` del MVP, rediseño CoreUI en tema
oscuro, dashboard avanzado, administración completa de usuarios/roles (5 roles nuevos
sobre Identity), y generación automática e idempotente de usuarios y datos demo.
Enfoque técnico: se mantiene el mismo monolito de dos capas (Angular + .NET Web API
sobre MySQL) del MVP — no se justifica microservicios ni un cambio de stack para este
alcance (Constitución, Restricciones Técnicas y de Calidad) — y se extiende el modelo
de datos y los roles de Identity ya existentes en vez de crear un sistema paralelo.

## Technical Context

**Language/Version**: TypeScript (Angular 20, standalone components/signals) en
frontend; C# / .NET 10 (SDK 10.0.100) en backend — mismas versiones que
`001-sige-mvp`.

**Primary Dependencies**:
- Backend: mismo backend de `001-sige-mvp` (Jason Taylor Clean Architecture,
  MediatR/CQRS, FluentValidation, ASP.NET Core Identity con `AddBearerToken`,
  EF Core 9.0.20 + Pomelo.EntityFrameworkCore.MySql 9.0.0, SignalR). Se añade
  `RoleManager`/`UserManager` de Identity para los 5 roles nuevos y un
  `IHostedService` (o extensión del `ApplicationDbContextInitialiser` ya
  existente) para el seed idempotente de usuarios/datos demo.
- Frontend: mismo frontend de `001-sige-mvp` (Angular + `@coreui/angular` +
  Leaflet/OSM). Se añade `@coreui/icons` (ya parte del ecosistema CoreUI) para la
  iconografía por tipo de emergencia/unidad (íconos, no solo color, requerido por
  FR-106), y el modo oscuro nativo de CoreUI (`data-coreui-theme="dark"`) para
  FR-128, en vez de un sistema de temas propio.

**Storage**: mismo MySQL existente en Docker usado por `001-sige-mvp` (contenedor
`mysql_local`, usuario dedicado `sige_app`). Se agregan tablas nuevas
(`TiposEmergencia`, `Ubicaciones` — o columnas embebidas en `Emergencias`/`Unidades`
según `data-model.md` —, `Personal`, `Recursos`) y una migración de datos (FR-103)
que vincula el `Tipo` de texto libre de cada `Emergencia` existente a un
`TipoEmergencia` del catálogo con el mismo nombre, creándolo si no existe.

**Testing**: mismo enfoque de `001-sige-mvp` — xUnit (`Application.UnitTests`,
`Application.FunctionalTests`) para reglas de negocio críticas (autorización por
rol ampliado, redacción de datos en la vista pública, idempotencia del seed,
migración de tipos) y Karma/Jasmine para componentes Angular nuevos. La
verificación de la vista pública (FR-114/FR-115/SC-105) se hace inspeccionando el
cuerpo completo de la respuesta HTTP, no solo el código de estado — lección
aprendida del bug de serialización encontrado en el MVP.

**Target Platform**: igual al MVP — aplicación web ejecutada localmente (frontend
puerto 4400, backend puerto 4401 en esta sesión de desarrollo).

**Project Type**: web application (mismos proyectos `frontend/` y `backend/` del
MVP, extendidos — no se crean proyectos nuevos).

**Performance Goals**: sin metas de carga productiva nuevas; el volumen objetivo de
datos demo (40–60 emergencias, 25–40 unidades, 60–100 personas, 100+ recursos, 10+
usuarios — sección 20 del documento de origen) debe seguir siendo instantáneo para
listar/filtrar/mapear en una demo de aula.

**Constraints**: mismas de la constitución (sin secretos versionados, reglas de
negocio solo en backend, validación estricta server-side, sin borrado físico) más
las nuevas explícitas del spec: el generador de datos/usuarios demo MUST ser
idempotente y MUST no ejecutarse en Production (FR-122–FR-124); la vista pública
MUST excluir siempre los campos de FR-114 aunque cambie el modelo interno.

**Scale/Scope**: 9 historias de usuario (US1–US9), 29 nuevos requisitos funcionales
(FR-101–FR-129 + FR-115a), 7 roles en total (2 del MVP + 5 nuevos), cobertura
geográfica nacional (sin restricción a una ciudad).

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principio (constitution.md) | Cómo se cumple en este plan |
|---|---|
| I. Arquitectura Limpia y Modular | Se extienden las mismas capas Domain/Application/Infrastructure/Web y `frontend/` del MVP; nuevas entidades (TipoEmergencia, Personal, Recurso) y nuevos endpoints siguen la misma convención de `IEndpointGroup` y CQRS ya establecida. |
| II. Reglas de Negocio en el Backend | La redacción de datos en la vista pública (FR-114/FR-115), la migración de tipos (FR-103) y la idempotencia del seed (FR-123) se implementan como queries/servicios de Application, nunca como filtrado en el frontend. |
| III. Validación Estricta de Entradas | Los formularios enriquecidos (US6) reutilizan FluentValidation server-side y el componente `app-form-field` ya validado en el MVP (FR-126). |
| IV. Trazabilidad y Auditoría | FR-119 extiende la auditoría existente a cuentas de usuario (último acceso, intentos fallidos, quién creó/modificó); no se elimina físicamente ningún registro nuevo (TipoEmergencia se desactiva, no se borra — FR-102). |
| V. Seguridad por Defecto y Mínimo Privilegio | Los 5 roles nuevos son estrictamente aditivos (Assumptions); la vista pública (US7) es la única superficie sin autenticación y se le aplica minimización explícita de datos (FR-114), no autorización implícita. Se reutiliza el `AuthorizationBehaviour` de MediatR — aplicado también a queries, corrigiendo el patrón que se rompió en el MVP y se corrigió en Converge. |
| VI. Calidad de Código y Manejo de Errores | Pruebas unitarias nuevas para: autorización de los 5 roles nuevos, redacción de datos públicos, idempotencia de seed, migración de tipos — siguiendo el mismo estándar de `AutorizacionQueriesTests.cs` del MVP. |
| VII. Verificabilidad y Alcance Acotado | Cada FR-10x/FR-11x/FR-12x es trazable a una historia de usuario priorizada (US1–US9); el MVP (FR-129, SC-106) se revalida como criterio de no regresión antes de Converge. |

**Resultado**: PASS. No se identifican violaciones que requieran justificación en
"Complexity Tracking".

## Project Structure

### Documentation (this feature)

```text
specs/002-ampliacion-operativa-nacional/
├── plan.md              # Este archivo (/speckit-plan)
├── research.md          # Fase 0 (/speckit-plan)
├── data-model.md         # Fase 1 (/speckit-plan)
├── quickstart.md         # Fase 1 (/speckit-plan)
├── contracts/            # Fase 1 (/speckit-plan) — REST nuevo/ampliado
└── tasks.md              # Fase 2 (/speckit-tasks) — no se crea aquí
```

### Source Code (repository root)

```text
backend/                                  # Mismo proyecto del MVP, extendido
├── src/
│   ├── Domain/
│   │   ├── Entities/                     # + TipoEmergencia, Personal, Recurso;
│   │   │                                  #   Emergencia/Unidad ganan Ubicacion enriquecida
│   │   └── Constants/Roles.cs            # + 5 roles nuevos
│   ├── Application/
│   │   ├── TiposEmergencia/              # Commands/Queries del catálogo (US3)
│   │   ├── Personal/                     # Commands/Queries (US5)
│   │   ├── Recursos/                     # Commands/Queries (US5)
│   │   ├── Usuarios/                     # Commands/Queries de administración (US1)
│   │   ├── Publico/                      # Queries de solo lectura para la vista pública (US7),
│   │   │                                  #   con DTOs propios que nunca incluyen campos de FR-114
│   │   └── Dashboard/                    # Queries ampliadas (US8)
│   ├── Infrastructure/
│   │   ├── Data/ApplicationDbContextInitialiser.cs  # + seed idempotente de roles/usuarios/datos demo (US2)
│   │   └── Migrations/                   # + migración de esquema y migración de datos (FR-103)
│   └── Web/
│       └── Endpoints/                    # + TiposEmergencia, Personal, Recursos, Usuarios, Publico (anónimo)
└── tests/
    ├── Application.UnitTests/            # + Publico/, Usuarios/, seed idempotente, migración de tipos
    └── Application.FunctionalTests/      # + endpoints públicos sin token, administración de usuarios

frontend/                                 # Mismo proyecto del MVP, extendido
├── src/app/
│   ├── views/
│   │   ├── usuarios/                     # Administración de usuarios/roles (US1)
│   │   ├── publico/                      # Vista pública sin login (US7), fuera del layout autenticado
│   │   ├── unidades/ , emergencias/      # Formularios enriquecidos + personal/recursos (US5, US6)
│   │   ├── mapa/                         # Leyenda, capas, filtros, iconografía (US4)
│   │   └── dashboard/                    # Indicadores ampliados (US8)
│   ├── core/models/                      # + tipo-emergencia, personal, recurso, usuario-ampliado
│   └── shared/                           # Reutiliza app-form-field (FR-126); tema oscuro vía CoreUI (US9)
```

**Structure Decision**: se mantiene la misma "Opción Web application" del MVP
(`frontend/` Angular + `backend/` .NET separados) — esta ampliación agrega
carpetas/entidades dentro de las capas ya existentes, sin introducir un tercer
proyecto ni microservicios, satisfaciendo el Principio VII (alcance acotado) y
evitando duplicar decisiones de arquitectura ya tomadas y validadas en
`001-sige-mvp/plan.md`.

## Complexity Tracking

> Sin violaciones de la Constitución que requieran justificación — tabla omitida.
