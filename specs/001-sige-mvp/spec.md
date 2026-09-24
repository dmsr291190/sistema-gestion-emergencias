# Feature Specification: MVP Sistema Integral de Gestión de Emergencias (SIGE)

**Feature Branch**: `001-sige-mvp`

**Created**: 2026-09-26

**Status**: Draft

**Input**: User description: "Especifica un MVP llamado Sistema Integral de Gestion de Emergencias (SIGE) para apoyar a un operador que recibe y coordina emergencias. El sistema debe permitir: registrar una emergencia con tipo, descripcion, ubicacion geografica, prioridad, fecha/hora y datos basicos del reportante; visualizar las emergencias activas en un mapa; administrar unidades de respuesta: ambulancias, bomberos y patrulleros; conocer disponibilidad y estado operativo de cada unidad; asignar una o varias unidades a una emergencia; impedir que una unidad no disponible sea asignada a otra emergencia incompatible; mantener una linea de tiempo completa desde el registro hasta el cierre; manejar estados de emergencia como reportada, validada, despachada, en ruta, en el lugar, atendida y cerrada; mostrar un dashboard con indicadores operativos basicos; registrar quien realizo cada cambio critico. Actores iniciales: Operador y Supervisor. Objetivo del MVP: demostrar de forma visual y entendible el ciclo completo de recepcion, despacho, seguimiento y cierre de una emergencia."

## Clarifications

### Session 2026-09-26

- Q: Cuando dos operadores intentan asignar casi al mismo tiempo la misma unidad a
  emergencias distintas, ¿cómo debe resolverlo el sistema? → A: el servidor revalida
  la disponibilidad de la unidad justo al confirmar la asignación (control optimista);
  si ya no está disponible, rechaza la operación con un mensaje claro.
- Q: Cuando una emergencia tiene varias unidades asignadas, ¿el estado es único para
  toda la emergencia o cada unidad tiene su propio estado? → A: cada unidad asignada
  progresa su propio estado (despachada, en ruta, en el lugar, atendida) de forma
  independiente dentro de la misma emergencia; el estado general de la emergencia se
  deriva del conjunto de estados de sus unidades.
- Q: ¿Quién puede cerrar una emergencia, y quién puede reabrirla si ya está cerrada?
  → A: únicamente el rol Supervisor puede cerrar o reabrir una emergencia.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Registrar y visualizar una emergencia (Priority: P1)

Un Operador recibe un aviso de emergencia y la registra en el sistema con tipo,
descripción, ubicación geográfica, prioridad, fecha/hora y datos básicos del
reportante. Al guardarla, la emergencia aparece inmediatamente en el mapa operativo.

**Why this priority**: es el punto de entrada de todo el flujo; sin esto no existe
nada que despachar, asignar ni cerrar. Es el mínimo indispensable para que el MVP
tenga sentido.

**Independent Test**: puede probarse completamente registrando una emergencia y
verificando que aparece en el mapa con sus datos correctos, sin necesidad de que
existan unidades ni asignaciones todavía.

**Acceptance Scenarios**:

1. **Given** un Operador autenticado en la pantalla "Nueva emergencia", **When**
   completa tipo, descripción, ubicación, prioridad y datos del reportante y guarda,
   **Then** la emergencia se crea con estado "reportada" y queda visible en el mapa
   operativo y en el dashboard.
2. **Given** una emergencia ya registrada, **When** el Operador abre su detalle,
   **Then** ve todos los datos ingresados y el estado actual.

---

### User Story 2 - Administrar unidades y conocer su disponibilidad (Priority: P2)

Un Operador o Supervisor consulta el listado de unidades de respuesta (ambulancia,
bomberos, patrullero) y su estado operativo (disponible, ocupada, fuera de servicio)
antes de decidir a quién asignar una emergencia.

**Why this priority**: sin visibilidad de qué unidades existen y su disponibilidad,
no es posible tomar una decisión de asignación correcta; es prerrequisito directo
de la historia de asignación.

**Independent Test**: puede probarse dando de alta unidades de cada tipo y
verificando que su estado se refleja correctamente en el listado y en el mapa,
sin necesidad de que exista todavía una emergencia.

**Acceptance Scenarios**:

1. **Given** un Supervisor en la pantalla "Unidades", **When** registra una nueva
   unidad con tipo y estado inicial "disponible", **Then** la unidad aparece en el
   listado y en el mapa con ese estado.
2. **Given** una unidad marcada como "fuera de servicio", **When** un Operador
   intenta asignarla a una emergencia, **Then** el sistema no la ofrece como opción
   válida de asignación.

---

### User Story 3 - Asignar una o varias unidades a una emergencia (Priority: P1)

Un Operador, viendo una emergencia activa y las unidades disponibles cercanas,
asigna una o varias unidades a esa emergencia. El sistema impide asignar una unidad
que ya está ocupada en una emergencia incompatible.

**Why this priority**: es el núcleo funcional del despacho; junto con la historia 1,
forma el recorrido mínimo demostrable del MVP.

**Independent Test**: puede probarse con una emergencia y al menos una unidad
disponible ya creadas; se verifica que la asignación cambia el estado de la unidad
y de la emergencia, y que una unidad no disponible no puede asignarse.

**Acceptance Scenarios**:

1. **Given** una emergencia en estado "validada" y una unidad "disponible", **When**
   el Operador asigna la unidad a la emergencia, **Then** la unidad pasa a estado
   "ocupada", esa asignación inicia en "despachada" (con su propio avance
   independiente si se suman más unidades) y queda registrado quién hizo la
   asignación y cuándo.
2. **Given** una unidad ya asignada a una emergencia activa, **When** otro Operador
   intenta asignarla a una emergencia distinta e incompatible, **Then** el sistema
   revalida la disponibilidad al confirmar y rechaza la asignación explicando el
   motivo si la unidad ya no está disponible.

---

### User Story 4 - Seguir el estado de la emergencia hasta el cierre (Priority: P2)

Un Operador actualiza el estado de una emergencia a medida que evoluciona
(despachada → en ruta → en el lugar → atendida → cerrada) y puede revisar la línea
de tiempo completa de cambios, incluyendo quién los realizó.

**Why this priority**: sin esto el MVP no puede demostrar el "ciclo completo" que
es el objetivo declarado del proyecto; depende de que ya exista una emergencia
asignada (historias 1 y 3).

**Independent Test**: puede probarse tomando una emergencia ya despachada y
avanzando sus estados uno por uno, verificando que cada cambio queda en el timeline
con autor y fecha/hora, y que el cierre deja la emergencia en un estado final
consultable (no eliminado).

**Acceptance Scenarios**:

1. **Given** una unidad asignada en estado "en ruta", **When** el Operador la marca
   como "en el lugar" y luego "atendida", **Then** cada transición de esa unidad
   queda registrada en el timeline con usuario y fecha/hora, y el estado general de
   la emergencia se actualiza cuando todas sus unidades asignadas llegan a "atendida".
2. **Given** una emergencia con todas sus unidades en "atendida", **When** el
   Supervisor la cierra, **Then** la emergencia pasa a "cerrada", conserva todo su
   historial y deja de aparecer como activa en el mapa y el dashboard.
3. **Given** una emergencia "cerrada", **When** el Supervisor decide reabrirla,
   **Then** el sistema lo permite y registra la reapertura en el timeline con
   usuario y fecha/hora; ningún Operador puede cerrar ni reabrir por sí mismo.

---

### User Story 5 - Ver un dashboard con indicadores operativos (Priority: P3)

Un Supervisor abre el dashboard y ve indicadores básicos del estado operativo
actual: cantidad de emergencias activas, por estado y por prioridad, y unidades
disponibles vs. ocupadas.

**Why this priority**: aporta valor de visibilidad gerencial y es un buen elemento
de demostración, pero el ciclo de negocio funciona sin él; por eso va después de
los flujos operativos (historias 1 a 4).

**Independent Test**: puede probarse con datos demo de emergencias y unidades ya
cargados, verificando que los indicadores mostrados coinciden con el conteo real
de esos datos.

**Acceptance Scenarios**:

1. **Given** existen emergencias activas en distintos estados, **When** el
   Supervisor abre el dashboard, **Then** ve el conteo correcto de emergencias por
   estado y por prioridad.
2. **Given** una emergencia cambia de estado o una unidad cambia de disponibilidad,
   **When** el dashboard se actualiza, **Then** los indicadores reflejan el cambio
   sin requerir recarga manual de la página.

---

### User Story 6 - Experiencia de uso clara, consistente y responsiva (Priority: P3)

Un Operador o Supervisor usa SIGE en distintos tamaños de pantalla (laptop en el
puesto de trabajo, proyector en una demostración en aula) y siempre entiende qué
está pasando: cada acción crítica (registrar, validar, asignar, cambiar estado,
cerrar) da una confirmación o un mensaje de error claro en español, los estados y
prioridades se identifican de un vistazo con el mismo lenguaje visual en todas las
pantallas, y la navegación es consistente entre vistas.

**Why this priority**: no bloquea el ciclo de negocio (las historias 1 a 5 funcionan
sin esto), pero es lo que hace que el MVP sea *demostrable* y comprensible por un
observador externo sin explicación técnica (objetivo del MVP, SC-005) — por eso
tiene la misma prioridad que el Dashboard (P3) y se trabaja junto con o después de
las historias operativas.

**Independent Test**: puede probarse abriendo cada pantalla del MVP (login,
dashboard, mapa, emergencias, unidades, despacho) en un ancho de escritorio y en un
ancho reducido (ej. proyector en 1024px), y forzando al menos un error de validación
en cada formulario, sin necesidad de un flujo de negocio completo.

**Acceptance Scenarios**:

1. **Given** cualquier pantalla del MVP, **When** se reduce el ancho de la ventana al
   típico de una demostración en aula (≥1024px), **Then** el contenido permanece
   legible y utilizable, sin scroll horizontal ni elementos superpuestos.
2. **Given** un formulario con campos obligatorios (nueva emergencia, alta de unidad,
   login), **When** el usuario lo envía incompleto o con un dato inválido, **Then**
   el sistema muestra, junto al campo correspondiente, un mensaje específico en
   español que indica qué corregir — no un error genérico ni un código técnico.
3. **Given** una acción crítica en curso (crear, validar, asignar, cerrar, reabrir),
   **When** el usuario la ejecuta, **Then** el botón muestra un estado de "cargando"
   y se deshabilita hasta recibir respuesta, para evitar envíos duplicados.
4. **Given** los mismos valores de estado y prioridad, **When** aparecen en el
   listado de emergencias, el mapa, el detalle y el dashboard, **Then** se
   representan con la misma etiqueta y el mismo color en todas las pantallas.
5. **Given** un usuario que navega solo con teclado, **When** recorre el formulario
   de "Nueva emergencia" con Tab, **Then** el orden de foco sigue el orden visual de
   los campos y el botón de enviar es alcanzable sin usar el mouse.
6. **Given** los formularios del MVP (login, nueva emergencia, alta de unidad,
   despacho), **When** se comparan entre sí, **Then** cada campo sigue la misma
   estructura de presentación (etiqueta, control y mensaje de error en la misma
   posición relativa) y ese patrón está implementado como un componente reutilizable,
   no repetido con marcado HTML distinto en cada pantalla.

---

### Edge Cases

- Si dos Operadores intentan asignar la misma unidad a emergencias distintas casi al
  mismo tiempo, el sistema revalida la disponibilidad de la unidad en el momento de
  confirmar; la segunda confirmación se rechaza con un mensaje que explica que la
  unidad ya fue asignada.
- Una emergencia puede tener varias unidades asignadas simultáneamente; cada unidad
  progresa su propio estado de forma independiente dentro de esa emergencia (ver
  sección Clarifications).
- Solo el rol Supervisor puede cerrar una emergencia o reabrir una que ya está
  cerrada; un Operador no tiene esa capacidad (ver sección Clarifications).
- ¿Qué sucede si una unidad asignada pasa a "fuera de servicio" en medio de una
  atención en curso? Se asume que el sistema debe permitir reasignar otra unidad
  sin perder el historial de la unidad original (ver Assumptions).
- ¿Qué pasa si se intenta registrar una emergencia sin ubicación geográfica válida?
  Se asume que el sistema debe bloquear el guardado y solicitar una ubicación
  válida (ver Assumptions).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema MUST permitir a un Operador autenticado registrar una
  emergencia con tipo, descripción, ubicación geográfica, prioridad, fecha/hora y
  datos básicos del reportante.
- **FR-002**: El sistema MUST asignar automáticamente el estado inicial "reportada"
  a toda emergencia nueva.
- **FR-003**: El sistema MUST mostrar las emergencias activas en un mapa operativo,
  junto con las unidades de respuesta.
- **FR-004**: El sistema MUST permitir administrar unidades de respuesta de tipo
  ambulancia, bomberos y patrullero, incluyendo su estado operativo (disponible,
  ocupada, fuera de servicio). Crear una unidad y cambiar su estado operativo
  manualmente (por ejemplo, a "fuera de servicio") MUST estar restringido al rol
  Supervisor; cualquier rol autenticado puede consultarlas (FR-005).
- **FR-005**: El sistema MUST mostrar la disponibilidad y el estado operativo actual
  de cada unidad en todo momento.
- **FR-006**: El sistema MUST permitir asignar una o más unidades a una emergencia.
  Mientras una unidad tenga una asignación activa (emergencia no cerrada), su estado
  operativo MUST ser "ocupada", por lo que no puede quedar asignada simultáneamente a
  otra emergencia (ver FR-007).
- **FR-007**: El sistema MUST revalidar la disponibilidad de una unidad en el momento
  de confirmar su asignación (control optimista) y MUST rechazar la asignación con un
  mensaje explicativo si la unidad ya no está disponible o es incompatible con su
  estado actual.
- **FR-008**: El sistema MUST mantener una línea de tiempo completa de cada
  emergencia, desde su registro hasta su cierre, incluyendo cada cambio de estado.
- **FR-009**: El sistema MUST soportar las transiciones de estado: reportada →
  validada → despachada → en ruta → en el lugar → atendida → cerrada. La transición
  "reportada" → "validada" es una acción explícita de cualquier Operador o Supervisor
  (a diferencia del cierre/reapertura, restringido a Supervisor por FR-017), y MUST
  ejecutarse antes de poder asignar unidades (FR-006).
- **FR-010**: El sistema MUST registrar qué usuario realizó cada cambio crítico —
  incluyendo la creación de la emergencia, su validación, cada asignación, cada
  cambio de estado, y su cierre o reapertura — y en qué momento.
- **FR-011**: El sistema MUST mostrar un dashboard con indicadores operativos
  básicos: emergencias activas por estado, por prioridad, y unidades disponibles
  vs. ocupadas.
- **FR-012**: El sistema MUST distinguir al menos dos roles de usuario, Operador y
  Supervisor, con acceso autenticado.
- **FR-013**: El sistema MUST conservar el historial de una emergencia cerrada; no
  MUST permitir su eliminación física.
- **FR-014**: El sistema MUST reflejar en el dashboard y el mapa los cambios
  relevantes de forma oportuna, sin requerir una recarga manual completa de la
  página.
- **FR-015**: El sistema MUST validar en el servidor los datos obligatorios de una
  emergencia (tipo, ubicación, prioridad) antes de aceptarla como registrada.
- **FR-016**: Cuando una emergencia tiene varias unidades asignadas, cada unidad
  MUST progresar su propio estado (despachada, en ruta, en el lugar, atendida) de
  forma independiente; el estado general de la emergencia MUST pasar a "atendida"
  únicamente cuando todas sus unidades asignadas alcanzan "atendida". Una asignación
  MUST solo poder avanzar hacia adelante en esa secuencia: el sistema MUST rechazar
  cualquier intento de retroceder a un estado anterior o de repetir el estado actual.
- **FR-017**: El sistema MUST restringir el cierre y la reapertura de una emergencia
  exclusivamente al rol Supervisor. No hay límite de veces que una emergencia puede
  reabrirse; cada cierre y cada reapertura MUST generar su propio evento de auditoría
  (FR-010).
- **FR-018**: Al cerrar una emergencia, el sistema MUST liberar automáticamente (pasar
  a "disponible") las unidades que quedaron asignadas a ella, salvo que una unidad ya
  esté marcada "fuera de servicio" por un motivo independiente. Al reabrir una
  emergencia, las unidades no se reasignan automáticamente; el Operador debe volver a
  asignarlas si corresponde.
- **FR-019**: El sistema MUST aceptar el registro de una emergencia con el nombre del
  reportante como dato obligatorio; el contacto del reportante (teléfono) es opcional,
  ya que en una emergencia real no siempre puede obtenerse.
- **FR-020**: El sistema MUST permanecer legible y utilizable, sin scroll horizontal
  ni elementos superpuestos, en anchos de pantalla desde 1024px (User Story 6).
- **FR-021**: El sistema MUST mostrar, junto a cada campo obligatorio inválido de un
  formulario, un mensaje de error específico en español (no genérico ni un código
  técnico) que indique qué corregir.
- **FR-022**: El sistema MUST deshabilitar el control de envío y mostrar un estado de
  "cargando" mientras una acción crítica (crear, validar, asignar, cambiar estado,
  cerrar, reabrir) está en curso, para evitar envíos duplicados.
- **FR-023**: El sistema MUST usar la misma etiqueta y el mismo color para cada valor
  de estado y de prioridad en todas las pantallas donde aparece (listado, mapa,
  detalle, dashboard).
- **FR-024**: Los formularios del MVP MUST poder completarse y enviarse navegando
  únicamente con teclado, siguiendo el orden visual de los campos.
- **FR-025**: Los campos de los formularios del MVP MUST presentarse mediante un
  único componente de campo reutilizable (etiqueta, control y mensaje de error en la
  misma estructura), en vez de que cada pantalla repita su propio marcado HTML para
  lo mismo.

### Key Entities

- **Emergencia**: incidente reportado; atributos clave: tipo, descripción,
  ubicación geográfica, prioridad, fecha/hora de reporte, datos del reportante,
  estado actual, unidades asignadas, timeline de cambios.
- **Unidad de Respuesta**: recurso operativo (ambulancia, bomberos, patrullero);
  atributos clave: tipo, identificador, estado operativo (disponible/ocupada/fuera
  de servicio), ubicación actual.
- **Asignación**: relación entre una Emergencia y una o más Unidades; atributos
  clave: fecha/hora de asignación, usuario que la realizó, estado de la asignación.
- **Usuario**: persona que opera el sistema; atributos clave: rol (Operador o
  Supervisor), credenciales de acceso.
- **Evento de Auditoría / Timeline**: registro histórico de un cambio crítico sobre
  una Emergencia o Unidad; atributos clave: tipo de cambio, usuario responsable,
  fecha/hora, estado anterior y nuevo.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Un Operador puede registrar una emergencia completa (todos los
  campos obligatorios) en menos de 2 minutos.
- **SC-002**: Una emergencia recién registrada aparece visible en el mapa operativo
  en menos de 5 segundos.
- **SC-003**: Un Operador puede completar la asignación de una unidad disponible a
  una emergencia en 3 pasos o menos desde el detalle de la emergencia.
- **SC-004**: El 100% de los cambios de estado de una emergencia queda visible en
  su línea de tiempo con usuario y fecha/hora asociados.
- **SC-005**: Un observador externo (por ejemplo, en una demostración en aula) puede
  seguir el ciclo completo de una emergencia — desde el registro hasta el cierre —
  usando únicamente lo que se muestra en pantalla, sin explicación técnica
  adicional.
- **SC-006**: El dashboard refleja un cambio de estado de una emergencia o de
  disponibilidad de una unidad sin que el usuario necesite recargar manualmente la
  página.
- **SC-007**: El 100% de los formularios del MVP (login, nueva emergencia, alta de
  unidad) muestra un mensaje de error específico junto al campo correspondiente ante
  un dato inválido, y ninguno permite un envío duplicado mientras la acción está en
  curso.

## Assumptions

- El MVP es un producto de demostración académica, no un sistema de producción; los
  datos de emergencias y unidades son ficticios/demo.
- Los usuarios (Operador, Supervisor) son creados previamente por el sistema o un
  administrador; el MVP no incluye autorregistro público de cuentas.
- Se asume que el sistema debe bloquear el registro de una emergencia sin una
  ubicación geográfica válida, en lugar de aceptarla con ubicación vacía.
- Se asume que una unidad que pasa a "fuera de servicio" durante una atención en
  curso permite reasignar la emergencia a otra unidad sin perder el historial de la
  unidad original.
