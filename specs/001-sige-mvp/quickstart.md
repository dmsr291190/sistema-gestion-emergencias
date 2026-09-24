# Quickstart de validación: MVP SIGE

Guía para levantar el proyecto localmente y validar el recorrido mínimo demostrable
(sección 16 de la guía SDD) una vez que exista código (`/speckit-implement`).

## Prerrequisitos

- Docker y Docker Compose instalados.
- El contenedor MySQL del proyecto ya debe estar corriendo en Docker (no lo levanta
  este `docker-compose.yml`).
- Archivo `.env` creado a partir de `.env.example`, con la cadena de conexión al MySQL
  existente (sin secretos reales, solo valores de desarrollo local).

## Levantar el entorno

```bash
docker compose up --build
```

Esto debe levantar backend (.NET Web API) y frontend (Angular), conectándose al MySQL
ya existente en Docker, según la estructura definida en `plan.md`.

## Datos demo esperados

- Al menos 1 usuario `Operador` y 1 usuario `Supervisor` precargados.
- Al menos 3 unidades de respuesta (una ambulancia, un carro de bomberos, un
  patrullero) en estado `Disponible`.

## Escenario de validación de extremo a extremo

1. Iniciar sesión como Operador.
2. Registrar una emergencia con ubicación válida en el mapa.
3. Verificar que la emergencia aparece en el mapa operativo y en el dashboard.
4. Consultar unidades disponibles y asignar una a la emergencia.
5. Verificar que la unidad pasa a "Ocupada" y la asignación inicia en "Despachada".
6. Avanzar la asignación: "En ruta" → "En el lugar" → "Atendida".
7. Verificar que, con una sola unidad asignada, la emergencia pasa a "Atendida"
   cuando la asignación llega a ese estado (FR-016).
8. Iniciar sesión como Supervisor y cerrar la emergencia.
9. Verificar que la emergencia deja de aparecer como activa en mapa y dashboard, pero
   su timeline sigue siendo consultable completo.
10. Confirmar en el detalle de la emergencia que cada paso (creación, asignación,
    cambios de estado, cierre) aparece en la línea de tiempo con usuario y fecha/hora
    (FR-008, FR-010).

## Prueba de la regla de concurrencia (FR-007)

1. Con una única unidad `Disponible`, abrir dos solicitudes de asignación casi
   simultáneas hacia dos emergencias distintas (puede simularse con dos pestañas o
   dos llamadas a `POST /api/Emergencias/{id}/asignaciones`).
2. Verificar que solo una tiene éxito y la otra recibe `409 Conflict` con código
   `UNIDAD_NO_DISPONIBLE`.

Este documento se referencia desde `tasks.md` (etapa `/speckit-tasks`) como la prueba
de aceptación de extremo a extremo del MVP.

## Resultado de la ejecución (Polish, 2026-09-26)

Ejecutado con `curl` contra el backend real (`http://localhost:4401`, MySQL en
Docker) usando datos nuevos, sin reutilizar los de las pruebas de US1–US5. Los 10
pasos del escenario principal y los 2 de la prueba de concurrencia se verificaron
exitosamente — ver el detalle completo en la Bitácora de Ejecución, sección Polish.
No se ejecutó vía `docker compose up --build` (el `Dockerfile`/`docker-compose.yml`
del backend no se probó en esta sesión; se usó `dotnet run` directo, ya usado durante
todo Implement) — queda como pendiente explícito antes de la demo real en aula.
