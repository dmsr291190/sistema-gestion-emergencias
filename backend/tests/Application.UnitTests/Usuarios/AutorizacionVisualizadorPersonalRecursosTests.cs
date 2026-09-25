using Sige.Application.Common.Behaviours;
using Sige.Application.Common.Exceptions;
using Sige.Application.Common.Interfaces;
using Sige.Application.PersonalUnidades.Queries;
using Sige.Application.Recursos.Queries;
using Sige.Domain.Constants;
using Moq;
using NUnit.Framework;

namespace Sige.Application.UnitTests.Usuarios;

// FR-120a (hallazgo CRITICAL de /speckit-converge): el Visualizador MUST NOT
// tener acceso a personal ni recursos. ListarPersonalPorUnidadQuery y
// ListarRecursosPorUnidadQuery solo tenian [Authorize] generico y respondian
// 200 con datos reales (incluido documento de identidad) a un Visualizador
// -- verificado con curl antes del fix.
public class AutorizacionVisualizadorPersonalRecursosTests
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
    public void DebeRechazar_ListarPersonalPorUnidad()
    {
        var behaviour = new AuthorizationBehaviour<ListarPersonalPorUnidadQuery, List<PersonalDto>>(_user.Object, _identityService.Object);

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await behaviour.Handle(new ListarPersonalPorUnidadQuery { UnidadRespuestaId = 1 }, _ => Task.FromResult(new List<PersonalDto>()), CancellationToken.None));
    }

    [Test]
    public void DebeRechazar_ListarRecursosPorUnidad()
    {
        var behaviour = new AuthorizationBehaviour<ListarRecursosPorUnidadQuery, List<RecursoDto>>(_user.Object, _identityService.Object);

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await behaviour.Handle(new ListarRecursosPorUnidadQuery { UnidadRespuestaId = 1 }, _ => Task.FromResult(new List<RecursoDto>()), CancellationToken.None));
    }
}
