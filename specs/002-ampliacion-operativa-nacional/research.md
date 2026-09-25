# Phase 0 Research: Ampliación Operativa Nacional de SIGE

No quedaron `NEEDS CLARIFICATION` en el Technical Context del plan (se reutiliza el
stack ya decidido y validado en `001-sige-mvp/plan.md` y `research.md`). Este
documento resuelve las decisiones técnicas nuevas específicas de esta ampliación.

## 1. Modelo de roles ampliado

**Decision**: los 5 roles nuevos (Administrador, Coordinador logístico, Jefe de
unidad, Unidad de respuesta, Visualizador) se agregan como roles adicionales de
ASP.NET Core Identity (`RoleManager<IdentityRole>`), en el mismo mecanismo que ya
usan Operador/Supervisor (`src/Domain/Constants/Roles.cs`), y se autorizan con el
mismo `AuthorizationBehaviour` de MediatR vía `[Authorize(Roles = "...")]` en
Commands/Queries — aplicado también a queries, corrigiendo explícitamente el patrón
que en el MVP solo se aplicaba a Commands (hallazgo de Converge).

**Rationale**: reutilizar el mecanismo ya probado evita introducir un segundo
sistema de autorización; Identity soporta de forma nativa múltiples roles por
usuario sin cambios de esquema adicionales.

**Alternatives considered**: tabla de permisos granular (RBAC completo con
permisos individuales) — rechazada por sobredimensionar el alcance de esta
ampliación (Constitución, Principio VII); los 7 roles fijos ya cubren los
escenarios de aceptación del spec.

## 2. Login propio del rol "Unidad de respuesta"

**Decision**: una unidad de respuesta es un usuario de Identity más, vinculado
1-a-1 a un registro `UnidadRespuesta` (nueva columna `UsuarioId` nullable en
`UnidadRespuesta`). Al iniciar sesión con ese rol, el frontend restringe la
navegación a una pantalla mínima de "mi unidad" (estado operativo + posición);
el backend sigue permitiendo que Operador/Supervisor actualicen cualquier unidad
(FR-120 es aditivo, no reemplaza el flujo existente del MVP).

**Rationale**: responde directamente a la Clarification confirmada por Diego
(la unidad se autentica ella misma desde un dispositivo propio, sin quitarle esa
capacidad a Operador/Supervisor).

**Alternatives considered**: cuenta compartida por tipo de unidad (una sola
cuenta "unidad" genérica) — rechazada porque no permite atribuir en la auditoría
qué unidad concreta actualizó su propio estado (Principio IV).

## 3. Redacción de ubicación en la vista pública

**Decision**: la vista pública consulta un DTO propio (`Application/Publico/`)
que nunca expone `Latitud`/`Longitud` exactas ni los campos listados en FR-114;
en su lugar devuelve el nombre del distrito/centro poblado y un centroide
aproximado (coordenada del centro del distrito, no la reportada) calculado una
sola vez por distrito, no por emergencia individual. Los endpoints autenticados
(`/api/Emergencias`, mapa interno) siguen devolviendo la coordenada exacta
(FR-115a), sin cambios respecto al MVP.

**Rationale**: usar un DTO y una query completamente separados (en vez de un
parámetro `esPublico` sobre el mismo endpoint) hace estructuralmente imposible
que un cambio futuro filtre coordenadas exactas por accidente — el mismo tipo de
bug de "campo sensible que se cuela" que ya se encontró una vez en el MVP
(serialización de `ValidationProblemDetails`).

**Alternatives considered**: redondear lat/lon con menos decimales — rechazada
porque redondear coordenadas numéricas todavía puede acercarse demasiado a un
punto exacto en zonas de baja densidad (un distrito rural grande); anclar al
centroide del distrito es más consistente con "aproximado a nivel de
distrito/centro poblado" tal como lo pidió Diego.

## 4. Catálogo de tipos de emergencia e iconografía

**Decision**: `TipoEmergencia` es una tabla administrable (nombre, ámbito, ícono,
color, prioridad por defecto, activo). El campo `ícono` almacena el nombre de un
ícono de `@coreui/icons` (ya parte del stack de CoreUI del MVP, funciona en tema
claro/oscuro) en vez de subir archivos SVG/PNG propios.

**Rationale**: `@coreui/icons` ya viene incluido en el ecosistema CoreUI elegido
en el MVP, cubre los ejemplos de la sección 6 del documento de origen (fuego,
cruz médica, vehículo, ola, montaña, etc.) con buen contraste en ambos temas, y
evita construir un pipeline de subida/gestión de archivos binarios fuera del
alcance del spec.

**Alternatives considered**: subir archivos de ícono personalizados por tipo —
rechazada por complejidad innecesaria (almacenamiento de archivos, validación de
formato) frente al beneficio para una demo académica.

## 5. Migración del campo `Tipo` existente (FR-103)

**Decision**: la migración se ejecuta como parte del `ApplicationDbContextInitialiser`
existente (el mismo componente que ya siembra datos del MVP), en un paso que:
1. Lee los valores distintos de `Emergencia.Tipo` (texto libre) que aún no
   tengan `TipoEmergenciaId`.
2. Para cada valor, busca un `TipoEmergencia` con ese nombre (case-insensitive);
   si no existe, lo crea con ámbito `Terrestre` y prioridad `Media` por defecto
   (editable después por un Administrador).
3. Vincula la emergencia a ese `TipoEmergenciaId`.

Es idempotente: si una emergencia ya tiene `TipoEmergenciaId`, se omite.

**Rationale**: responde a la Clarification confirmada por Diego (migración
automática por coincidencia de nombre, creando el tipo si no existe) y reutiliza
el mismo mecanismo de inicialización ya usado y probado en el MVP en vez de un
script de migración de datos separado.

**Alternatives considered**: dejar `Tipo` como texto libre y agregar
`TipoEmergenciaId` solo para emergencias nuevas — rechazada explícitamente por
Diego en la Clarification (quiere que las emergencias del MVP también queden
vinculadas al catálogo).

## 6. Generación automática e idempotente de usuarios/datos demo

**Decision**: se extiende `ApplicationDbContextInitialiser.SeedAsync` (ya existe
desde el MVP para 2 usuarios/3 unidades) para, condicionado a
`env.IsDevelopment() || env.EnvironmentName is "Demo" or "Testing"`, crear los 10
usuarios demo documentados en la sección 19.1 del documento de origen (uno por
rol nuevo, más los ya existentes de Operador/Supervisor) si no existen
(`UserManager.FindByNameAsync` antes de `CreateAsync`), y un volumen de datos
demo (40–60 emergencias, 25–40 unidades, 60–100 personal, 100+ recursos —
sección 20) generado solo si la tabla correspondiente está vacía. La contraseña
demo se lee de la variable de entorno `SIGE_DEMO_PASSWORD` (default
`DemoSige#2026` solo si la variable no está definida, nunca hardcodeada como
único valor posible).

**Rationale**: reutilizar el inicializador ya existente (en vez de un
`IHostedService` nuevo) mantiene un único punto de siembra de datos, ya probado
en el arranque del MVP; los chequeos de existencia antes de crear garantizan
idempotencia (FR-123) sin necesitar una tabla de control adicional.

**Alternatives considered**: un endpoint HTTP manual `/api/seed` para
regenerar la demo bajo demanda — rechazada como fuera de alcance del spec (que
pide generación automática al arrancar, no un endpoint operable); se puede
agregar en una ampliación posterior si Diego lo pide explícitamente.

## 7. Tema oscuro (FR-128)

**Decision**: se usa el mecanismo nativo de temas de CoreUI
(`data-coreui-theme="dark"` en el elemento raíz + las variables CSS de CoreUI ya
preparadas para modo oscuro), con un toggle simple en el layout, en vez de un
sistema de theming propio con variables CSS nuevas.

**Rationale**: CoreUI para Angular (ya adoptado en el MVP) trae soporte de tema
oscuro incorporado y probado contra sus propios componentes (tablas, cards,
modales, widgets), lo que minimiza el riesgo de inconsistencias de contraste
mencionado en el spec (US9, FR-128) frente a construir un tema desde cero.

**Alternatives considered**: variables CSS propias sobre los componentes CoreUI
— rechazada porque duplicaría trabajo ya resuelto por la librería y aumentaría
el riesgo de romper el contraste de los colores de prioridad/estado ya definidos
en el MVP.
