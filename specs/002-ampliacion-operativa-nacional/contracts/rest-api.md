# Contrato REST: Ampliación Operativa Nacional (Fase 1)

Extiende `001-sige-mvp/contracts/rest-api.md`. Mismos endpoints del MVP siguen
vigentes sin cambios de ruta (FR-129). Convención igual: prefijo `/api/{Clase}`
(`IEndpointGroup`), bearer token de Identity, `[Rol]` = `[Authorize(Roles = "...")]`
aplicado en el Command/Query de MediatR (Application), **incluidas las queries**
(corrección aplicada en Converge del MVP, no repetir el error en esta ampliación).

## Catálogo de instituciones (`/api/Instituciones`) — prerrequisito de US1, US5

**Agregado tras Analyze (hallazgo C2)**: `Personal`, `UnidadRespuesta` y `Usuario`
referencian `InstitucionId`, pero no existía un endpoint para listarlas.

| Método | Ruta | Descripción | Requisitos |
|---|---|---|---|
| GET | `/api/Instituciones` | Lista el catálogo de instituciones (autenticado); usado para poblar los selectores de Usuario, Personal y Unidad. | Key Entity "Institución" |

## Catálogo de tipos de emergencia (`/api/TiposEmergencia`) — US3

| Método | Ruta | Descripción | Requisitos |
|---|---|---|---|
| POST | `/api/TiposEmergencia` | **[Administrador]** Crea un tipo (nombre, ámbito, ícono, color, prioridad por defecto). | FR-101 |
| PATCH | `/api/TiposEmergencia/{id}` | **[Administrador]** Edita o activa/desactiva un tipo. | FR-101, FR-102 |
| GET | `/api/TiposEmergencia` | Lista tipos (autenticado); `?soloActivos=true` para el formulario de registro. | FR-102 |

## Emergencias — cambios sobre el MVP — US3, US6

| Método | Ruta | Cambio | Requisitos |
|---|---|---|---|
| POST | `/api/Emergencias` | Body ahora incluye `tipoEmergenciaId`, `ubicacion` (`sinDireccionFormal: bool`; si es `false`, departamento/provincia/distrito son requeridos, si es `true` solo lat/lon; ámbito con 4 valores incl. Mixto — corregido tras Analyze I1/I2), `afectados/heridos/desaparecidos/fallecidos/evacuados`. | FR-104, FR-105, FR-125 |
| GET | `/api/Emergencias`, `/api/Emergencias/{id}` | Respuesta incluye el tipo del catálogo (no solo texto) y la ubicación completa. Solo para usuarios autenticados — siguen viendo coordenada exacta (FR-115a). | FR-101, FR-115a |

## Personal (`/api/Unidades/{unidadId}/personal`) — US5

| Método | Ruta | Descripción | Requisitos |
|---|---|---|---|
| POST | `/api/Unidades/{unidadId}/personal` | **[Coordinador logístico, Jefe de unidad]** Agrega personal a una unidad. | FR-110 |
| GET | `/api/Unidades/{unidadId}/personal` | Lista personal de la unidad (autenticado). | FR-110 |

## Recursos (`/api/Unidades/{unidadId}/recursos`) — US5

| Método | Ruta | Descripción | Requisitos |
|---|---|---|---|
| POST | `/api/Unidades/{unidadId}/recursos` | **[Coordinador logístico, Jefe de unidad]** Agrega un recurso con cantidad/mínimo. | FR-111 |
| PATCH | `/api/Unidades/{unidadId}/recursos/{id}` | **[Coordinador logístico, Jefe de unidad]** Actualiza cantidad disponible. | FR-111, FR-112 |
| GET | `/api/Unidades/{unidadId}/recursos` | Lista recursos; cada ítem incluye `bajoStock: bool` (`cantidadDisponible < cantidadMinima`). | FR-112 |

## Administración de usuarios (`/api/Usuarios`) — US1

| Método | Ruta | Descripción | Requisitos |
|---|---|---|---|
| POST | `/api/Usuarios` | **[Administrador]** Crea usuario con rol(es). | FR-117, FR-118 |
| PATCH | `/api/Usuarios/{id}` | **[Administrador]** Edita datos/roles. | FR-118 |
| POST | `/api/Usuarios/{id}/bloquear` | **[Administrador]** Bloquea la cuenta. | FR-118 |
| POST | `/api/Usuarios/{id}/desbloquear` | **[Administrador]** Desbloquea la cuenta. | FR-118 |
| POST | `/api/Usuarios/{id}/forzar-cambio-password` | **[Administrador]** Marca `RequiereCambioPassword`. | FR-118 |
| GET | `/api/Usuarios` | **[Administrador]** Lista usuarios con rol, estado, último acceso, intentos fallidos. | FR-119 |

**Error de negocio nuevo** (`403`, ya cubierto por el pipeline de autorización
existente, sin nuevo código): cualquier rol distinto de Administrador que llame a
`/api/Usuarios/*` recibe `403 Forbidden` (FR-116).

## Vista pública (`/api/Publico/*`) — US7, sin autenticación

DTOs propios (`Application/Publico/`), nunca reutilizan el DTO interno de
`Emergencia` — ver `research.md` §3.

| Método | Ruta | Descripción | Requisitos |
|---|---|---|---|
| GET | `/api/Publico/Emergencias` | Lista emergencias activas: código público, tipo, prioridad, estado, ubicación aproximada (distrito/centro poblado + centroide), última actualización. Excluye siempre los campos de FR-114. | FR-113, FR-114, FR-115 |
| GET | `/api/Publico/Emergencias/{codigo}` | Resumen + línea de tiempo pública (solo transiciones de estado, sin usuarios internos ni observaciones). Identificador inexistente o fuera de alcance público responde `404` genérico (Edge Case del spec: no revelar por enumeración). | FR-113, FR-114 |

**Verificación obligatoria (SC-105)**: cada endpoint público se prueba con `curl`
sin header `Authorization`, inspeccionando el **cuerpo completo** de la respuesta
(no solo el status code) para confirmar ausencia real de los campos de FR-114 —
mismo método que descubrió el bug de serialización en el MVP.

## Dashboard — cambios sobre el MVP — US8

| Método | Ruta | Cambio | Requisitos |
|---|---|---|---|
| GET | `/api/Dashboard/indicadores` | Respuesta agrega `emergenciasPorAmbito` (terrestre/marítimo/aéreo), `tiempoPromedioAtencionMinutos`, `personalDesplegado`, `recursosMovilizados`. | FR-127 |

## Eventos SignalR — sin cambios

Mismo hub `/hubs/operaciones` y eventos `EmergenciaActualizada`/`UnidadActualizada`
del MVP; no se agregan eventos nuevos en esta ampliación (fuera de alcance salvo
que una historia lo requiera explícitamente en Tasks).
