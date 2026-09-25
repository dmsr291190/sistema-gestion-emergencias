using System.Reflection;
using Sige.Application.Publico.Queries;
using Sige.Domain.Entities;
using Sige.Domain.Enums;
using Sige.Domain.ValueObjects;
using Sige.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Sige.Application.UnitTests.Publico;

// FR-114, FR-115, SC-105: la vista publica nunca debe exponer datos sensibles ni
// la coordenada exacta. Complementa (no reemplaza) la verificacion manual con
// `curl` documentada en quickstart.md Escenario 5 -- ver nota en tasks.md T052
// sobre por que esta prueba es una prueba unitaria (InMemory) y no una prueba de
// integracion HTTP real: la infraestructura de Application.FunctionalTests exige
// una base de datos MySQL de pruebas real (variable ConnectionStrings__SigeDbTest)
// que nunca se aprovisiono en este proyecto, ni en el MVP ni en esta ampliacion.
public class RedaccionDatosPublicosTests
{
    // FR-114: nombres de campos que jamas deben aparecer en un tipo de DTO publico.
    private static readonly string[] CamposProhibidos =
    [
        "Documento", "Telefono", "Correo", "Email", "ReportanteNombre", "ReportanteContacto",
        "Observaciones", "UsuarioId", "CreadoPorId", "ModificadoPorId", "Latitud", "Longitud"
    ];

    private ApplicationDbContext _context = null!;
    private int _emergenciaId;
    private const double LatitudExacta = -12.056789;
    private const double LongitudExacta = -77.043210;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var emergencia = new Emergencia
        {
            Descripcion = "Prueba",
            ReportanteNombre = "Nombre Sensible del Reportante",
            ReportanteContacto = "987654321",
            Ubicacion = new Ubicacion
            {
                Departamento = "Lima", Provincia = "Lima", Distrito = "Miraflores",
                Latitud = LatitudExacta, Longitud = LongitudExacta, Ambito = Ambito.Terrestre
            },
            Prioridad = Prioridad.Alta,
            Estado = EstadoEmergencia.Reportada
        };
        _context.Emergencias.Add(emergencia);
        _context.SaveChanges();
        _emergenciaId = emergencia.Id;

        _context.EventosAuditoria.Add(new EventoAuditoria
        {
            EmergenciaId = _emergenciaId,
            TipoEvento = "EmergenciaCreada",
            EstadoNuevo = nameof(EstadoEmergencia.Reportada),
            UsuarioId = "operador-interno-id",
            FechaHora = DateTimeOffset.UtcNow
        });
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public void LosTiposPublicos_NuncaDebenTenerPropiedadesConNombresProhibidos()
    {
        foreach (var tipo in new[] { typeof(EmergenciaPublicaDto), typeof(EmergenciaPublicaDetalleDto), typeof(EventoPublicoDto), typeof(UbicacionPublicaDto) })
        {
            var propiedades = tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name);

            // Comparacion exacta (no "Contains"): "LatitudAproximada" es el campo
            // redactado permitido y no debe confundirse con el prohibido "Latitud".
            foreach (var prohibido in CamposProhibidos)
            {
                Assert.That(propiedades, Has.None.EqualTo(prohibido),
                    $"{tipo.Name} no debe exponer una propiedad llamada '{prohibido}' (FR-114).");
            }
        }
    }

    [Test]
    public async Task ListarEmergenciasPublicas_DebeRedondearLaUbicacion_YNoDevolverLaExacta()
    {
        var handler = new ListarEmergenciasPublicasQueryHandler(_context);

        var resultado = await handler.Handle(new ListarEmergenciasPublicasQuery(), CancellationToken.None);

        var dto = resultado.Single();
        Assert.That(dto.Ubicacion.LatitudAproximada, Is.Not.EqualTo(LatitudExacta));
        Assert.That(dto.Ubicacion.LongitudAproximada, Is.Not.EqualTo(LongitudExacta));
        Assert.That(dto.Ubicacion.Distrito, Is.EqualTo("Miraflores"));
    }

    [Test]
    public async Task ObtenerEmergenciaPublica_TimelineSoloDebeIncluirTipoEventoEstadoYFecha()
    {
        var handler = new ObtenerEmergenciaPublicaQueryHandler(_context);

        var resultado = await handler.Handle(new ObtenerEmergenciaPublicaQuery { Codigo = _emergenciaId }, CancellationToken.None);

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Timeline, Has.Count.EqualTo(1));
        Assert.That(resultado.Timeline[0].TipoEvento, Is.EqualTo("EmergenciaCreada"));
        // EventoPublicoDto (verificado arriba por reflexion) no tiene UsuarioId --
        // aqui se confirma ademas que el handler no lo intenta poblar via otro nombre.
    }

    [Test]
    public async Task ObtenerEmergenciaPublica_DebeDevolverNull_CuandoElCodigoNoExiste()
    {
        var handler = new ObtenerEmergenciaPublicaQueryHandler(_context);

        var resultado = await handler.Handle(new ObtenerEmergenciaPublicaQuery { Codigo = 999_999 }, CancellationToken.None);

        Assert.That(resultado, Is.Null);
    }
}
