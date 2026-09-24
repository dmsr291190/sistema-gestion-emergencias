using Sige.Application.Asignaciones.Commands.CambiarEstadoAsignacion;
using Sige.Domain.Enums;
using Sige.Web.Hubs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;

namespace Sige.Web.Endpoints;

public class Asignaciones : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPatch(CambiarEstadoDeAsignacion, "{id}/estado");
    }

    // Nombre distinto de Unidades.CambiarEstado: los nombres de endpoint deben ser
    // unicos globalmente (se usan como operationId de OpenAPI) — bug real encontrado
    // al probar US4, ver bitacora.
    public static async Task<NoContent> CambiarEstadoDeAsignacion(
        ISender sender, IHubContext<OperacionesHub> hub, int id, CambiarEstadoAsignacionRequest body)
    {
        await sender.Send(new CambiarEstadoAsignacionCommand { AsignacionId = id, NuevoEstado = body.NuevoEstado });
        await hub.Clients.All.SendAsync("EmergenciaActualizada", new { asignacionId = id, estado = body.NuevoEstado.ToString() });
        return TypedResults.NoContent();
    }
}

public record CambiarEstadoAsignacionRequest(EstadoAsignacion NuevoEstado);
