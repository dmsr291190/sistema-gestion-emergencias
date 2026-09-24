using Sige.Application.Common.Behaviours;
using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Unidades.Commands.CrearUnidad;
using Sige.Domain.Constants;
using Sige.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace Sige.Application.UnitTests.Unidades;

// FR-004: alta y cambio de estado de unidades restringidos al rol Supervisor.
public class AutorizacionUnidadesTests
{
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;
    private AuthorizationBehaviour<CrearUnidadCommand, int> _behaviour = null!;

    [SetUp]
    public void Setup()
    {
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
        _behaviour = new AuthorizationBehaviour<CrearUnidadCommand, int>(_user.Object, _identityService.Object);
    }

    private static CrearUnidadCommand Comando() => new()
    {
        Tipo = TipoUnidad.Ambulancia,
        Identificador = "AMB-99"
    };

    [Test]
    public void DebeRechazar_CuandoElUsuarioNoEsSupervisor()
    {
        _user.Setup(u => u.Id).Returns("op-1");
        _user.Setup(u => u.Roles).Returns([Roles.Operador]);

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await _behaviour.Handle(Comando(), _ => Task.FromResult(1), CancellationToken.None));
    }

    [Test]
    public async Task DebePermitir_CuandoElUsuarioEsSupervisor()
    {
        _user.Setup(u => u.Id).Returns("sup-1");
        _user.Setup(u => u.Roles).Returns([Roles.Supervisor]);

        var resultado = await _behaviour.Handle(Comando(), _ => Task.FromResult(42), CancellationToken.None);

        Assert.That(resultado, Is.EqualTo(42));
    }

    [Test]
    public void DebeRechazar_CuandoNoHayUsuarioAutenticado()
    {
        _user.Setup(u => u.Id).Returns((string?)null);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _behaviour.Handle(Comando(), _ => Task.FromResult(1), CancellationToken.None));
    }
}
