# Implementation Plan: MVP Sistema Integral de Gestión de Emergencias (SIGE)

**Branch**: `001-sige-mvp` | **Date**: 2026-09-26 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-sige-mvp/spec.md`

## Summary

Construir un MVP demostrable de un centro de despacho de emergencias: registrar
emergencias georreferenciadas, verlas en un mapa junto a las unidades de respuesta
(ambulancia, bomberos, patrullero), asignarles una o varias unidades con progreso de
estado independiente por unidad, actualizar en tiempo real mapa y dashboard, y cerrar
el incidente conservando trazabilidad completa. Enfoque técnico: monolito simple de
dos capas (Angular + .NET Web API) sobre MySQL (ya provisionado en Docker), con
SignalR para push en tiempo real; se evitan microservicios porque el alcance del MVP
no los justifica
(Constitución, Restricciones Técnicas y de Calidad). Para acelerar un MVP de curso, el
backend se escafolda a partir de la plantilla **Clean Architecture de Jason Taylor**
(`dotnet new ca-sln`) y el frontend adopta **CoreUI para Angular** como kit de UI/admin
en lugar de construir el layout y los widgets desde cero.

## Technical Context

**Language/Version**: TypeScript (Angular 18+) en frontend; C# / .NET 8 en backend.

**Primary Dependencies**:
- Backend: plantilla **Jason Taylor Clean Architecture** para .NET (ASP.NET Core Web
  API + MediatR para CQRS, FluentValidation, AutoMapper, Entity Framework Core),
  SignalR (tiempo real), autenticación JWT.
- Frontend: Angular, **CoreUI para Angular** (`@coreui/angular`) como kit de
  layout/admin (sidebar, tablas, widgets de dashboard, gráficos), Leaflet +
  OpenStreetMap (mapa) integrado dentro del layout de CoreUI.

**Storage**: MySQL, ya provisionado y en ejecución en Docker en el entorno del usuario
(no se levanta una instancia nueva vía docker-compose; el backend se conecta a ese
contenedor existente mediante variables de entorno). Sin extensión espacial: si el MVP
necesita "unidades más cercanas" se resuelve con lat/lon + cálculo de distancia en
memoria (Haversine), no con tipos `SPATIAL`/`POINT` de MySQL.

**Testing**: xUnit + integración con base de datos en memoria/contenedor para el
backend; pruebas unitarias de componentes/servicios Angular; un flujo de pruebas de
extremo a extremo cubre el recorrido mínimo demostrable (sección 16 de la guía).

**Target Platform**: aplicación web ejecutada localmente vía Docker Compose (navegador
de escritorio para la demo en aula).

**Project Type**: web application (frontend Angular + backend .NET Web API).

**Performance Goals**: sin metas de carga productiva; suficiente con responder
interacciones de un operador en tiempo perceptible como instantáneo (acorde a SC-002,
SC-006 de la especificación) para una demo con datos de volumen reducido (decenas de
emergencias/unidades, no miles).

**Constraints**: sin secretos versionados (variables de entorno); reglas de negocio
críticas solo en backend (Constitución, Principio II); validación estricta server-side
(Principio III); no eliminar físicamente emergencias (Principio IV).

**Scale/Scope**: alcance de MVP académico — decenas de emergencias y unidades de
prueba, 2 roles (Operador, Supervisor), sin multi-tenant ni alta disponibilidad.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principio (constitution.md) | Cómo se cumple en este plan |
|---|---|
| I. Arquitectura Limpia y Modular | Separación explícita `frontend/` (Angular + CoreUI) y `backend/` (.NET) con capas Domain / Application / Infrastructure / Web, ya impuestas por la plantilla Jason Taylor. |
| II. Reglas de Negocio en el Backend | Transiciones de estado, reglas de asignación y revalidación de disponibilidad (FR-007, FR-016) viven en Commands/Queries de MediatR dentro de Application; el frontend (CoreUI) solo consume la API. |
| III. Validación Estricta de Entradas | Validación server-side con FluentValidation (parte de la plantilla) en cada Command de creación de emergencia y asignación (FR-015). |
| IV. Trazabilidad y Auditoría | Entidad `EventoAuditoria`/timeline persistida, sin borrado físico de `Emergencia` (soft-state, nunca DELETE). |
| V. Seguridad por Defecto y Mínimo Privilegio | JWT + roles `Operador`/`Supervisor`; endpoints de cierre/reapertura (FR-017) protegidos con autorización por rol. |
| VI. Calidad de Código y Manejo de Errores | Pruebas unitarias sobre reglas críticas (asignación, transiciones); manejo de errores centralizado (middleware de excepciones) en la API. |
| VII. Verificabilidad y Alcance Acotado | Estructura simple de 2 proyectos (frontend/backend), sin microservicios; cada requisito FR-* es trazable a un endpoint o componente concreto en las secciones siguientes. |

**Resultado**: PASS. No se identifican violaciones que requieran justificación en
"Complexity Tracking".

## Project Structure

### Documentation (this feature)

```text
specs/001-sige-mvp/
├── plan.md              # Este archivo (/speckit-plan)
├── research.md          # Fase 0 (/speckit-plan)
├── data-model.md         # Fase 1 (/speckit-plan)
├── quickstart.md         # Fase 1 (/speckit-plan)
├── contracts/            # Fase 1 (/speckit-plan) — REST + eventos SignalR
└── tasks.md              # Fase 2 (/speckit-tasks) — no se crea aquí
```

### Source Code (repository root)

```text
backend/                            # Escafoldado con la plantilla Jason Taylor (dotnet new ca-sln)
├── src/
│   ├── Domain/                # Entidades y reglas de negocio (Emergencia, Unidad, Asignacion)
│   ├── Application/           # CQRS (MediatR): Commands/Queries, FluentValidation, AutoMapper
│   ├── Infrastructure/        # EF Core (Pomelo.EntityFrameworkCore.MySql), repositorios, migraciones
│   └── Web/                   # Controllers REST, SignalR hub, autenticación JWT (nombre de
│                               # proyecto según convención del template: "Web", no "Api")
└── tests/
    ├── Domain.UnitTests/
    ├── Application.UnitTests/     # Reglas críticas: asignación, transiciones, roles
    └── Application.FunctionalTests/ # Endpoints principales end-to-end contra BD de prueba

frontend/                           # Angular + CoreUI (@coreui/angular)
├── src/
│   ├── app/
│   │   ├── views/
│   │   │   ├── auth/               # Login por rol
│   │   │   ├── emergencias/        # Registro, detalle, timeline
│   │   │   ├── unidades/           # Administración y disponibilidad
│   │   │   ├── despacho/           # Asignación de unidades
│   │   │   ├── mapa/               # Mapa operativo (Leaflet + OSM dentro del layout CoreUI)
│   │   │   └── dashboard/          # Indicadores con widgets/gráficos de CoreUI
│   │   ├── layout/                 # DefaultLayoutComponent + sidebar (_nav.ts) de CoreUI
│   │   ├── core/                   # Servicios API, guards de rol, cliente SignalR
│   │   └── shared/                 # Componentes reutilizables propios del proyecto
└── tests/                          # Pruebas unitarias de componentes/servicios

docker-compose.yml                  # Orquesta frontend + backend; se conecta al MySQL
                                     # ya existente en Docker (no define un nuevo servicio de BD)
.env.example                        # Variables de entorno sin secretos reales (incluye
                                     # cadena de conexión al MySQL existente)
```

**Structure Decision**: Opción "Web application" (frontend Angular + backend .NET Web
API separados), tal como pide la guía. El backend usa la plantilla Jason Taylor, que ya
impone Clean Architecture (`Domain` → `Application` → `Infrastructure` → `Web`) con
CQRS vía MediatR, satisfaciendo el Principio I de la constitución sin introducir
microservicios (Principio VII). El frontend usa CoreUI para Angular como kit de UI/admin
(layout, sidebar, tablas y widgets de dashboard ya construidos), reduciendo el trabajo
de UI propio a los flujos específicos del dominio (mapa, despacho, timeline).

## Complexity Tracking

> Sin violaciones de la Constitución que requieran justificación — tabla omitida.
