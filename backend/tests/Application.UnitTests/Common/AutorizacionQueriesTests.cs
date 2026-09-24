using Sige.Application.Common.Behaviours;
using Sige.Application.Common.Interfaces;
using Sige.Application.Dashboard.Queries;
using Sige.Application.Emergencias.Queries;
using Sige.Application.Unidades.Queries;
using MediatR;
using Moq;
using NUnit.Framework;

namespace Sige.Application.UnitTests.Common;

// Constitution Principio V: todo acceso MUST requerir autenticacion.
// Hallazgo de Converge: las 4 queries de lectura respondian sin token porque no
// tenian [Authorize] — verificado con curl (200 sin Authorization header).
public class AutorizacionQueriesTests
{
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
        _user.Setup(u => u.Id).Returns((string?)null);
    }

    [Test]
    public void ListarEmergenciasQuery_DebeRechazar_SinUsuarioAutenticado()
    {
        var behaviour = new AuthorizationBehaviour<ListarEmergenciasQuery, List<EmergenciaDto>>(_user.Object, _identityService.Object);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await behaviour.Handle(new ListarEmergenciasQuery(), _ => Task.FromResult(new List<EmergenciaDto>()), CancellationToken.None));
    }

    [Test]
    public void ObtenerEmergenciaPorIdQuery_DebeRechazar_SinUsuarioAutenticado()
    {
        var behaviour = new AuthorizationBehaviour<ObtenerEmergenciaPorIdQuery, EmergenciaDetailDto?>(_user.Object, _identityService.Object);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await behaviour.Handle(new ObtenerEmergenciaPorIdQuery { EmergenciaId = 1 }, _ => Task.FromResult<EmergenciaDetailDto?>(null), CancellationToken.None));
    }

    [Test]
    public void ListarUnidadesQuery_DebeRechazar_SinUsuarioAutenticado()
    {
        var behaviour = new AuthorizationBehaviour<ListarUnidadesQuery, List<UnidadDto>>(_user.Object, _identityService.Object);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await behaviour.Handle(new ListarUnidadesQuery(), _ => Task.FromResult(new List<UnidadDto>()), CancellationToken.None));
    }

    [Test]
    public void ObtenerIndicadoresDashboardQuery_DebeRechazar_SinUsuarioAutenticado()
    {
        var behaviour = new AuthorizationBehaviour<ObtenerIndicadoresDashboardQuery, DashboardIndicadoresDto>(_user.Object, _identityService.Object);

        var dto = new DashboardIndicadoresDto
        {
            EmergenciasPorEstado = [],
            EmergenciasPorPrioridad = []
        };

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await behaviour.Handle(new ObtenerIndicadoresDashboardQuery(), _ => Task.FromResult(dto), CancellationToken.None));
    }
}
