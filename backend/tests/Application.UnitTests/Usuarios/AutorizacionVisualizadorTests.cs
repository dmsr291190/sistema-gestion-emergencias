using Sige.Application.Common.Behaviours;
using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Usuarios.Commands.BloquearUsuario;
using Sige.Application.Usuarios.Commands.CrearUsuario;
using Sige.Application.Usuarios.Commands.DesbloquearUsuario;
using Sige.Application.Usuarios.Commands.EditarUsuario;
using Sige.Application.Usuarios.Commands.ForzarCambioPassword;
using Sige.Application.Usuarios.Queries;
using Sige.Domain.Constants;
using Moq;
using NUnit.Framework;

namespace Sige.Application.UnitTests.Usuarios;

// FR-120a: el rol "Visualizador" se limita a mapa + dashboard en solo lectura;
// no tiene acceso a la administracion de usuarios. La misma restriccion se
// vuelve a verificar para Personal/Recursos en Application.UnitTests/Personal
// y /Recursos cuando esas Commands existan (US5).
public class AutorizacionVisualizadorTests
{
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
        _user.Setup(u => u.Id).Returns("visor-1");
        _user.Setup(u => u.Roles).Returns([Roles.Visualizador]);
    }

    [Test]
    public void DebeRechazar_CrearUsuario()
    {
        var behaviour = new AuthorizationBehaviour<CrearUsuarioCommand, string>(_user.Object, _identityService.Object);
        var comando = new CrearUsuarioCommand { UserName = "x", NombreCompleto = "x", Password = "Password1!", Roles = [Roles.Visualizador] };

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await behaviour.Handle(comando, _ => Task.FromResult("id"), CancellationToken.None));
    }

    [Test]
    public void DebeRechazar_EditarUsuario()
    {
        var behaviour = new AuthorizationBehaviour<EditarUsuarioCommand, MediatR.Unit>(_user.Object, _identityService.Object);
        var comando = new EditarUsuarioCommand { UserId = "u-1", NombreCompleto = "x", Roles = [Roles.Visualizador] };

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await behaviour.Handle(comando, _ => Task.FromResult(MediatR.Unit.Value), CancellationToken.None));
    }

    [Test]
    public void DebeRechazar_DesbloquearUsuario()
    {
        var behaviour = new AuthorizationBehaviour<DesbloquearUsuarioCommand, MediatR.Unit>(_user.Object, _identityService.Object);

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await behaviour.Handle(new DesbloquearUsuarioCommand { UserId = "u-1" }, _ => Task.FromResult(MediatR.Unit.Value), CancellationToken.None));
    }

    [Test]
    public void DebeRechazar_ForzarCambioPassword()
    {
        var behaviour = new AuthorizationBehaviour<ForzarCambioPasswordCommand, MediatR.Unit>(_user.Object, _identityService.Object);

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await behaviour.Handle(new ForzarCambioPasswordCommand { UserId = "u-1", PasswordTemporal = "Temp123!" }, _ => Task.FromResult(MediatR.Unit.Value), CancellationToken.None));
    }

    [Test]
    public void DebeRechazar_ListarUsuarios()
    {
        var behaviour = new AuthorizationBehaviour<ListarUsuariosQuery, List<UsuarioAdminDto>>(_user.Object, _identityService.Object);

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await behaviour.Handle(new ListarUsuariosQuery(), _ => Task.FromResult(new List<UsuarioAdminDto>()), CancellationToken.None));
    }
}
