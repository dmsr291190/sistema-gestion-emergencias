using Sige.Application.Emergencias.Commands.CrearEmergencia;
using Sige.Domain.Enums;
using NUnit.Framework;

namespace Sige.Application.UnitTests.Emergencias;

// FR-001, FR-015, FR-019, FR-104, FR-105 (ampliacion 002).
public class CrearEmergenciaValidatorTests
{
    private readonly CrearEmergenciaCommandValidator _validator = new();

    private static UbicacionInput UbicacionValida() => new()
    {
        Departamento = "Lima",
        Provincia = "Lima",
        Distrito = "Miraflores",
        Latitud = -12.05,
        Longitud = -77.03,
        Ambito = Ambito.Terrestre,
        SinDireccionFormal = false
    };

    private static CrearEmergenciaCommand ComandoValido() => new()
    {
        TipoEmergenciaId = 1,
        Descripcion = "Persona desmayada",
        Ubicacion = UbicacionValida(),
        Prioridad = Prioridad.Alta,
        ReportanteNombre = "Juan Perez"
    };

    [Test]
    public void DebePasar_CuandoTodosLosCamposObligatoriosEstanCompletos()
    {
        var result = _validator.Validate(ComandoValido());

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void DebeFallar_CuandoDescripcionEstaVacia()
    {
        var comando = ComandoValido() with { Descripcion = "" };

        var result = _validator.Validate(comando);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void DebeFallar_CuandoReportanteNombreEstaVacio()
    {
        // FR-019: el nombre del reportante es obligatorio.
        var comando = ComandoValido() with { ReportanteNombre = "" };

        var result = _validator.Validate(comando);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void DebePasar_CuandoReportanteContactoEsNulo()
    {
        // FR-019: el contacto del reportante es opcional.
        var comando = ComandoValido() with { ReportanteContacto = null };

        var result = _validator.Validate(comando);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void DebeFallar_CuandoLatitudEstaFueraDeRango()
    {
        var comando = ComandoValido() with { Ubicacion = UbicacionValida() with { Latitud = 200 } };

        var result = _validator.Validate(comando);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void DebeFallar_CuandoFaltaDistritoYNoEsSinDireccionFormal()
    {
        // Corregido tras Analyze (I2): el distrito es obligatorio salvo que
        // SinDireccionFormal sea true -- no depende del Ambito.
        var comando = ComandoValido() with { Ubicacion = UbicacionValida() with { Distrito = null } };

        var result = _validator.Validate(comando);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void DebePasar_CuandoSinDireccionFormalYSinDistrito()
    {
        // FR-104: zona maritima o terrestre remota sin direccion formal.
        var comando = ComandoValido() with
        {
            Ubicacion = UbicacionValida() with { Distrito = null, Provincia = null, Departamento = null, SinDireccionFormal = true, Ambito = Ambito.Maritimo }
        };

        var result = _validator.Validate(comando);

        Assert.That(result.IsValid, Is.True);
    }
}
