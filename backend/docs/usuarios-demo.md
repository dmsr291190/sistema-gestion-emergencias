# Usuarios de demostración

**No exponer esta información en Producción** (FR-124: el generador de usuarios y
datos demo nunca se ejecuta en Production; este documento es solo para uso local
en Development/Demo/Testing).

Contraseña por defecto de todas las cuentas: la variable de entorno
`SIGE_DEMO_PASSWORD` si está definida, o `DemoSige#2026` si no lo está (ver
`.env.example`).

**Nota sobre el login**: el endpoint `POST /api/Users/login` recibe un campo
`email`, pero internamente lo usa como `UserName` (Identity), no como dirección
de correo. Inicia sesión con el valor de la columna "Usuario" de la tabla, no
con la columna "Correo".

| Usuario (login) | Correo | Rol | Notas |
|---|---|---|---|
| `operador@sige.local` | `operador@sige.local` | Operador | Cuenta original del MVP (`001-sige-mvp`) |
| `supervisor@sige.local` | `supervisor@sige.local` | Supervisor | Cuenta original del MVP |
| `admin.sige` | `admin.sige@sige.local` | Administrador | Administra usuarios y roles (US1) |
| `supervisor.sige` | `supervisor.sige@sige.local` | Supervisor | Cuenta nacional de la ampliación |
| `operador.lima` | `operador.lima@sige.local` | Operador | Región Lima |
| `operador.norte` | `operador.norte@sige.local` | Operador | Región Norte |
| `logistica.sige` | `logistica.sige@sige.local` | CoordinadorLogistico | Gestiona personal y recursos (US5) |
| `jefe.unidad01` | `jefe.unidad01@sige.local` | JefeDeUnidad | Gestiona personal y recursos de su unidad |
| `unidad.maritima01` | `unidad.maritima01@sige.local` | UnidadDeRespuesta | Cuenta propia vinculada a una embarcación demo (FR-120) |
| `unidad.terrestre01` | `unidad.terrestre01@sige.local` | UnidadDeRespuesta | Cuenta propia vinculada a una ambulancia demo (FR-120) |
| `visor.sige` | `visor.sige@sige.local` | Visualizador | Solo lectura: mapa + dashboard (FR-120a) |
| `prueba.restringida` | `prueba.restringida@sige.local` | Visualizador | Cuenta adicional de prueba de acceso restringido |

Fuente: `backend/src/Infrastructure/Data/ApplicationDbContextInitialiser.cs`
(`SeedUsuariosDemoAsync`), sección 19.1 del documento de origen
(`SIGE_Ampliacion_Operativa_Nacional.md`).
