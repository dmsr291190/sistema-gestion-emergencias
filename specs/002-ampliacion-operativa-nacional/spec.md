# Feature Specification: Ampliación Operativa Nacional de SIGE

**Feature Branch**: `002-ampliacion-operativa-nacional`

**Created**: 2026-09-26

**Status**: Draft

**Input**: Documento `SIGE_Ampliacion_Operativa_Nacional.md` (aportado por Diego),
que evoluciona el MVP cerrado en `001-sige-mvp` (tag `mvp-sdd-demo`) hacia una
plataforma nacional de monitoreo, coordinación y seguimiento de emergencias
terrestres, marítimas y aéreas en el Perú: catálogo configurable de tipos de
emergencia, cobertura geográfica nacional, mapa operativo avanzado (leyenda, capas,
iconografía, popups), vista pública sin autenticación, gestión de personal y
recursos logísticos por unidad, formularios enriquecidos, rediseño visual con CoreUI
en tema oscuro, dashboard avanzado, administración completa de usuarios y roles, y
generación automática de datos y usuarios demo. Esta ampliación se integra al
proyecto existente — no lo reemplaza; el MVP (Operador/Supervisor, emergencia →
validar → asignar → avanzar → cerrar/reabrir) sigue vigente y debe seguir
funcionando.

## Clarifications

### Session 2026-09-26

- Q: El catálogo de roles (18.4) incluye "Unidad de respuesta" como rol de inicio de
  sesión. ¿Una unidad de respuesta tiene su propia cuenta con la que se autentica
  (por ejemplo, desde un dispositivo en el vehículo), o es solo una etiqueta interna
  sin cuenta real de acceso? → A: Sí tiene cuenta propia: una unidad de respuesta
  puede iniciar sesión con credenciales propias y actualizar su propio estado
  operativo y posición, además de que el Operador/Supervisor siga pudiendo hacerlo
  por ella.
- Q: ¿Qué tan "aproximada" debe ser la ubicación de una emergencia en la vista
  pública, para no exponer coordenadas sensibles de reportantes o víctimas? → A: Se
  redondea a nivel de distrito/centro poblado (sin mostrar la coordenada exacta);
  el mapa público ubica el marcador en el centro aproximado de esa zona, no en el
  punto exacto reportado. Los usuarios autenticados (Operador, Supervisor,
  Coordinador logístico, etc.) siguen viendo siempre la coordenada exacta
  registrada; el redondeo aplica únicamente a la vista pública sin login.
- Q: El campo `Emergencia.Tipo` del MVP es texto libre. Con el nuevo catálogo
  administrable de tipos/subtipos, ¿las emergencias ya existentes deben migrarse
  para referenciar el catálogo, o el catálogo aplica solo hacia adelante? → A: Se
  migran: al activar la ampliación, cada emergencia existente se vincula al tipo del
  catálogo que coincida por nombre (creando ese tipo en el catálogo si no existe
  todavía), para que no queden emergencias del MVP sin tipo de catálogo. El tipo
  creado por esta migración queda **activo** por defecto, para no bloquear el
  registro de nuevas emergencias de ese tipo.

### Session 2026-09-26 (continuación — hallazgos de Checklist)

- Q: "Institución" aparece en Personal, Recurso y Usuario ampliado — ¿es un único
  catálogo compartido de organismos (bomberos, policía, salud, marina, etc.), o
  un catálogo independiente por cada entidad? → A: Es un único catálogo
  compartido: la misma lista de instituciones se reutiliza en Personal, Recurso
  y Usuario.
- Q: ¿El rol "Visualizador" tiene acceso de solo lectura a todo el sistema
  autenticado, o solo a un subconjunto de pantallas? → A: Solo a un subconjunto:
  dado que ya existe una vista pública de solo lectura (US7) para el ciudadano
  general (incendios, desastres naturales, etc.), el rol "Visualizador"
  autenticado se limita a mapa y dashboard; no tiene acceso a administración de
  usuarios, personal ni recursos.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Administrar usuarios y roles ampliados (Priority: P1)

Un Administrador gestiona las cuentas del sistema: crea usuarios, les asigna uno o
más de los roles ampliados (Administrador, Supervisor, Operador, Coordinador
logístico, Jefe de unidad, Unidad de respuesta, Visualizador), bloquea/desbloquea
cuentas, fuerza el restablecimiento de contraseña, y consulta el historial de seguridad
de cada cuenta.

**Why this priority**: todo lo demás en esta ampliación (usuarios demo, vista de
unidades logueándose, control de acceso a la vista pública) depende de que el
sistema soporte más de dos roles y de que exista alguien capaz de administrarlos.

**Independent Test**: iniciar sesión como Administrador, crear un usuario nuevo,
asignarle el rol Coordinador logístico, bloquearlo y desbloquearlo, sin necesidad de
que exista todavía ninguna emergencia ni unidad.

**Acceptance Scenarios**:

1. **Given** un Administrador autenticado, **When** crea un usuario con nombre,
   documento, correo, institución y un rol válido, **Then** el usuario queda en
   estado "Activo" y puede iniciar sesión con ese rol.
2. **Given** un usuario bloqueado por el Administrador, **When** ese usuario intenta
   iniciar sesión, **Then** el sistema rechaza el acceso indicando que la cuenta
   está bloqueada.
3. **Given** una cuenta marcada para "requiere cambio de contraseña", **When** el
   usuario inicia sesión con la contraseña temporal, **Then** el sistema exige
   definir una nueva contraseña antes de continuar.
4. **Given** un usuario con rol "Unidad de respuesta", **When** inicia sesión,
   **Then** accede únicamente a las pantallas necesarias para actualizar el estado
   operativo y la posición de su propia unidad, no a la administración de usuarios
   ni a la de otras unidades.
5. **Given** cualquier rol distinto de Administrador, **When** intenta acceder al
   módulo de administración de usuarios, **Then** el sistema lo rechaza.

---

### User Story 2 - Generar automáticamente usuarios y datos de demostración (Priority: P1)

Al desplegar el sistema en un entorno de Desarrollo, Demo o Pruebas, el propio
sistema genera automáticamente las cuentas demo (Administrador, Supervisor,
Operadores regionales, Coordinador logístico, Jefes de unidad, Unidades de
respuesta, Visualizadores) y un volumen realista de emergencias, unidades, personal
y recursos, sin que nadie tenga que crearlos manualmente ni importar un archivo
externo.

**Why this priority**: sin esto, ninguna de las demás historias (mapa con datos,
dashboard con indicadores, vista pública con contenido) es demostrable; es la base
de datos que todas las demás historias necesitan para tener sentido.

**Independent Test**: levantar el sistema desde cero en un entorno de Desarrollo y
verificar que, sin ninguna acción manual, existen los usuarios demo documentados y
un volumen mínimo de emergencias/unidades/personal/recursos variado.

**Acceptance Scenarios**:

1. **Given** una base de datos vacía en un entorno de Desarrollo, **When** el
   sistema arranca, **Then** quedan creados automáticamente los usuarios demo
   (con sus roles) y los datos demo (emergencias, unidades, personal, recursos) en
   cantidad suficiente para que filtros, mapa y dashboard muestren información
   significativa.
2. **Given** que el sistema ya arrancó una vez y generó los datos demo, **When**
   se reinicia nuevamente en el mismo entorno, **Then** no se duplican usuarios,
   roles ni datos demo ya existentes (el proceso es idempotente).
3. **Given** un entorno marcado como Producción, **When** el sistema arranca,
   **Then** el generador de usuarios y datos demo NO se ejecuta.
4. **Given** los usuarios demo generados, **When** un desarrollador consulta la
   documentación local (no expuesta en producción), **Then** puede ver la lista de
   usuarios y su rol para poder iniciar sesión como cualquiera de ellos.

---

### User Story 3 - Registrar emergencias con catálogo nacional y cobertura geográfica completa (Priority: P1)

Un Operador registra una emergencia eligiendo su tipo y subtipo desde un catálogo
administrable (terrestre, marítimo o aéreo), y su ubicación en cualquier punto del
Perú — costa, sierra, selva, zona urbana, rural, carretera, río, puerto, litoral,
mar, aeropuerto o zona remota — con departamento/provincia/distrito cuando aplique,
o solo coordenadas cuando no exista dirección formal (por ejemplo, en el mar).

**Why this priority**: es la extensión directa del núcleo del MVP (registrar una
emergencia) hacia el nuevo alcance nacional y multi-ámbito; sin esto, ninguna
emergencia marítima o aérea puede existir en el sistema.

**Independent Test**: un Administrador da de alta un nuevo tipo y subtipo de
emergencia marítima en el catálogo, y un Operador registra una emergencia de ese
subtipo con solo coordenadas (sin dirección formal), sin depender de que exista
ninguna unidad todavía.

**Acceptance Scenarios**:

1. **Given** el catálogo de tipos de emergencia, **When** un Administrador crea un
   nuevo tipo con su ámbito (terrestre/marítimo/aéreo/mixto), ícono, color y
   prioridad por defecto, **Then** ese tipo queda disponible de inmediato para
   registrar nuevas emergencias.
2. **Given** una emergencia en zona urbana, **When** el Operador la registra,
   **Then** puede indicar departamento, provincia, distrito, centro poblado,
   dirección y referencia, además de la ubicación en el mapa.
3. **Given** una emergencia marítima sin dirección formal, **When** el Operador la
   registra, **Then** el sistema acepta únicamente las coordenadas como ubicación
   principal, sin exigir departamento/distrito.
4. **Given** un tipo de emergencia desactivado por un Administrador, **When** un
   Operador intenta registrar una emergencia nueva, **Then** ese tipo no aparece
   como opción disponible.

---

### User Story 4 - Explorar el mapa operativo avanzado (Priority: P2)

Un Operador o Supervisor explora un mapa con leyenda plegable, capas
activables/desactivables (emergencias, unidades, bases, hospitales, puertos, rutas,
histórico), iconografía distinta por tipo de emergencia y de unidad, y filtros por
tipo, prioridad, estado, ubicación y ámbito; al seleccionar un marcador, ve un
resumen rápido sin abandonar el mapa.

**Why this priority**: es el valor visual central de la ampliación, pero depende de
que ya existan datos (US2) y un catálogo de tipos (US3) para tener sentido — por eso
va después de esas dos historias P1.

**Independent Test**: con datos demo ya cargados, abrir el mapa, activar/desactivar
al menos dos capas, aplicar un filtro por tipo de emergencia, y abrir el resumen de
un marcador, sin necesidad de registrar nada nuevo.

**Acceptance Scenarios**:

1. **Given** el mapa con emergencias y unidades de distintos tipos, **When** el
   usuario lo abre, **Then** cada tipo de emergencia y de unidad se distingue con un
   ícono propio (no solo con color), consistente con la leyenda.
2. **Given** la leyenda del mapa, **When** el usuario la abre, **Then** puede ver y
   activar/desactivar cada capa disponible (emergencias, unidades, bases,
   hospitales, puertos, rutas, histórico).
3. **Given** un filtro aplicado (por ejemplo, solo emergencias marítimas críticas),
   **When** se aplica, **Then** el mapa muestra únicamente los marcadores que
   cumplen ese filtro.
4. **Given** un marcador de emergencia o de unidad, **When** el usuario hace clic,
   **Then** se abre un resumen (tooltip, popup o modal) con los datos principales y
   un botón para ver el detalle completo, sin cerrar el mapa.

---

### User Story 5 - Gestionar personal y recursos de las unidades (Priority: P2)

Un Coordinador logístico o Jefe de unidad registra el personal asignado a cada
unidad (nombre, documento, especialidad, función, certificaciones, disponibilidad) y
los recursos que transporta (equipos, herramientas, víveres, líquidos, estructuras,
material médico, equipo de rescate), con cantidades y mínimos, para que un Operador
pueda evaluar si una unidad es adecuada para una emergencia.

**Why this priority**: enriquece la decisión de despacho del MVP existente
(asignar unidades) con información logística real, pero no bloquea el ciclo de
negocio principal — por eso es P2, después de que el mapa (US4) ya sea usable.

**Independent Test**: registrar una unidad, asignarle un miembro de personal y un
recurso con cantidad y mínimo, y verificar que esa información se puede consultar al
seleccionar la unidad, sin necesidad de una emergencia real.

**Acceptance Scenarios**:

1. **Given** una unidad de respuesta, **When** un Coordinador logístico le asigna
   personal (nombre, especialidad, función) y recursos (categoría, cantidad,
   mínimo), **Then** esa información queda visible al seleccionar la unidad en el
   despacho o el mapa.
2. **Given** un recurso cuya cantidad disponible cae por debajo de su mínimo
   definido, **When** se consulta la unidad, **Then** el sistema lo señala
   visualmente como una alerta de bajo stock.
3. **Given** una unidad con personal y recursos ya cargados, **When** un Operador
   evalúa a qué unidad despachar, **Then** puede ver de un vistazo su capacidad,
   personal y recursos antes de asignarla.

---

### User Story 6 - Completar formularios enriquecidos (Priority: P2)

Un usuario autorizado completa formularios más completos para emergencia, unidad,
personal, recurso y despacho — organizados en secciones claras (datos generales,
ubicación, afectados, evidencias, observaciones), con ayudas contextuales,
validaciones y campos dependientes — en vez de los formularios mínimos del MVP.

**Why this priority**: mejora la calidad de los datos capturados por las historias
anteriores (US3, US5), pero no introduce comportamiento nuevo por sí sola — depende
de que existan los campos ampliados de esas historias.

**Independent Test**: abrir el formulario ampliado de "Nueva emergencia" y verificar
que incluye las secciones de datos generales, ubicación, afectados y observaciones,
con validación específica por campo, sin necesidad de completar un flujo de negocio
completo.

**Acceptance Scenarios**:

1. **Given** el formulario ampliado de emergencia, **When** el usuario lo completa,
   **Then** puede registrar afectados, heridos, desaparecidos, fallecidos y
   evacuados, además de los campos ya existentes en el MVP.
2. **Given** un campo dependiente (por ejemplo, subtipo según el tipo elegido),
   **When** el usuario cambia el tipo, **Then** las opciones de subtipo se
   actualizan de forma consistente.
3. **Given** un dato inválido en cualquier formulario ampliado, **When** el usuario
   lo envía, **Then** recibe un mensaje de error específico junto al campo
   correspondiente (mismo estándar de la Historia de Usuario 6 del MVP).

---

### User Story 7 - Consultar la vista pública sin autenticación (Priority: P3)

Cualquier persona, sin iniciar sesión, navega un mapa público con las emergencias
activas, usa filtros y leyenda, y consulta un resumen y una línea de tiempo pública
de cada emergencia, sin ver ningún dato sensible (identidad de reportantes o
víctimas, contacto, ubicación exacta, observaciones internas).

**Why this priority**: es un valor añadido de transparencia, pero depende de que ya
existan emergencias reales y un mapa funcional (US3, US4) — por eso es P3, la última
historia orientada al ciudadano.

**Independent Test**: sin iniciar sesión, abrir la vista pública, filtrar
emergencias, y abrir el resumen de una, verificando que ningún dato restringido
aparece en la respuesta ni en la pantalla.

**Acceptance Scenarios**:

1. **Given** una emergencia con datos de reportante y observaciones internas,
   **When** se consulta desde la vista pública, **Then** esos datos no aparecen en
   ninguna parte de la respuesta.
2. **Given** la ubicación exacta de una emergencia, **When** se muestra en el mapa
   público, **Then** aparece aproximada a nivel de distrito/centro poblado, no en la
   coordenada exacta reportada.
3. **Given** una emergencia cerrada, **When** se consulta su línea de tiempo
   pública, **Then** se muestra una versión resumida (sin usuarios internos ni
   auditoría interna) del progreso hasta el cierre.

---

### User Story 8 - Ver un dashboard operativo avanzado (Priority: P3)

Un Supervisor o Administrador abre un dashboard rediseñado que muestra, además de
los indicadores del MVP, el desglose por ámbito (terrestre/marítimo/aéreo), personal
desplegado, recursos movilizados, tiempo promedio de atención, y visualizaciones de
evolución temporal y unidades más activas.

**Why this priority**: es una evolución directa del dashboard ya existente en el
MVP; depende de que exista suficiente variedad de datos (US2) y del catálogo (US3)
para que las nuevas visualizaciones tengan sentido — por eso se ubica al final.

**Independent Test**: con datos demo cargados, abrir el dashboard y verificar que
los nuevos indicadores (por ámbito, personal desplegado, tiempo promedio) coinciden
con los datos reales.

**Acceptance Scenarios**:

1. **Given** emergencias de distintos ámbitos, **When** se abre el dashboard,
   **Then** se muestra el desglose terrestre vs. marítimo vs. aéreo.
2. **Given** el historial de emergencias cerradas, **When** se calcula el tiempo
   promedio de atención, **Then** el valor mostrado es verificable contra las
   marcas de tiempo reales del timeline de esas emergencias.

---

### User Story 9 - Usar una interfaz operativa en tema oscuro (Priority: P3)

Un usuario navega toda la aplicación (login, mapa, formularios, tablas, modales,
dashboard, vista pública, administración de usuarios) con un tema oscuro consistente
que transmite la estética de un centro nacional de operaciones, sin perder
legibilidad ni el color distintivo de prioridades y estados.

**Why this priority**: es la capa visual final sobre todas las demás historias; no
aporta comportamiento nuevo, solo presentación — por eso se implementa al final,
sobre pantallas que ya funcionan.

**Independent Test**: recorrer las pantallas principales del sistema y verificar que
el tema oscuro se aplica de forma consistente, sin comprometer el contraste de
colores de prioridad/estado ya definidos en el MVP (Historia de Usuario 6).

**Acceptance Scenarios**:

1. **Given** cualquier pantalla del sistema, **When** se navega con el tema activo,
   **Then** el fondo, texto, tablas y componentes de CoreUI se ven consistentes en
   tema oscuro, sin áreas que queden en tema claro por error.
2. **Given** los colores de prioridad y estado ya definidos en el MVP, **When** se
   ven sobre el tema oscuro, **Then** mantienen suficiente contraste para
   distinguirse entre sí.

---

### Edge Cases

- Un usuario con el rol "Unidad de respuesta" deja de tener acceso si su unidad se
  marca como "Fuera de servicio" de forma prolongada: el sistema no bloquea
  automáticamente esa cuenta; el bloqueo, si corresponde, sigue siendo una acción
  manual del Administrador.
- Si dos Administradores editan el mismo usuario casi al mismo tiempo, la última
  escritura exitosa prevalece; no se define en esta ampliación un mecanismo de
  bloqueo optimista adicional al ya existente para unidades (FR-007 del MVP).
- Si se desactiva un tipo de emergencia que ya tiene emergencias existentes
  asociadas, esas emergencias existentes conservan su tipo; solo se impide crear
  emergencias *nuevas* con un tipo desactivado.
- Si la vista pública recibe una solicitud para una emergencia que no existe o fue
  eliminada del alcance público, responde de forma genérica (no revela si el
  identificador existe o no, para no filtrar información por enumeración).

## Requirements *(mandatory)*

### Functional Requirements

**Catálogo de tipos de emergencia**

- **FR-101**: El sistema MUST permitir a un Administrador crear, editar y
  activar/desactivar tipos y subtipos de emergencia, cada uno con ámbito
  (terrestre/marítimo/aéreo/mixto), ícono, color y prioridad por defecto.
- **FR-102**: El sistema MUST impedir seleccionar un tipo desactivado al registrar
  una emergencia nueva, sin afectar las emergencias existentes que ya lo usan.
- **FR-103**: El sistema MUST migrar, al activar esta ampliación, cada valor de
  `Tipo` existente del MVP a un tipo del catálogo con el mismo nombre, creando ese
  tipo en el catálogo si todavía no existe. Todo tipo creado por esta migración
  MUST quedar activo por defecto, para no bloquear el registro de emergencias
  nuevas de ese tipo.

**Cobertura geográfica nacional**

- **FR-104**: El sistema MUST permitir registrar una emergencia con
  departamento, provincia, distrito, centro poblado, dirección y referencia cuando
  la ubicación lo permita, y únicamente con coordenadas cuando no exista dirección
  formal (zonas marítimas o remotas).
- **FR-105**: El sistema MUST permitir clasificar cada emergencia por ámbito
  (terrestre, marítimo, aéreo o mixto), heredado por defecto del tipo elegido
  (FR-101) pero editable en el registro.

**Mapa operativo avanzado**

- **FR-106**: El mapa MUST distinguir visualmente, mediante un ícono propio (no solo
  color), cada tipo de emergencia y cada tipo de unidad definidos en los catálogos.
- **FR-107**: El mapa MUST ofrecer una leyenda plegable que identifique prioridad,
  estado, tipo de unidad, ámbito y las capas disponibles (emergencias, unidades,
  bases, hospitales, puertos, rutas, histórico), permitiendo activar o desactivar
  cada capa.
- **FR-108**: El mapa MUST permitir filtrar los marcadores mostrados por tipo,
  subtipo, prioridad, estado, ubicación (departamento/provincia), ámbito, tipo de
  unidad, institución y disponibilidad.
- **FR-109**: El sistema MUST mostrar, al seleccionar un marcador de emergencia o de
  unidad, un resumen rápido (tooltip, popup o modal) con sus datos principales y un
  acceso directo al detalle completo, sin obligar a abandonar el mapa.

**Personal y recursos**

- **FR-110**: El sistema MUST permitir registrar personal (nombre, documento,
  institución, especialidad, función, certificaciones, disponibilidad) asociado a
  una unidad de respuesta.
- **FR-111**: El sistema MUST permitir registrar recursos transportados por una
  unidad (equipos, herramientas, víveres, líquidos, estructuras, material médico, de
  rescate) con cantidad, cantidad disponible y cantidad mínima.
- **FR-112**: El sistema MUST señalar cuando la cantidad disponible de un recurso
  cae por debajo de su mínimo definido.

**Vista pública**

- **FR-113**: El sistema MUST exponer una vista de mapa y listado de emergencias
  activas accesible sin autenticación.
- **FR-114**: La vista pública MUST excluir siempre: documento de identidad,
  teléfono, correo, nombres de reportantes o víctimas, información médica,
  coordenadas exactas, observaciones internas, y cualquier dato de auditoría interna.
- **FR-115**: La vista pública MUST mostrar la ubicación de cada emergencia
  aproximada a nivel de distrito o centro poblado, nunca la coordenada exacta
  reportada.
- **FR-115a**: El sistema MUST seguir mostrando la coordenada exacta registrada a
  todo usuario autenticado (Operador, Supervisor, Coordinador logístico, Jefe de
  unidad, Administrador); el redondeo de FR-115 aplica únicamente a la vista pública
  sin autenticación.

**Administración de usuarios y roles**

- **FR-116**: El sistema MUST restringir la administración de usuarios
  exclusivamente al rol Administrador.
- **FR-117**: El sistema MUST soportar, además de Operador y Supervisor (MVP), los
  roles Administrador, Coordinador logístico, Jefe de unidad, Unidad de respuesta y
  Visualizador.
- **FR-118**: El sistema MUST permitir al Administrador crear, editar,
  activar/desactivar, bloquear/desbloquear un usuario, forzar el restablecimiento de
  su contraseña, y asignar o retirar roles.
- **FR-119**: El sistema MUST registrar, para cada usuario, su último acceso,
  intentos fallidos de inicio de sesión, y quién creó o modificó la cuenta.
- **FR-120**: El sistema MUST permitir que un usuario con rol "Unidad de respuesta"
  inicie sesión con credenciales propias y actualice el estado operativo y la
  posición de su propia unidad; el acceso de ese rol MUST limitarse a esa función.
- **FR-120a**: El sistema MUST limitar el acceso de un usuario con rol
  "Visualizador" a las pantallas de mapa y dashboard en modo solo lectura; ese rol
  MUST NOT tener acceso a la administración de usuarios, personal ni recursos
  (la vista pública sin login, US7, ya cubre la consulta general del ciudadano).
- **FR-121**: El sistema MUST almacenar las contraseñas mediante un mecanismo de
  hashing seguro; nunca en texto plano.

**Generación automática de datos demo**

- **FR-122**: El sistema MUST generar automáticamente, solo en entornos de
  Desarrollo, Demo o Pruebas, los usuarios demo documentados (uno por cada rol
  nuevo) y un volumen variado de emergencias, unidades, personal y recursos.
- **FR-123**: El proceso de generación de datos demo MUST ser idempotente: ejecutarlo
  más de una vez MUST NOT duplicar usuarios, roles ni datos ya creados.
- **FR-124**: El sistema MUST impedir que el generador de usuarios y datos demo se
  ejecute en un entorno marcado como Producción.

**Formularios enriquecidos**

- **FR-125**: El formulario de emergencia MUST permitir registrar afectados,
  heridos, desaparecidos, fallecidos y evacuados, además de los campos ya definidos
  en el MVP.
- **FR-126**: Los formularios de esta ampliación MUST seguir el mismo estándar de
  validación por campo, estado de carga y componente de campo reutilizable definido
  en la Historia de Usuario 6 del MVP (FR-021, FR-022, FR-025).

**Dashboard avanzado**

- **FR-127**: El dashboard MUST mostrar, además de los indicadores del MVP, el
  desglose de emergencias por ámbito (terrestre/marítimo/aéreo) y el tiempo promedio
  de atención de las emergencias cerradas.

**Tema oscuro**

- **FR-128**: La interfaz MUST aplicar un tema oscuro consistente en todas las
  pantallas, manteniendo el contraste de los colores de prioridad y estado ya
  definidos en el MVP.

**Compatibilidad con el MVP**

- **FR-129**: Esta ampliación MUST mantener operativas todas las funcionalidades del
  MVP existente (autenticación, roles Operador/Supervisor, registro, validación,
  asignación, avance de estado, cierre/reapertura, auditoría, timeline).

### Key Entities

- **TipoEmergencia**: entrada del catálogo administrable; atributos clave: nombre,
  ámbito, ícono, color, prioridad por defecto, activo/inactivo. Reemplaza el campo
  de texto libre `Tipo` de `Emergencia` del MVP (con migración, FR-103).
- **Ubicación**: datos geográficos de una emergencia o unidad; atributos clave:
  departamento, provincia, distrito, centro poblado, dirección, referencia,
  latitud, longitud, ámbito.
- **Personal**: persona asociada a una unidad; atributos clave: nombres, apellidos,
  documento, institución, especialidad, función, certificaciones, disponibilidad,
  unidad asociada.
- **Recurso**: ítem inventariable transportado por una unidad; atributos clave:
  código, nombre, categoría, unidad de medida, cantidad, cantidad disponible,
  cantidad mínima, unidad asociada.
- **Usuario** (ampliado del MVP): ahora con más roles posibles, estado (Activo,
  Inactivo, Bloqueado, Pendiente de cambio de contraseña), último acceso, intentos
  fallidos, y trazabilidad de quién lo creó/modificó.
- **CapaDelMapa**: agrupación visual activable/desactivable en el mapa (emergencias,
  unidades, bases, hospitales, puertos, rutas, histórico).
- **Institución**: catálogo simple compartido de organismos participantes
  (bomberos, policía, salud, marina, etc.); una única lista reutilizada como
  referencia desde Personal, Unidad de respuesta (dueña de sus recursos) y
  Usuario ampliado, no un catálogo independiente por entidad.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-101**: Un Administrador puede crear un usuario, asignarle un rol y
  bloquearlo/desbloquearlo en menos de 1 minuto.
- **SC-102**: Al desplegar el sistema desde cero en un entorno de Desarrollo, el
  100% de los usuarios demo documentados existe y puede iniciar sesión sin ninguna
  acción manual adicional.
- **SC-103**: Ejecutar el proceso de generación de datos demo dos veces seguidas no
  produce ningún usuario, rol o dato duplicado (idempotencia verificable contando
  registros antes/después).
- **SC-104**: Un observador puede identificar el tipo de al menos 5 marcadores
  distintos en el mapa (emergencias y unidades) solo por su ícono, sin necesidad de
  hacer clic en cada uno.
- **SC-105**: La vista pública, consultada sin ningún token de autenticación, nunca
  devuelve en su respuesta ninguno de los campos restringidos listados en FR-114
  (verificable inspeccionando la respuesta completa, no solo la pantalla).
- **SC-106**: El MVP existente (registrar → validar → asignar → avanzar → cerrar →
  reabrir) sigue siendo demostrable de punta a punta después de la ampliación, sin
  ninguna regresión.

## Assumptions

- Esta ampliación se implementa como una nueva feature (`002-ampliacion-operativa-nacional`)
  sobre el mismo repositorio y proyecto (`sistema-gestion-emergencias`), no como un
  proyecto nuevo; reutiliza el stack ya decidido en `001-sige-mvp/plan.md`
  (Angular + CoreUI, .NET + Jason Taylor Clean Architecture, MySQL) salvo que
  `/speckit-plan` de esta feature justifique explícitamente un cambio.
- Los roles Operador y Supervisor del MVP conservan exactamente sus permisos
  actuales; los roles nuevos (Administrador, Coordinador logístico, Jefe de unidad,
  Unidad de respuesta, Visualizador) son estrictamente aditivos.
- El volumen de datos demo (sección 20 del documento de origen: 40–60 emergencias,
  25–40 unidades, 60–100 personas, 100+ recursos, 10+ usuarios) se toma como
  objetivo orientativo para el seed, no como un requisito exacto verificado al
  dato.
- Los organismos/instituciones (bomberos, policía, salud, marina, etc.) se modelan
  como un único catálogo simple de texto/etiqueta, compartido por Personal, Unidad
  de respuesta y Usuario ampliado (no un catálogo independiente por entidad), sin
  un módulo completo de gestión institucional — se puede profundizar en una
  ampliación posterior si se requiere.
