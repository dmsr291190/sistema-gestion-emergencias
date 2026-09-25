# Data Model: Ampliación Operativa Nacional de SIGE

Extiende el modelo de `001-sige-mvp/data-model.md`. Las entidades del MVP
(`Emergencia`, `UnidadRespuesta`, `Asignacion`, `EventoAuditoria`) se conservan sin
romper compatibilidad (FR-129); se agregan columnas y entidades nuevas.

## Entidades nuevas

### TipoEmergencia

Catálogo administrable que reemplaza el texto libre `Emergencia.Tipo` (FR-101).

| Campo | Tipo | Notas |
|---|---|---|
| Id | int (PK, autonumérico) | |
| Nombre | string, requerido, único | |
| Ambito | enum: Terrestre / Marítimo / Aéreo / Mixto | FR-105 |
| Icono | string | nombre de ícono `@coreui/icons` (Research §4) |
| Color | string (hex) | |
| PrioridadPorDefecto | enum: Baja/Media/Alta/Crítica (mismo enum del MVP) | editable al registrar (FR-101) |
| Activo | bool, default true | FR-102: desactivar no borra ni afecta emergencias existentes |

### Ubicación (Value Object, embebido en Emergencia y UnidadRespuesta)

No es una tabla propia — es un conjunto de columnas embebidas (owned type de EF
Core) reutilizado en `Emergencia` y `UnidadRespuesta`, igual que ya se hace con
`Latitud`/`Longitud` en el MVP.

| Campo | Tipo | Notas |
|---|---|---|
| Departamento | string, nullable | |
| Provincia | string, nullable | |
| Distrito | string, nullable | |
| CentroPoblado | string, nullable | |
| Direccion | string, nullable | |
| Referencia | string, nullable | |
| Latitud | decimal | requerido |
| Longitud | decimal | requerido |
| Ambito | enum: Terrestre/Marítimo/Aéreo/Mixto | FR-105 — **corregido tras Analyze (I1)**: debe tener los mismos 4 valores que `TipoEmergencia.Ambito`, no solo 3 |
| SinDireccionFormal | bool, default false | **agregado tras Analyze (I2)** — ver regla debajo |

Regla (FR-104, corregida tras Analyze — I2): Departamento/Provincia/Distrito son
requeridos **salvo que `SinDireccionFormal == true`** — un flag explícito que el
usuario marca al registrar (por ejemplo, en zonas marítimas o terrestres remotas sin
dirección formal), en vez de inferirlo del `Ambito`. Esto evita que una emergencia
terrestre en una zona remota (ámbito Terrestre, pero sin dirección formal) quede
forzada a dar un distrito que no aplica. Latitud/Longitud siempre son requeridas,
independientemente de `SinDireccionFormal`.

### Institución

Catálogo único y compartido de organismos participantes (bomberos, policía,
salud, marina, etc.) — decisión de Diego tras el Checklist (CHK038): una sola
tabla referenciada por `Personal`, `UnidadRespuesta` (dueña de sus recursos) y
`Usuario`, no un catálogo por entidad.

| Campo | Tipo | Notas |
|---|---|---|
| Id | int (PK) | |
| Nombre | string, requerido, único | |
| Activo | bool, default true | |

### Personal

| Campo | Tipo | Notas |
|---|---|---|
| Id | int (PK) | |
| Nombres, Apellidos | string, requerido | |
| Documento | string, requerido | |
| InstitucionId | int (FK a Institución) | |
| Especialidad | string | |
| Funcion | string | |
| Certificaciones | string, nullable | texto libre o lista separada por comas |
| Disponible | bool, default true | |
| UnidadRespuestaId | int (FK) | |

### Recurso

| Campo | Tipo | Notas |
|---|---|---|
| Id | int (PK) | |
| Codigo | string, requerido | |
| Nombre | string, requerido | |
| Categoria | enum: Equipos/Herramientas/Víveres/Líquidos/Estructuras/MaterialMédico/EquipoRescate | |
| UnidadMedida | string | |
| Cantidad | int | total nominal |
| CantidadDisponible | int | FR-112: alerta si < CantidadMinima |
| CantidadMinima | int | |
| UnidadRespuestaId | int (FK) | |

## Entidades del MVP — cambios

### Emergencia

- **+ `TipoEmergenciaId`** (FK, nullable durante la migración, luego requerido) —
  reemplaza el uso de `Tipo` (texto libre) como fuente de verdad; `Tipo` se
  conserva como columna legada de solo lectura para trazabilidad, no se borra
  (Principio IV).
- **+ `Ubicacion`** (owned type) — reemplaza los campos sueltos de
  lat/lon del MVP, agregando departamento/provincia/distrito/etc.
- **+ `Afectados`, `Heridos`, `Desaparecidos`, `Fallecidos`, `Evacuados`** (int,
  default 0) — FR-125.

### UnidadRespuesta

- **+ `Ubicacion`** (owned type, **nullable/aditivo** — decisión tomada en
  Implement: a diferencia de `Emergencia`, aquí `Ubicacion` NO reemplaza
  `Latitud`/`Longitud` porque esos campos representan la posición actual de la
  unidad, actualizada con frecuencia vía `CambiarEstadoOperativoUnidad`;
  `Ubicacion` solo agrega los campos de dirección/ámbito cuando se necesitan,
  por ejemplo para su base).
- **+ `InstitucionId`** (FK a Institución) — organismo dueño de la unidad
  (bomberos, policía, salud, marina, etc.); se muestra en tooltip/popup del mapa
  (sección 7.2 del documento de origen) y en el detalle de la unidad.
- **+ `UsuarioId`** (FK nullable a `AspNetUsers`) — vínculo 1-a-1 con la cuenta
  de login propia cuando el rol es "Unidad de respuesta" (Research §2).
- **+ colección `Personal`** (1-a-muchos).
- **+ colección `Recursos`** (1-a-muchos).

### Usuario (AspNetUsers, vía Identity — no es una tabla nueva)

- **+ `InstitucionId`** (FK nullable a Institución) — organismo al que pertenece
  el usuario (mismo catálogo compartido, CHK038).
- **+ `UltimoAcceso`** (DateTime, nullable).
- **+ `IntentosFallidos`** (int) — ya soportado nativamente por
  `AccessFailedCount` de Identity; se expone en el DTO de administración.
- **+ `RequiereCambioPassword`** (bool, default false) — FR-118.
- **+ `CreadoPor`, `ModificadoPor`** (string, FK a `AspNetUsers.Id`) — FR-119.

## Migración de datos (FR-103)

Ver Research §5. Resumen del flujo dentro del initialiser existente:

```text
Para cada Emergencia con TipoEmergenciaId == null:
  tipo = TipoEmergencia.FirstOrDefault(t => t.Nombre == emergencia.Tipo) (case-insensitive)
  si tipo == null:
    tipo = crear TipoEmergencia { Nombre = emergencia.Tipo, Ambito = Terrestre, PrioridadPorDefecto = Media, Activo = true }
  emergencia.TipoEmergenciaId = tipo.Id
```

## CapaDelMapa (no persistida)

No es una entidad de base de datos — es una enumeración fija en el frontend
(`emergencias`, `unidades`, `bases`, `hospitales`, `puertos`, `rutas`,
`histórico`, FR-107) usada para agrupar marcadores visibles/ocultos en el mapa;
las "bases", "hospitales" y "puertos" se modelan como un catálogo simple de
puntos de interés estáticos (sin flujo de administración propio en esta
ampliación, fuera de alcance salvo que Diego lo pida).

## Reglas de validación nuevas (FluentValidation, server-side)

- `TipoEmergencia.Nombre`: requerido, único, máx. 100 caracteres.
- `Ubicacion`: Latitud/Longitud siempre requeridas y dentro de rango válido
  (-90..90 / -180..180); Departamento/Provincia/Distrito requeridos salvo que
  `SinDireccionFormal == true` (FR-104) — **no** se infiere de `Ambito` (corregido
  tras Analyze, hallazgo I2).
- `Recurso.CantidadDisponible <= Recurso.Cantidad`.
- `Personal.Documento`: requerido, máximo 20 caracteres (columna `varchar(20)`)
  — **corrección tras Analyze (hallazgo U2)**: la referencia original a "el
  mismo formato que el registro de usuarios del MVP" era incorrecta, el MVP
  nunca validó ni tuvo un campo de documento de identidad para usuarios
  (`AspNetUsers` solo usa `Email`/`UserName`); no se define una expresión
  regular de formato porque el Perú usa varios tipos de documento (DNI, RUC,
  carné de extranjería) sin un patrón único.
