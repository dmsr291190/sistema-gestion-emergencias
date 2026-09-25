using Sige.Application.Common.Behaviours;
using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.Usuarios.Commands.BloquearUsuario;
using Sige.Application.Usuarios.Commands.CrearUsuario;
using Sige.Application.Usuarios.Queries;
using Sige.Domain.Constants;
using Moq;
using NUnit.Framework;

namespace Sige.Application.UnitTests.Usuarios;

// FR-116: la administracion de usuarios se restringe exclusivamente al rol
// Administrador.
public class AutorizacionUsuariosTests
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
    public void CrearUsuario_DebeRechazar_CuandoElUsuarioNoEsAdministrador()
    {
        _user.Setup(u => u.Id).Returns("op-1");
        _user.Setup(u => u.Roles).Returns([Roles.Operador]);

        var behaviour = new AuthorizationBehaviour<CrearUsuarioCommand, string>(_user.Object, _identityService.Object);
        var comando = new CrearUsuarioCommand { UserName = "prueba", NombreCompleto = "Prueba", Password = "Prueba123!", Roles = [Roles.Visualizador] };

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await behaviour.Handle(comando, _ => Task.FromResult("id-1"), CancellationToken.None));
    }

    [Test]
    public async Task CrearUsuario_DebePermitir_CuandoElUsuarioEsAdministrador()
    {
        _user.Setup(u => u.Id).Returns("admin-1");
        _user.Setup(u => u.Roles).Returns([Roles.Administrador]);

        var behaviour = new AuthorizationBehaviour<CrearUsuarioCommand, string>(_user.Object, _identityService.Object);
        var comando = new CrearUsuarioCommand { UserName = "prueba", NombreCompleto = "Prueba", Password = "Prueba123!", Roles = [Roles.Visualizador] };

        var resultado = await behaviour.Handle(comando, _ => Task.FromResult("id-1"), CancellationToken.None);

        Assert.That(resultado, Is.EqualTo("id-1"));
    }

    [Test]
    public void BloquearUsuario_DebeRechazar_CuandoElUsuarioNoEsAdministrador()
    {
        _user.Setup(u => u.Id).Returns("sup-1");
        _user.Setup(u => u.Roles).Returns([Roles.Supervisor]);

        var behaviour = new AuthorizationBehaviour<BloquearUsuarioCommand, MediatR.Unit>(_user.Object, _identityService.Object);

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await behaviour.Handle(new BloquearUsuarioCommand { UserId = "u-1" }, _ => Task.FromResult(MediatR.Unit.Value), CancellationToken.None));
    }

    [Test]
    public void ListarUsuarios_DebeRechazar_CuandoElUsuarioNoEsAdministrador()
    {
        _user.Setup(u => u.Id).Returns("visor-1");
        _user.Setup(u => u.Roles).Returns([Roles.Visualizador]);

        var behaviour = new AuthorizationBehaviour<ListarUsuariosQuery, List<UsuarioAdminDto>>(_user.Object, _identityService.Object);

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await behaviour.Handle(new ListarUsuariosQuery(), _ => Task.FromResult(new List<UsuarioAdminDto>()), CancellationToken.None));
    }

    [Test]
    public void ListarUsuarios_DebeRechazar_SinUsuarioAutenticado()
    {
        _user.Setup(u => u.Id).Returns((string?)null);

        var behaviour = new AuthorizationBehaviour<ListarUsuariosQuery, List<UsuarioAdminDto>>(_user.Object, _identityService.Object);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await behaviour.Handle(new ListarUsuariosQuery(), _ => Task.FromResult(new List<UsuarioAdminDto>()), CancellationToken.None));
    }
}
