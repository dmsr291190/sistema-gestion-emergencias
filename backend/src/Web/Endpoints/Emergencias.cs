using Sige.Application.Asignaciones.Commands.AsignarUnidad;
using Sige.Application.Emergencias.Commands.CerrarEmergencia;
using Sige.Application.Emergencias.Commands.CrearEmergencia;
using Sige.Application.Emergencias.Commands.ReabrirEmergencia;
using Sige.Application.Emergencias.Commands.ValidarEmergencia;
using Sige.Application.Emergencias.Queries;
using Sige.Web.Hubs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;

namespace Sige.Web.Endpoints;

// contracts/rest-api.md (seccion Emergencias). Prefijo real: /api/Emergencias
// (convencion de IEndpointGroup de la plantilla), no /emergencias como en el
// contrato original — desviacion documentada en la bitacora.
public class Emergencias : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CrearEmergencia);
        groupBuilder.MapGet(ListarEmergencias);
        groupBuilder.MapGet(ObtenerEmergencia, "{id}");
        groupBuilder.MapPost(ValidarEmergencia, "{id}/validar");
        groupBuilder.MapPost(AsignarUnidad, "{id}/asignaciones");
        groupBuilder.MapPost(CerrarEmergencia, "{id}/cerrar");
        groupBuilder.MapPost(ReabrirEmergencia, "{id}/reabrir");
    }

    public static async Task<Created<int>> CrearEmergencia(
        ISender sender, IHubContext<OperacionesHub> hub, CrearEmergenciaCommand command)
    {
        var id = await sender.Send(command);
        await hub.Clients.All.SendAsync("EmergenciaActualizada", new { emergenciaId = id, estado = "Reportada" });
        return TypedResults.Created($"/api/Emergencias/{id}", id);
    }

    public static async Task<Ok<List<EmergenciaDto>>> ListarEmergencias(ISender sender)
    {
        var emergencias = await sender.Send(new ListarEmergenciasQuery());
        return TypedResults.Ok(emergencias);
    }

    public static async Task<Results<Ok<EmergenciaDetailDto>, NotFound>> ObtenerEmergencia(ISender sender, int id)
    {
        var emergencia = await sender.Send(new ObtenerEmergenciaPorIdQuery { EmergenciaId = id });
        return emergencia is null ? TypedResults.NotFound() : TypedResults.Ok(emergencia);
    }

    public static async Task<NoContent> ValidarEmergencia(ISender sender, IHubContext<OperacionesHub> hub, int id)
    {
        await sender.Send(new ValidarEmergenciaCommand { EmergenciaId = id });
        await hub.Clients.All.SendAsync("EmergenciaActualizada", new { emergenciaId = id, estado = "Validada" });
        return TypedResults.NoContent();
    }

    public static async Task<Created<int>> AsignarUnidad(
        ISender sender, IHubContext<OperacionesHub> hub, int id, AsignarUnidadRequest body)
    {
        var emergenciaId = await sender.Send(new AsignarUnidadCommand { EmergenciaId = id, UnidadId = body.UnidadId });
        await hub.Clients.All.SendAsync("EmergenciaActualizada", new { emergenciaId, estado = "Despachada" });
        await hub.Clients.All.SendAsync("UnidadActualizada", new { unidadId = body.UnidadId, estadoOperativo = "Ocupada" });
        return TypedResults.Created($"/api/Emergencias/{id}", emergenciaId);
    }

    public static async Task<NoContent> CerrarEmergencia(ISender sender, IHubContext<OperacionesHub> hub, int id)
    {
        await sender.Send(new CerrarEmergenciaCommand { EmergenciaId = id });
        await hub.Clients.All.SendAsync("EmergenciaActualizada", new { emergenciaId = id, estado = "Cerrada" });
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> ReabrirEmergencia(ISender sender, IHubContext<OperacionesHub> hub, int id)
    {
        await sender.Send(new ReabrirEmergenciaCommand { EmergenciaId = id });
        await hub.Clients.All.SendAsync("EmergenciaActualizada", new { emergenciaId = id, estado = "Atendida" });
        return TypedResults.NoContent();
    }
}

public record AsignarUnidadRequest(int UnidadId);
