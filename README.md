# SIGE — Sistema Integral de Gestión de Emergencias

Plataforma de despacho y monitoreo de emergencias a nivel nacional: registro
de emergencias, asignación de unidades de respuesta, seguimiento hasta el
cierre, mapa operativo en vivo, gestión de personal/recursos y una vista
pública sin necesidad de iniciar sesión.

Proyecto de curso construido íntegramente con **Spec-Driven Development
(SDD)**: cada decisión de negocio quedó escrita y aprobada antes de
programarse. Ver la sección [Spec-Driven Development](#spec-driven-development)
más abajo.

## Estructura del repositorio

| Carpeta / archivo | Contenido |
|---|---|
| [`backend/`](backend/) | API en .NET 10 (Clean Architecture) + MySQL. Ver [`backend/README.md`](backend/README.md). |
| [`frontend/`](frontend/) | SPA en Angular 20 + CoreUI. Ver [`frontend/README.md`](frontend/README.md). |
| [`specs/`](specs/) | Artefactos de Spec-Driven Development de cada feature (spec, plan, research, data-model, tasks, checklists). |
| [`docker-compose.yml`](docker-compose.yml) / [`.env.example`](.env.example) | Levanta el backend en contenedor, apuntando al MySQL ya corriendo en Docker. |
| [`usuarios_prueba.txt`](usuarios_prueba.txt) | Cuentas de demostración (usuario/contraseña/rol) para probar el sistema. |
| [`Presentacion_SIGE_SDD.pptx`](Presentacion_SIGE_SDD.pptx) | Presentación (8–10 min) de qué es SIGE y las 9 etapas de SDD aplicadas. |
| `backend/database/schema.sql` | Estructura de `SigeDb` (16 tablas), sin datos. |
| `backend/database/schema_con_datos.sql` | La misma base con datos de demostración sintéticos. |

## Features implementadas

1. **`001-sige-mvp`** — Registro de emergencias, unidades de respuesta,
   asignación, seguimiento de estados hasta el cierre, dashboard básico y
   una experiencia de uso consistente. 6 historias de usuario.
2. **`002-ampliacion-operativa-nacional`** — Administración de usuarios y
   roles ampliados (7 roles), generación automática de datos demo, catálogo
   nacional de tipos de emergencia (terrestre/marítimo/aéreo), mapa avanzado
   con capas y filtros, gestión de personal y recursos por unidad,
   formularios enriquecidos, vista pública sin autenticación, dashboard
   avanzado y tema oscuro. 9 historias de usuario.

Cada una vive en `specs/<feature>/` con su especificación, plan técnico,
modelo de datos, contratos de API, checklist de calidad y lista de tareas.

## Cómo correrlo localmente

1. **Base de datos**: MySQL 8.0 corriendo en Docker (contenedor `mysql_local`,
   puerto 3306, base `SigeDb`). El backend aplica migraciones y siembra datos
   demo automáticamente al arrancar en Desarrollo.
2. **Backend**: ver [`backend/README.md`](backend/README.md) — `dotnet run --project ./src/Web`
   (puerto 4401 en este proyecto de curso).
3. **Frontend**: ver [`frontend/README.md`](frontend/README.md) — `npm start`
   (puerto 4400).
4. **Vista pública** (sin login): `http://localhost:4400/publico`.
5. **Cuentas de prueba**: ver [`usuarios_prueba.txt`](usuarios_prueba.txt).

## Spec-Driven Development

El proyecto siguió, para cada feature, el flujo completo de 9 etapas:

`Constitución → Especificar → Aclarar → Planear → Checklist → Tareas → Analizar → Implementar → Converger`

`Constitución` (`.specify/memory/constitution.md`) fija los principios no
negociables una sola vez para todo el proyecto; las demás etapas se repiten
por feature. Las etapas `Analizar` y `Converger` son de verificación —
comparan el código real contra la especificación— y en este proyecto
encontraron vulnerabilidades reales de autorización antes de la entrega,
documentadas en `specs/*/tasks.md` (fases de Convergencia).
