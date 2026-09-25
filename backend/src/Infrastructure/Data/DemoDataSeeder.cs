using Sige.Domain.Entities;
using Sige.Domain.Enums;
using Sige.Domain.ValueObjects;
using Sige.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Sige.Infrastructure.Data;

// FR-122: volumen de datos demo variado (sección 20 del documento de origen:
// 40-60 emergencias, 25-40 unidades, 60-100 personal, 100+ recursos). Se invoca
// desde ApplicationDbContextInitialiser solo en entornos permitidos (T016).
public static class DemoDataSeeder
{
    private static readonly (string Nombre, Ambito Ambito, string Icono, string Color, Prioridad Prioridad)[] TiposDemo =
    [
        ("Incendio", Ambito.Terrestre, "cilFire", "#e55353", Prioridad.Alta),
        ("Emergencia Médica", Ambito.Terrestre, "cilMedicalCross", "#e55353", Prioridad.Alta),
        ("Accidente de Tránsito", Ambito.Terrestre, "cilCarAlt", "#f9b115", Prioridad.Media),
        ("Emergencia Marítima", Ambito.Maritimo, "cilBoatAlt", "#3399ff", Prioridad.Alta),
        ("Huaico / Derrumbe", Ambito.Terrestre, "cilTerrain", "#f9b115", Prioridad.Critica),
        ("Inundación", Ambito.Terrestre, "cilRain", "#3399ff", Prioridad.Alta),
        ("Material Peligroso", Ambito.Mixto, "cilWarning", "#e55353", Prioridad.Critica),
        ("Emergencia Aérea", Ambito.Aereo, "cilAirplaneMode", "#e55353", Prioridad.Critica),
        ("Búsqueda y Rescate", Ambito.Mixto, "cilLifeRing", "#f9b115", Prioridad.Media)
    ];

    // Coordenadas aproximadas dentro del territorio peruano, solo para demo.
    private static readonly (string Departamento, string Provincia, string Distrito, double Lat, double Lon)[] UbicacionesDemo =
    [
        ("Lima", "Lima", "Miraflores", -12.1211, -77.0296),
        ("Lima", "Lima", "San Isidro", -12.0977, -77.0365),
        ("Lima", "Lima", "Callao", -12.0566, -77.1181),
        ("La Libertad", "Trujillo", "Trujillo", -8.1116, -79.0288),
        ("Piura", "Piura", "Piura", -5.1945, -80.6328),
        ("Arequipa", "Arequipa", "Cercado", -16.4090, -71.5375),
        ("Cusco", "Cusco", "Cusco", -13.5319, -71.9675),
        ("Loreto", "Maynas", "Iquitos", -3.7491, -73.2538),
        ("Áncash", "Huaraz", "Huaraz", -9.5277, -77.5278),
        ("Lambayeque", "Chiclayo", "Chiclayo", -6.7714, -79.8409)
    ];

    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        // Idempotencia (FR-123): Personal es enteramente nuevo de esta ampliación,
        // y las unidades demo usan el prefijo "DEMO-" -- si existe cualquiera de
        // los dos, el volumen demo ya fue generado (o quedó a medias en un intento
        // anterior), y no se debe repetir.
        var yaSembrado = await context.Personal.AnyAsync()
            || await context.UnidadesRespuesta.AnyAsync(u => u.Identificador.StartsWith("DEMO-"));

        if (yaSembrado)
        {
            return;
        }

        var tipos = await SeedTiposEmergenciaAsync(context);
        var instituciones = await context.Instituciones.ToListAsync();

        var unidadMaritimaUser = await userManager.FindByNameAsync("unidad.maritima01");
        var unidadTerrestreUser = await userManager.FindByNameAsync("unidad.terrestre01");

        var unidades = CrearUnidadesDemo(instituciones, unidadMaritimaUser, unidadTerrestreUser);
        context.UnidadesRespuesta.AddRange(unidades);
        await context.SaveChangesAsync();

        var random = new Random(2026);

        foreach (var unidad in unidades)
        {
            unidad.Personal.Add(new Personal
            {
                Nombres = "Personal",
                Apellidos = $"Demo {unidad.Identificador}-1",
                Documento = $"DEMO{unidad.Id:D5}1",
                InstitucionId = unidad.InstitucionId,
                Especialidad = "Paramédico",
                Funcion = "Rescatista",
                Disponible = true,
                UnidadRespuestaId = unidad.Id
            });
            unidad.Personal.Add(new Personal
            {
                Nombres = "Personal",
                Apellidos = $"Demo {unidad.Identificador}-2",
                Documento = $"DEMO{unidad.Id:D5}2",
                InstitucionId = unidad.InstitucionId,
                Especialidad = "Conductor",
                Funcion = "Operador de unidad",
                Disponible = true,
                UnidadRespuestaId = unidad.Id
            });

            foreach (var categoria in Enum.GetValues<CategoriaRecurso>())
            {
                var minima = random.Next(5, 15);
                var cantidad = random.Next(minima + 5, minima + 40);
                // ~1 de cada 6 recursos queda intencionalmente bajo el minimo,
                // para poder demostrar la alerta de bajo stock (FR-112).
                var disponible = random.Next(0, 6) == 0 ? Math.Max(0, minima - 2) : random.Next(minima, cantidad);

                context.Recursos.Add(new Recurso
                {
                    Codigo = $"{categoria}-{unidad.Identificador}",
                    Nombre = $"{categoria} de {unidad.Identificador}",
                    Categoria = categoria,
                    UnidadMedida = "unidad",
                    Cantidad = cantidad,
                    CantidadDisponible = disponible,
                    CantidadMinima = minima,
                    UnidadRespuestaId = unidad.Id
                });
            }
        }

        await context.SaveChangesAsync();

        var operador = await userManager.FindByNameAsync("operador.lima") ?? await userManager.FindByNameAsync("operador@sige.local");

        for (var i = 0; i < 45; i++)
        {
            var tipo = tipos[random.Next(tipos.Count)];
            var loc = UbicacionesDemo[random.Next(UbicacionesDemo.Length)];
            var esMaritimoOAereo = tipo.Ambito is Ambito.Maritimo or Ambito.Aereo;

            context.Emergencias.Add(new Emergencia
            {
                TipoEmergenciaId = tipo.Id,
                Tipo = tipo.Nombre,
                Descripcion = $"Emergencia demo #{i + 1} ({tipo.Nombre}) generada automáticamente para pruebas.",
                Ubicacion = new Ubicacion
                {
                    Departamento = esMaritimoOAereo ? null : loc.Departamento,
                    Provincia = esMaritimoOAereo ? null : loc.Provincia,
                    Distrito = esMaritimoOAereo ? null : loc.Distrito,
                    Latitud = loc.Lat + (random.NextDouble() - 0.5) * 0.2,
                    Longitud = loc.Lon + (random.NextDouble() - 0.5) * 0.2,
                    Ambito = tipo.Ambito,
                    SinDireccionFormal = esMaritimoOAereo
                },
                Afectados = random.Next(0, 20),
                Heridos = random.Next(0, 5),
                Desaparecidos = random.Next(0, 2),
                Fallecidos = 0,
                Evacuados = random.Next(0, 10),
                Prioridad = tipo.PrioridadPorDefecto,
                FechaHoraReporte = DateTimeOffset.UtcNow.AddHours(-random.Next(1, 240)),
                ReportanteNombre = "Reportante Demo",
                Estado = EstadoEmergencia.Reportada,
                CreadoPorId = operador?.Id
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task<List<TipoEmergencia>> SeedTiposEmergenciaAsync(ApplicationDbContext context)
    {
        var existentes = await context.TiposEmergencia.ToListAsync();
        var resultado = new List<TipoEmergencia>(existentes);

        foreach (var (nombre, ambito, icono, color, prioridad) in TiposDemo)
        {
            if (existentes.Any(t => t.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var tipo = new TipoEmergencia
            {
                Nombre = nombre,
                Ambito = ambito,
                Icono = icono,
                Color = color,
                PrioridadPorDefecto = prioridad,
                Activo = true
            };

            context.TiposEmergencia.Add(tipo);
            resultado.Add(tipo);
        }

        await context.SaveChangesAsync();

        return resultado;
    }

    private static List<UnidadRespuesta> CrearUnidadesDemo(List<Institucion> instituciones, ApplicationUser? unidadMaritimaUser, ApplicationUser? unidadTerrestreUser)
    {
        var tiposUnidad = Enum.GetValues<TipoUnidad>();
        var unidades = new List<UnidadRespuesta>();

        for (var i = 1; i <= 30; i++)
        {
            var tipo = tiposUnidad[i % tiposUnidad.Length];
            var estado = i % 5 == 0 ? EstadoOperativoUnidad.FueraDeServicio : i % 3 == 0 ? EstadoOperativoUnidad.Ocupada : EstadoOperativoUnidad.Disponible;
            var institucion = instituciones.Count > 0 ? instituciones[i % instituciones.Count] : null;

            var unidad = new UnidadRespuesta
            {
                Tipo = tipo,
                Identificador = $"DEMO-{tipo.ToString().ToUpperInvariant()[..3]}-{i:D3}",
                EstadoOperativo = estado,
                InstitucionId = institucion?.Id
            };

            // FR-120: se vincula una unidad marítima y una terrestre a las
            // cuentas demo que inician sesión como "Unidad de respuesta".
            if (tipo == TipoUnidad.Embarcacion && unidad.UsuarioId == null && unidadMaritimaUser != null && unidades.All(u => u.UsuarioId != unidadMaritimaUser.Id))
            {
                unidad.UsuarioId = unidadMaritimaUser.Id;
            }
            else if (tipo == TipoUnidad.Ambulancia && unidadTerrestreUser != null && unidades.All(u => u.UsuarioId != unidadTerrestreUser.Id))
            {
                unidad.UsuarioId = unidadTerrestreUser.Id;
            }

            unidades.Add(unidad);
        }

        return unidades;
    }
}
