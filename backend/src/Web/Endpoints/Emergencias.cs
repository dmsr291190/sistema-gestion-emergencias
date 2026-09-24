using Sige.Application.Emergencias.Commands.CrearEmergencia;
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
}
