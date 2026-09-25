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
        groupBuilder.MapGet(ObtenerMiUnidad, "mia");
        groupBuilder.MapGet(ObtenerUnidad, "{id}");
        groupBuilder.MapPatch(CambiarEstado, "{id}/estado");
    }

    // US5: detalle de una unidad (para la vista con personal y recursos).
    public static async Task<Results<Ok<UnidadDto>, NotFound>> ObtenerUnidad(ISender sender, int id)
    {
        var unidad = await sender.Send(new ObtenerUnidadPorIdQuery { UnidadId = id });
        return unidad != null ? TypedResults.Ok(unidad) : TypedResults.NotFound();
    }

    // FR-120: endpoint dedicado para el rol UnidadDeRespuesta -- ver
    // ObtenerMiUnidadQuery.
    public static async Task<Results<Ok<UnidadDto>, NotFound>> ObtenerMiUnidad(ISender sender)
    {
        var unidad = await sender.Send(new ObtenerMiUnidadQuery());
        return unidad != null ? TypedResults.Ok(unidad) : TypedResults.NotFound();
    }

    public static async Task<Created<int>> CrearUnidad(ISender sender, CrearUnidadCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Unidades/{id}", id);
    }

    // FR-108: filtros opcionales del mapa avanzado (US4).
    public static async Task<Ok<List<UnidadDto>>> ListarUnidades(
        ISender sender, TipoUnidad? tipo, EstadoOperativoUnidad? estadoOperativo, int? institucionId)
    {
        var unidades = await sender.Send(new ListarUnidadesQuery
        {
            Tipo = tipo,
            EstadoOperativo = estadoOperativo,
            InstitucionId = institucionId
        });
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
