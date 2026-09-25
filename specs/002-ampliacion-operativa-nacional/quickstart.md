# Quickstart de Validación: Ampliación Operativa Nacional

Requiere el entorno del MVP ya funcionando (`001-sige-mvp/quickstart.md`): MySQL
en Docker (`mysql_local`), backend en `http://localhost:4401`, frontend en
`http://localhost:4400`.

## Prerrequisitos

- Rama `feature/ampliacion-operativa-nacional` con `dotnet ef database update`
  aplicado (incluye la migración de esquema + el seed extendido de
  `ApplicationDbContextInitialiser`).
- Variable de entorno `SIGE_DEMO_PASSWORD` definida (o se usa el default
  `DemoSige#2026` solo en Development/Demo/Testing).
- Entorno del backend en `Development` (o `Demo`/`Testing`) para que el seed de
  usuarios/datos demo se ejecute (FR-122, FR-124).

## Escenario 1 — Seed idempotente de usuarios y datos demo (US2)

```bash
# Primer arranque: debe crear los 10 usuarios demo y el volumen de datos demo
cd backend && dotnet run --project src/Web

# Verificar (en otra terminal) que existen los 10 usuarios documentados
curl -s -X POST http://localhost:4401/api/Users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin.sige","password":"DemoSige#2026"}'
# → 200 con accessToken

# Contar registros antes de reiniciar
mysql ... -e "SELECT COUNT(*) FROM AspNetUsers; SELECT COUNT(*) FROM Emergencias;"

# Reiniciar el backend y volver a contar — los conteos NO deben cambiar (FR-123, SC-103)
```

**Resultado esperado**: mismos conteos antes/después del segundo arranque.

## Escenario 2 — Administración de usuarios y roles (US1)

```bash
TOKEN_ADMIN=$(curl -s -X POST http://localhost:4401/api/Users/login \
  -d '{"email":"admin.sige","password":"DemoSige#2026"}' | jq -r .accessToken)

curl -s -X POST http://localhost:4401/api/Usuarios \
  -H "Authorization: Bearer $TOKEN_ADMIN" -H "Content-Type: application/json" \
  -d '{"nombre":"Prueba QA","documento":"12345678","email":"qa.prueba@sige.pe","rol":"CoordinadorLogistico"}'
# → 201

curl -s -X POST http://localhost:4401/api/Usuarios/{id}/bloquear -H "Authorization: Bearer $TOKEN_ADMIN"
curl -s -X POST http://localhost:4401/api/Users/login -d '{"email":"qa.prueba@sige.pe","password":"..."}'
# → 401/403 con mensaje de cuenta bloqueada
```

## Escenario 3 — Login propio de "Unidad de respuesta" (Clarification 1)

```bash
curl -s -X POST http://localhost:4401/api/Users/login \
  -d '{"email":"unidad.terrestre01","password":"DemoSige#2026"}'
# → 200; el token solo permite acceder a "mi unidad" (estado/posición propios)
```

## Escenario 4 — Catálogo de tipos + migración del MVP (US3, FR-103)

```bash
# Verificar que las emergencias creadas en el MVP (001) ya tienen TipoEmergenciaId
curl -s http://localhost:4401/api/Emergencias -H "Authorization: Bearer $TOKEN_ADMIN" | jq '.[0].tipoEmergencia'
# → no debe ser null para ninguna emergencia preexistente
```

## Escenario 5 — Vista pública sin redactar datos sensibles (US7, SC-105)

```bash
curl -s http://localhost:4401/api/Publico/Emergencias | jq '.'
# Inspeccionar el JSON completo:
#   - NO debe contener: documento, telefono, correo, nombreReportante, nombreVictima,
#     observacionesInternas, latitud/longitud exactas, usuario interno.
#   - SÍ debe contener: codigo, tipo, prioridad, estado, distrito/centroPoblado.
```

## Escenario 6 — Personal y recursos con alerta de bajo stock (US5)

```bash
curl -s -X POST http://localhost:4401/api/Unidades/1/recursos \
  -H "Authorization: Bearer $TOKEN_ADMIN" -H "Content-Type: application/json" \
  -d '{"codigo":"AGUA-01","nombre":"Bidones de agua","categoria":"Liquidos","cantidad":50,"cantidadDisponible":3,"cantidadMinima":10}'

curl -s http://localhost:4401/api/Unidades/1/recursos -H "Authorization: Bearer $TOKEN_ADMIN" | jq '.[] | select(.bajoStock == true)'
# → debe listar el recurso recién creado
```

## Escenario 7 — Mapa avanzado y tema oscuro (US4, US9 — manual en navegador)

1. Abrir `http://localhost:4400/mapa` autenticado.
2. Activar/desactivar al menos dos capas de la leyenda (FR-107).
3. Aplicar un filtro por tipo y verificar que el mapa se reduce a esos marcadores
   (FR-108).
4. Hacer clic en un marcador y confirmar que abre un resumen sin abandonar el
   mapa (FR-109).
5. Alternar el tema oscuro y confirmar que tablas, cards y modales mantienen
   contraste legible, incluidos los colores de prioridad/estado del MVP
   (FR-128).

## Escenario 8 — No regresión del MVP (SC-106)

Repetir el flujo completo de `001-sige-mvp/quickstart.md` (registrar → validar →
asignar → avanzar → cerrar → reabrir) sin ninguna modificación, y confirmar que
sigue funcionando de punta a punta después de esta ampliación.
