using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sige.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AmpliacionOperativaNacional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Longitud",
                table: "Emergencias",
                newName: "Ubicacion_Longitud");

            migrationBuilder.RenameColumn(
                name: "Latitud",
                table: "Emergencias",
                newName: "Ubicacion_Latitud");

            migrationBuilder.AddColumn<int>(
                name: "InstitucionId",
                table: "UnidadesRespuesta",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion_Ambito",
                table: "UnidadesRespuesta",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion_CentroPoblado",
                table: "UnidadesRespuesta",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion_Departamento",
                table: "UnidadesRespuesta",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion_Direccion",
                table: "UnidadesRespuesta",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion_Distrito",
                table: "UnidadesRespuesta",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "Ubicacion_Latitud",
                table: "UnidadesRespuesta",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Ubicacion_Longitud",
                table: "UnidadesRespuesta",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion_Provincia",
                table: "UnidadesRespuesta",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion_Referencia",
                table: "UnidadesRespuesta",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Ubicacion_SinDireccionFormal",
                table: "UnidadesRespuesta",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioId",
                table: "UnidadesRespuesta",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "Emergencias",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Afectados",
                table: "Emergencias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Desaparecidos",
                table: "Emergencias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Evacuados",
                table: "Emergencias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Fallecidos",
                table: "Emergencias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Heridos",
                table: "Emergencias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TipoEmergenciaId",
                table: "Emergencias",
                type: "int",
                nullable: true);

            // defaultValue "Terrestre" (no "") para que las emergencias del MVP ya
            // existentes queden con un Ambito valido del enum -- corregido a mano
            // tras revisar la migracion generada por EF Core.
            migrationBuilder.AddColumn<string>(
                name: "Ubicacion_Ambito",
                table: "Emergencias",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Terrestre")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion_CentroPoblado",
                table: "Emergencias",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion_Departamento",
                table: "Emergencias",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion_Direccion",
                table: "Emergencias",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion_Distrito",
                table: "Emergencias",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion_Provincia",
                table: "Emergencias",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Ubicacion_Referencia",
                table: "Emergencias",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Ubicacion_SinDireccionFormal",
                table: "Emergencias",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CreadoPorId",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "InstitucionId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModificadoPorId",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NombreCompleto",
                table: "AspNetUsers",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "RequiereCambioPassword",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UltimoAcceso",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Instituciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastModified = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instituciones", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Recursos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Categoria = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnidadMedida = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    CantidadDisponible = table.Column<int>(type: "int", nullable: false),
                    CantidadMinima = table.Column<int>(type: "int", nullable: false),
                    UnidadRespuestaId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastModified = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recursos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recursos_UnidadesRespuesta_UnidadRespuestaId",
                        column: x => x.UnidadRespuestaId,
                        principalTable: "UnidadesRespuesta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TiposEmergencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ambito = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icono = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrioridadPorDefecto = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastModified = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposEmergencia", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Personal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombres = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Apellidos = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Documento = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InstitucionId = table.Column<int>(type: "int", nullable: true),
                    Especialidad = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Funcion = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Certificaciones = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Disponible = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UnidadRespuestaId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastModified = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Personal_Instituciones_InstitucionId",
                        column: x => x.InstitucionId,
                        principalTable: "Instituciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Personal_UnidadesRespuesta_UnidadRespuestaId",
                        column: x => x.UnidadRespuestaId,
                        principalTable: "UnidadesRespuesta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesRespuesta_InstitucionId",
                table: "UnidadesRespuesta",
                column: "InstitucionId");

            migrationBuilder.CreateIndex(
                name: "IX_Emergencias_TipoEmergenciaId",
                table: "Emergencias",
                column: "TipoEmergenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_InstitucionId",
                table: "AspNetUsers",
                column: "InstitucionId");

            migrationBuilder.CreateIndex(
                name: "IX_Instituciones_Nombre",
                table: "Instituciones",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personal_InstitucionId",
                table: "Personal",
                column: "InstitucionId");

            migrationBuilder.CreateIndex(
                name: "IX_Personal_UnidadRespuestaId",
                table: "Personal",
                column: "UnidadRespuestaId");

            migrationBuilder.CreateIndex(
                name: "IX_Recursos_UnidadRespuestaId",
                table: "Recursos",
                column: "UnidadRespuestaId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposEmergencia_Nombre",
                table: "TiposEmergencia",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Instituciones_InstitucionId",
                table: "AspNetUsers",
                column: "InstitucionId",
                principalTable: "Instituciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Emergencias_TiposEmergencia_TipoEmergenciaId",
                table: "Emergencias",
                column: "TipoEmergenciaId",
                principalTable: "TiposEmergencia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UnidadesRespuesta_Instituciones_InstitucionId",
                table: "UnidadesRespuesta",
                column: "InstitucionId",
                principalTable: "Instituciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Backfill manual: los usuarios del MVP ya existentes no tenian
            // NombreCompleto; se usa el UserName como valor de respaldo en vez de
            // dejarlo vacio.
            migrationBuilder.Sql("UPDATE AspNetUsers SET NombreCompleto = UserName WHERE NombreCompleto = '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Instituciones_InstitucionId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Emergencias_TiposEmergencia_TipoEmergenciaId",
                table: "Emergencias");

            migrationBuilder.DropForeignKey(
                name: "FK_UnidadesRespuesta_Instituciones_InstitucionId",
                table: "UnidadesRespuesta");

            migrationBuilder.DropTable(
                name: "Personal");

            migrationBuilder.DropTable(
                name: "Recursos");

            migrationBuilder.DropTable(
                name: "TiposEmergencia");

            migrationBuilder.DropTable(
                name: "Instituciones");

            migrationBuilder.DropIndex(
                name: "IX_UnidadesRespuesta_InstitucionId",
                table: "UnidadesRespuesta");

            migrationBuilder.DropIndex(
                name: "IX_Emergencias_TipoEmergenciaId",
                table: "Emergencias");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_InstitucionId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "InstitucionId",
                table: "UnidadesRespuesta");

            migrationBuilder.DropColumn(
                name: "Ubicacion_Ambito",
                table: "UnidadesRespuesta");

            migrationBuilder.DropColumn(
                name: "Ubicacion_CentroPoblado",
                table: "UnidadesRespuesta");

            migrationBuilder.DropColumn(
                name: "Ubicacion_Departamento",
                table: "UnidadesRespuesta");

            migrationBuilder.DropColumn(
                name: "Ubicacion_Direccion",
                table: "UnidadesRespuesta");

            migrationBuilder.DropColumn(
                name: "Ubicacion_Distrito",
                table: "UnidadesRespuesta");

            migrationBuilder.DropColumn(
                name: "Ubicacion_Latitud",
                table: "UnidadesRespuesta");

            migrationBuilder.DropColumn(
                name: "Ubicacion_Longitud",
                table: "UnidadesRespuesta");

            migrationBuilder.DropColumn(
                name: "Ubicacion_Provincia",
                table: "UnidadesRespuesta");

            migrationBuilder.DropColumn(
                name: "Ubicacion_Referencia",
                table: "UnidadesRespuesta");

            migrationBuilder.DropColumn(
                name: "Ubicacion_SinDireccionFormal",
                table: "UnidadesRespuesta");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "UnidadesRespuesta");

            migrationBuilder.DropColumn(
                name: "Afectados",
                table: "Emergencias");

            migrationBuilder.DropColumn(
                name: "Desaparecidos",
                table: "Emergencias");

            migrationBuilder.DropColumn(
                name: "Evacuados",
                table: "Emergencias");

            migrationBuilder.DropColumn(
                name: "Fallecidos",
                table: "Emergencias");

            migrationBuilder.DropColumn(
                name: "Heridos",
                table: "Emergencias");

            migrationBuilder.DropColumn(
                name: "TipoEmergenciaId",
                table: "Emergencias");

            migrationBuilder.DropColumn(
                name: "Ubicacion_Ambito",
                table: "Emergencias");

            migrationBuilder.DropColumn(
                name: "Ubicacion_CentroPoblado",
                table: "Emergencias");

            migrationBuilder.DropColumn(
                name: "Ubicacion_Departamento",
                table: "Emergencias");

            migrationBuilder.DropColumn(
                name: "Ubicacion_Direccion",
                table: "Emergencias");

            migrationBuilder.DropColumn(
                name: "Ubicacion_Distrito",
                table: "Emergencias");

            migrationBuilder.DropColumn(
                name: "Ubicacion_Provincia",
                table: "Emergencias");

            migrationBuilder.DropColumn(
                name: "Ubicacion_Referencia",
                table: "Emergencias");

            migrationBuilder.DropColumn(
                name: "Ubicacion_SinDireccionFormal",
                table: "Emergencias");

            migrationBuilder.DropColumn(
                name: "CreadoPorId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "InstitucionId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ModificadoPorId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NombreCompleto",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RequiereCambioPassword",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UltimoAcceso",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Ubicacion_Longitud",
                table: "Emergencias",
                newName: "Longitud");

            migrationBuilder.RenameColumn(
                name: "Ubicacion_Latitud",
                table: "Emergencias",
                newName: "Latitud");

            migrationBuilder.UpdateData(
                table: "Emergencias",
                keyColumn: "Tipo",
                keyValue: null,
                column: "Tipo",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "Emergencias",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
