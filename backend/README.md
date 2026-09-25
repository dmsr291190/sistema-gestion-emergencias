# SIGE — Backend

API del Sistema Integral de Gestión de Emergencias (SIGE), construida con
[Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/CleanArchitecture)
(.NET 10) sobre MySQL. Todo el proyecto — MVP y ampliación operativa nacional —
se desarrolló siguiendo Spec-Driven Development; las especificaciones,
decisiones y tareas de cada feature viven en `../specs/`.

## Stack

- .NET 10, Clean Architecture (Domain / Application / Infrastructure / Web)
- MediatR (CQRS) + FluentValidation
- ASP.NET Core Identity (tokens de acceso opaquos vía `AddBearerToken`)
- EF Core 9 + Pomelo.EntityFrameworkCore.MySql
- MySQL 8.0 (contenedor Docker `mysql_local`, base de datos `SigeDb`)

## Base de datos

- Migraciones EF Core: `src/Infrastructure/Data/Migrations/`
- **Estructura de referencia (solo esquema, sin datos)**: [`database/schema.sql`](database/schema.sql) —
  volcado con `mysqldump --no-data` de la base ya migrada; útil para revisar
  tablas, columnas, índices y llaves foráneas sin tener que levantar MySQL.
  Se regenera después de cada migración nueva con:

  ```bash
  docker exec mysql_local mysqldump -u sige_app -p --no-data --no-tablespaces \
    --routines --triggers --skip-comments SigeDb > database/schema.sql
  ```

- Al arrancar en un entorno de Desarrollo/Demo/Pruebas, `ApplicationDbContextInitialiser`
  aplica las migraciones pendientes y siembra roles, catálogo de tipos de
  emergencia, instituciones y datos demo automáticamente — no requiere pasos
  manuales ni scripts de importación.
- Usuarios de prueba (usuario/contraseña/rol de cada cuenta demo): ver
  [`../usuarios_prueba.txt`](../usuarios_prueba.txt) en la raíz del repositorio.

## Build

Run `dotnet build` to build the solution.

## Run

Con MySQL corriendo en Docker (`mysql_local`, puerto 3306) y la cadena de
conexión configurada en `src/Web/appsettings.Development.json`:

```bash
dotnet run --project ./src/Web
```

La API queda disponible en `http://localhost:4401` (puerto fijado por el
proyecto de curso). También puede levantarse junto al frontend con
`docker-compose.yml` en la raíz del repositorio.

## Code Styles & Formatting

The template includes [EditorConfig](https://editorconfig.org/) support to help maintain consistent coding styles for multiple developers working on the same project across various editors and IDEs. The **.editorconfig** file defines the coding styles applicable to this solution.

## Code Scaffolding

The template includes support to scaffold new commands and queries.

Start in the `.\src\Application\` folder.

Create a new command:

```
dotnet new ca-usecase --name CreateTodoList --feature-name TodoLists --usecase-type command --return-type int
```

Create a new query:

```
dotnet new ca-usecase -n GetTodos -fn TodoLists -ut query -rt TodosVm
```

If you encounter the error *"No templates or subcommands found matching: 'ca-usecase'."*, install the template and try again:

```bash
dotnet new install Clean.Architecture.Solution.Template::10.8.0
```

## Test

The solution contains unit, integration, and functional tests.

To run the tests:
```bash
dotnet test
```

## Help
To learn more about the template go to the [project website](https://cleanarchitecture.jasontaylor.dev). Here you can find additional guidance, request new features, report a bug, and discuss the template with other users.
