using Sige.Application.Instituciones.Queries;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Sige.Web.Endpoints;

// contracts/rest-api.md (seccion Catalogo de instituciones). Agregado tras
// /speckit-analyze (hallazgo C2). Prefijo real: /api/Instituciones.
public class Instituciones : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(ListarInstituciones);
    }

    public static async Task<Ok<List<InstitucionDto>>> ListarInstituciones(ISender sender)
    {
        var instituciones = await sender.Send(new ListarInstitucionesQuery());
        return TypedResults.Ok(instituciones);
    }
}
