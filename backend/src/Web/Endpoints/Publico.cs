using Sige.Application.Publico.Queries;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Sige.Web.Endpoints;

// contracts/rest-api.md (seccion Vista publica). US7 de la Ampliacion
// Operativa Nacional. Prefijo real: /api/Publico. NINGUN endpoint de este
// grupo llama a RequireAuthorization() ni tiene Commands/Queries con
// [Authorize] -- deliberadamente anonimo (FR-113).
public class Publico : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(ListarEmergenciasPublicas, "Emergencias");
        groupBuilder.MapGet(ObtenerEmergenciaPublica, "Emergencias/{codigo}");
    }

    // Nombres con sufijo "Publica(s)" para no colisionar con los operationId
    // ListarEmergencias/ObtenerEmergencia ya usados por Emergencias.cs -- los
    // nombres de endpoint deben ser globalmente unicos (mismo tipo de bug ya
    // encontrado una vez en el MVP con Unidades/Asignaciones.CambiarEstado).
    public static async Task<Ok<List<EmergenciaPublicaDto>>> ListarEmergenciasPublicas(ISender sender)
    {
        var emergencias = await sender.Send(new ListarEmergenciasPublicasQuery());
        return TypedResults.Ok(emergencias);
    }

    // Edge Case (spec.md): identificador inexistente o fuera de alcance publico
    // responde 404 generico, sin distinguir el motivo (no revelar por enumeracion).
    public static async Task<Results<Ok<EmergenciaPublicaDetalleDto>, NotFound>> ObtenerEmergenciaPublica(ISender sender, int codigo)
    {
        var emergencia = await sender.Send(new ObtenerEmergenciaPublicaQuery { Codigo = codigo });
        return emergencia != null ? TypedResults.Ok(emergencia) : TypedResults.NotFound();
    }
}
