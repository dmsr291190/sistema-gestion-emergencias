using Sige.Application.TiposEmergencia.Commands.CrearTipoEmergencia;
using Sige.Application.TiposEmergencia.Commands.EditarTipoEmergencia;
using Sige.Application.TiposEmergencia.Queries;
using Sige.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Sige.Web.Endpoints;

// contracts/rest-api.md (seccion Catalogo de tipos de emergencia). Prefijo real:
// /api/TiposEmergencia. US3 de la Ampliacion Operativa Nacional.
public class TiposEmergencia : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CrearTipoEmergencia);
        groupBuilder.MapGet(ListarTiposEmergencia);
        groupBuilder.MapPatch(EditarTipoEmergencia, "{id}");
    }

    public static async Task<Created<int>> CrearTipoEmergencia(ISender sender, CrearTipoEmergenciaCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/TiposEmergencia/{id}", id);
    }

    public static async Task<Ok<List<TipoEmergenciaDto>>> ListarTiposEmergencia(ISender sender, bool? soloActivos)
    {
        var tipos = await sender.Send(new ListarTiposEmergenciaQuery { SoloActivos = soloActivos ?? false });
        return TypedResults.Ok(tipos);
    }

    public static async Task<NoContent> EditarTipoEmergencia(ISender sender, int id, EditarTipoEmergenciaRequest body)
    {
        await sender.Send(new EditarTipoEmergenciaCommand
        {
            Id = id,
            Nombre = body.Nombre,
            Ambito = body.Ambito,
            Icono = body.Icono,
            Color = body.Color,
            PrioridadPorDefecto = body.PrioridadPorDefecto,
            Activo = body.Activo
        });
        return TypedResults.NoContent();
    }
}

public record EditarTipoEmergenciaRequest(
    string Nombre, Ambito Ambito, string? Icono, string? Color, Prioridad PrioridadPorDefecto, bool Activo);
