# Research: MVP SIGE (Fase 0)

Todas las incógnitas del Technical Context del plan se resuelven aquí antes de pasar
al diseño (Fase 1).

## 1. Motor de base de datos y manejo de ubicación geográfica

- **Decision**: **MySQL**, ya provisionado y en ejecución en un contenedor Docker del
  entorno del usuario (no se crea un servicio nuevo de base de datos en
  `docker-compose.yml`; el backend solo se conecta a ese contenedor existente vía
  variables de entorno). Acceso desde .NET con EF Core + proveedor
  `Pomelo.EntityFrameworkCore.MySql`. Ubicación como columnas `latitud`/`longitud`
  (`double`) simples, sin tipos `SPATIAL`/`POINT` de MySQL.
- **Rationale**: el usuario ya tiene MySQL corriendo en Docker para este proyecto, así
  que reutilizarlo evita infraestructura duplicada. El MVP no requiere consultas
  espaciales complejas (radio de búsqueda, intersección de polígonos); "unidad más
  cercana" puede resolverse en memoria con la fórmula de Haversine sobre un conjunto
  pequeño de unidades (decenas), suficiente para una demo en aula.
- **Alternatives considered**: PostgreSQL/PostGIS — descartado tras confirmar que el
  motor ya disponible es MySQL; tipos `SPATIAL` de MySQL con `ST_Distance_Sphere` — se
  descarta por complejidad innecesaria para el volumen de datos del MVP; se puede
  reconsiderar si el proyecto crece más allá del curso.

## 2. Estrategia de tiempo real

- **Decision**: SignalR con un único hub (`OperacionesHub`) que emite eventos
  `EmergenciaActualizada` y `UnidadActualizada` al backend notificar un cambio de
  estado, asignación o disponibilidad.
- **Rationale**: cubre exactamente los dos casos de la especificación (SC-002, SC-006:
  mapa y dashboard reflejando cambios sin recarga manual) sin necesidad de colas de
  mensajería externas.
- **Alternatives considered**: polling periódico desde el frontend — se descarta porque
  añade latencia perceptible y contradice el criterio de éxito "sin recargar
  manualmente"; WebSockets crudos — SignalR ya provee reconexión y fallback
  automáticos sobre WebSockets/SSE/long polling, evitando reinventar esa capa.

## 3. Autenticación y roles

- **Decision**: JWT emitido por el backend tras login con usuario/contraseña; el token
  incluye el claim de rol (`Operador` o `Supervisor`); los endpoints críticos
  (cierre/reapertura, administración de unidades) exigen el rol correspondiente vía
  `[Authorize(Roles = "Supervisor")]`.
- **Rationale**: cumple el Principio V (seguridad por defecto, mínimo privilegio) con
  la solución más simple soportada nativamente por ASP.NET Core, sin depender de un
  proveedor de identidad externo que exceda el alcance del MVP.
- **Alternatives considered**: OAuth2/OIDC con proveedor externo (Auth0, Azure AD) — se
  descarta por complejidad de configuración desproporcionada para un MVP de curso con
  usuarios demo predefinidos.

## 4. Concurrencia en la asignación de unidades (FR-007)

- **Decision**: verificación optimista en el momento de confirmar (transacción que
  relee el estado de la unidad dentro de la misma operación de escritura); si la unidad
  ya no está disponible, la operación falla con un error de negocio explícito que el
  frontend traduce en un mensaje al Operador.
- **Rationale**: es la decisión ya tomada en `/speckit-clarify` (sesión 2026-09-26);
  técnicamente se implementa con una comprobación de estado dentro de una transacción
  de base de datos (o un token de concurrencia/`RowVersion` en la entidad `Unidad`),
  sin locks distribuidos ni colas de reserva.
- **Alternatives considered**: bloqueo pesimista (reservar la unidad al abrir el
  formulario) — descartado explícitamente por el usuario en Clarify por mayor
  complejidad de UX (unidades "bloqueadas" sin confirmar) para el beneficio que aporta
  en un MVP de demostración.

## 5. Estructura del backend (Clean Architecture ligera)

- **Decision**: 4 proyectos .NET — `Sige.Domain`, `Sige.Application`,
  `Sige.Infrastructure`, `Sige.Api` — en una sola solución, sin separación en
  microservicios ni despliegues independientes.
- **Rationale**: satisface el Principio I (arquitectura limpia y modular) y el
  Principio VII (evitar complejidad innecesaria) simultáneamente: hay separación de
  responsabilidades pero un solo proceso desplegable.
- **Alternatives considered**: microservicios por dominio (emergencias, unidades,
  auditoría) — rechazado explícitamente por la guía y por la constitución salvo
  justificación clara, que no existe para el alcance de un MVP de fin de semana.

## Resumen

No quedan marcadores `NEEDS CLARIFICATION` en el Technical Context del plan; todas las
decisiones técnicas están resueltas y listas para la Fase 1 (data-model, contratos,
quickstart).
