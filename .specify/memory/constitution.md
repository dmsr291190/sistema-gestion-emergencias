<!--
Sync Impact Report
- Version change: (initial template) → 1.0.0
- Modified principles: n/a (initial ratification)
- Added sections:
  - Core Principles (7): Arquitectura Limpia y Modular; Reglas de Negocio en el Backend;
    Validación Estricta de Entradas; Trazabilidad y Auditoría; Seguridad por Defecto y
    Mínimo Privilegio; Calidad de Código y Manejo de Errores; Verificabilidad y Alcance
    Acotado del MVP
  - Restricciones Técnicas y de Calidad
  - Flujo de Trabajo y Puertas de Calidad SDD
  - Governance
- Removed sections: none (initial document)
- Follow-up TODOs: none
-->

# Sistema Integral de Gestión de Emergencias (SIGE) Constitution

## Core Principles

### I. Arquitectura Limpia y Modular
El sistema MUST mantener una separación clara entre frontend, backend, dominio e
infraestructura. Cada capa MUST poder evolucionar o probarse de forma independiente.
No se permite lógica de dominio incrustada en la capa de presentación ni acceso directo
a infraestructura desde el dominio.
**Rationale**: un MVP construido con agentes de código crece rápido; sin límites claros
entre capas, la deuda técnica y el acoplamiento se acumulan antes de llegar a la demo.

### II. Reglas de Negocio en el Backend
Toda regla de negocio crítica (transiciones de estado de una emergencia, disponibilidad
de unidades, reglas de asignación) MUST residir y validarse en el backend. El frontend
MUST tratarse como una capa de presentación no confiable para efectos de reglas de negocio.
**Rationale**: evita inconsistencias cuando distintas pantallas o clientes ejecutan la
misma operación, y protege la integridad del flujo de despacho.

### III. Validación Estricta de Entradas
Toda entrada proveniente de un cliente (formularios, API, eventos en tiempo real) MUST
validarse en el servidor antes de persistirse o de disparar una transición de estado.
Las validaciones del cliente son solo una ayuda de UX, nunca la única barrera.
**Rationale**: los datos de una emergencia (ubicación, prioridad, estado) son la base de
decisiones operativas; datos inválidos pueden propagar errores críticos en el despacho.

### IV. Trazabilidad y Auditoría
Las emergencias NO MUST eliminarse físicamente; todo cambio de estado o de asignación de
unidades MUST quedar registrado con quién lo realizó y cuándo. Las operaciones críticas
(asignar, reasignar, cerrar, reabrir) MUST generar un registro de auditoría consultable.
**Rationale**: el MVP debe poder demostrar el ciclo completo de una emergencia con
evidencia verificable, y un sistema de despacho real no puede perder historial.

### V. Seguridad por Defecto y Mínimo Privilegio
El acceso al sistema MUST requerir autenticación, y cada acción MUST evaluarse contra el
rol del usuario (Operador o Supervisor). Por defecto, un usuario solo tiene los permisos
mínimos necesarios para su rol; cualquier permiso adicional MUST justificarse explícitamente.
**Rationale**: un centro de despacho maneja datos sensibles y decisiones operativas; el
acceso indebido tiene consecuencias directas sobre la seguridad de personas.

### VI. Calidad de Código y Manejo de Errores
El código MUST ser legible y testeable, con manejo consistente de errores (sin fallos
silenciosos en operaciones críticas). Las funcionalidades del MVP SHOULD contar con
pruebas automatizadas para las reglas de negocio críticas y los endpoints principales.
**Rationale**: el proyecto se construye con agentes de IA en lotes; sin legibilidad y
pruebas mínimas, es difícil verificar que el comportamiento generado sea correcto.

### VII. Verificabilidad y Alcance Acotado del MVP
Toda funcionalidad implementada MUST poder verificarse contra un criterio de aceptación
explícito. No se MUST introducir funcionalidad fuera del alcance definido en la
especificación vigente sin actualizar primero esa especificación. La interfaz MUST ser
responsive y comprensible para una demostración en aula.
**Rationale**: el objetivo es un MVP demostrable en un plazo de curso, no un sistema de
producción; el alcance disciplinado es lo que permite llegar a una demo funcional.

## Restricciones Técnicas y de Calidad

- El stack técnico (frontend, backend, base de datos, tiempo real, infraestructura) se
  define en `plan.md` y MUST ser consistente con estos principios; esta constitución no
  fija tecnologías específicas.
- Ningún secreto, clave o token MUST versionarse en el repositorio; la configuración
  sensible se maneja por variables de entorno.
- La complejidad arquitectónica (por ejemplo, microservicios) MUST justificarse
  explícitamente contra el alcance del MVP; en ausencia de justificación, se prefiere la
  solución más simple que cumpla la especificación.

## Flujo de Trabajo y Puertas de Calidad SDD

- El proyecto sigue Spec-Driven Development: Constitution → Specify → Clarify → Plan →
  Checklist → Tasks → Analyze → Implement → Converge. No se MUST saltar etapas ni derivar
  código directamente de una conversación informal.
- Ninguna etapa posterior MUST contradecir un artefacto de una etapa anterior sin que esa
  contradicción se resuelva primero actualizando el artefacto correspondiente.
- Cada etapa completada SHOULD cerrarse con un commit de control que deje evidencia del
  artefacto generado.

## Governance

Esta constitución prevalece sobre cualquier preferencia individual de implementación o de
agente (Claude Code, Codex CLI). Toda enmienda MUST documentarse en este archivo, indicando
qué principio cambia y por qué, y MUST reflejarse en un incremento de versión según semver:
MAJOR para eliminación o redefinición incompatible de un principio, MINOR para agregar un
principio o sección, PATCH para aclaraciones de redacción sin cambio de sentido. Toda
implementación (`/speckit-implement`) y convergencia (`/speckit-converge`) MUST verificar
cumplimiento de estos principios antes de considerarse completa.

**Version**: 1.0.0 | **Ratified**: 2026-09-26 | **Last Amended**: 2026-09-26
