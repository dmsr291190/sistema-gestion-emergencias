using Sige.Application.Common.Behaviours;
using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Emergencias.Commands.CerrarEmergencia;
using Sige.Application.Emergencias.Commands.ReabrirEmergencia;
using Sige.Domain.Constants;
using MediatR;
using Moq;
using NUnit.Framework;

namespace Sige.Application.UnitTests.Emergencias;

// FR-017: cerrar y reabrir una emergencia estan restringidos al rol Supervisor.
public class AutorizacionCierreTests
{
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public void CerrarEmergencia_DebeRechazar_CuandoElUsuarioNoEsSupervisor()
    {
        var behaviour = new AuthorizationBehaviour<CerrarEmergenciaCommand, Unit>(_user.Object, _identityService.Object);
        _user.Setup(u => u.Id).Returns("op-1");
        _user.Setup(u => u.Roles).Returns([Roles.Operador]);

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await behaviour.Handle(
                new CerrarEmergenciaCommand { EmergenciaId = 1 },
                _ => Task.FromResult(Unit.Value),
                CancellationToken.None));
    }

    [Test]
    public async Task CerrarEmergencia_DebePermitir_CuandoElUsuarioEsSupervisor()
    {
        var behaviour = new AuthorizationBehaviour<CerrarEmergenciaCommand, Unit>(_user.Object, _identityService.Object);
        _user.Setup(u => u.Id).Returns("sup-1");
        _user.Setup(u => u.Roles).Returns([Roles.Supervisor]);

        await behaviour.Handle(
            new CerrarEmergenciaCommand { EmergenciaId = 1 },
            _ => Task.FromResult(Unit.Value),
            CancellationToken.None);
    }

    [Test]
    public void ReabrirEmergencia_DebeRechazar_CuandoElUsuarioNoEsSupervisor()
    {
        var behaviour = new AuthorizationBehaviour<ReabrirEmergenciaCommand, Unit>(_user.Object, _identityService.Object);
        _user.Setup(u => u.Id).Returns("op-1");
        _user.Setup(u => u.Roles).Returns([Roles.Operador]);

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await behaviour.Handle(
                new ReabrirEmergenciaCommand { EmergenciaId = 1 },
                _ => Task.FromResult(Unit.Value),
                CancellationToken.None));
    }
}
