using Sige.Application.Unidades.Commands.CambiarEstadoOperativoUnidad;
using Sige.Application.Unidades.Commands.CrearUnidad;
using Sige.Application.Unidades.Queries;
using Sige.Domain.Enums;
using Sige.Web.Hubs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;

namespace Sige.Web.Endpoints;

// contracts/rest-api.md (seccion Unidades). Prefijo real: /api/Unidades.
public class Unidades : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CrearUnidad);
        groupBuilder.MapGet(ListarUnidades);
        groupBuilder.MapPatch(CambiarEstado, "{id}/estado");
    }

    public static async Task<Created<int>> CrearUnidad(ISender sender, CrearUnidadCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Unidades/{id}", id);
    }

    public static async Task<Ok<List<UnidadDto>>> ListarUnidades(ISender sender)
    {
        var unidades = await sender.Send(new ListarUnidadesQuery());
        return TypedResults.Ok(unidades);
    }

    public static async Task<NoContent> CambiarEstado(
        ISender sender, IHubContext<OperacionesHub> hub, int id, CambiarEstadoOperativoUnidadRequest body)
    {
        await sender.Send(new CambiarEstadoOperativoUnidadCommand { UnidadId = id, NuevoEstado = body.NuevoEstado });
        await hub.Clients.All.SendAsync("UnidadActualizada", new { unidadId = id, estadoOperativo = body.NuevoEstado.ToString() });
        return TypedResults.NoContent();
    }
}

public record CambiarEstadoOperativoUnidadRequest(EstadoOperativoUnidad NuevoEstado);
