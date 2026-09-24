using Sige.Application.Emergencias.Commands.CrearEmergencia;
using Sige.Domain.Enums;
using NUnit.Framework;

namespace Sige.Application.UnitTests.Emergencias;

// FR-001, FR-015, FR-019.
public class CrearEmergenciaValidatorTests
{
    private readonly CrearEmergenciaCommandValidator _validator = new();

    private static CrearEmergenciaCommand ComandoValido() => new()
    {
        Tipo = "Medica",
        Descripcion = "Persona desmayada",
        Latitud = -12.05,
        Longitud = -77.03,
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
        var comando = ComandoValido() with { Latitud = 200 };

        var result = _validator.Validate(comando);

        Assert.That(result.IsValid, Is.False);
    }
}
