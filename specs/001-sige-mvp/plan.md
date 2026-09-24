# Implementation Plan: MVP Sistema Integral de Gestión de Emergencias (SIGE)

**Branch**: `001-sige-mvp` | **Date**: 2026-09-26 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-sige-mvp/spec.md`

## Summary

Construir un MVP demostrable de un centro de despacho de emergencias: registrar
emergencias georreferenciadas, verlas en un mapa junto a las unidades de respuesta
(ambulancia, bomberos, patrullero), asignarles una o varias unidades con progreso de
estado independiente por unidad, actualizar en tiempo real mapa y dashboard, y cerrar
el incidente conservando trazabilidad completa. Enfoque técnico: monolito simple de
dos capas (Angular + .NET Web API) sobre PostgreSQL, con SignalR para push en tiempo
real; se evitan microservicios porque el alcance del MVP no los justifica
(Constitución, Restricciones Técnicas y de Calidad).

## Technical Context

**Language/Version**: TypeScript (Angular 18+) en frontend; C# / .NET 8 en backend.

**Primary Dependencies**: Angular, Leaflet + OpenStreetMap (mapa); ASP.NET Core Web
API, Entity Framework Core, SignalR (tiempo real), autenticación JWT.

**Storage**: PostgreSQL. Se evalúa PostGIS solo si una consulta geográfica concreta lo
requiere (p. ej. "unidades más cercanas"); si el MVP puede resolverse con
lat/lon + cálculo simple de distancia, no se agrega PostGIS.

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
| I. Arquitectura Limpia y Modular | Separación explícita `frontend/` (Angular) y `backend/` (.NET) con capas Domain / Application / Infrastructure / Api dentro del backend. |
| II. Reglas de Negocio en el Backend | Transiciones de estado, reglas de asignación y revalidación de disponibilidad (FR-007, FR-016) viven en la capa Application del backend; el frontend solo consume la API. |
| III. Validación Estricta de Entradas | Validación server-side con FluentValidation o Data Annotations en los endpoints de creación de emergencia y asignación (FR-015). |
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
backend/
├── src/
│   ├── Sige.Domain/           # Entidades y reglas de negocio (Emergencia, Unidad, Asignacion)
│   ├── Sige.Application/      # Casos de uso, validaciones, orquestación de transiciones
│   ├── Sige.Infrastructure/   # EF Core, repositorios, PostgreSQL, migraciones
│   └── Sige.Api/              # Controllers REST, SignalR hub, autenticación JWT
└── tests/
    ├── Sige.UnitTests/        # Reglas críticas: asignación, transiciones, roles
    └── Sige.IntegrationTests/ # Endpoints principales end-to-end contra BD de prueba

frontend/
├── src/
│   ├── app/
│   │   ├── features/
│   │   │   ├── auth/               # Login por rol
│   │   │   ├── emergencias/        # Registro, detalle, timeline
│   │   │   ├── unidades/           # Administración y disponibilidad
│   │   │   ├── despacho/           # Asignación de unidades
│   │   │   ├── mapa/               # Mapa operativo (Leaflet + OSM)
│   │   │   └── dashboard/          # Indicadores
│   │   ├── core/                   # Servicios API, guards de rol, cliente SignalR
│   │   └── shared/                 # Componentes UI reutilizables
└── tests/                          # Pruebas unitarias de componentes/servicios

docker-compose.yml                  # Orquesta frontend + backend + PostgreSQL local
.env.example                        # Variables de entorno sin secretos reales
```

**Structure Decision**: Opción "Web application" (frontend Angular + backend .NET Web
API separados), tal como pide la guía. Dentro del backend se aplica una separación tipo
Clean Architecture (`Domain` → `Application` → `Infrastructure`/`Api`) para satisfacer
el Principio I de la constitución sin introducir microservicios (Principio VII).

## Complexity Tracking

> Sin violaciones de la Constitución que requieran justificación — tabla omitida.
