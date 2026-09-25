using Sige.Application.PersonalUnidades.Commands.CrearPersonal;
using Sige.Application.PersonalUnidades.Commands.EditarPersonal;
using Sige.Application.PersonalUnidades.Queries;
using Sige.Web.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Sige.Web.Endpoints;

// contracts/rest-api.md (seccion Personal). El personal siempre pertenece a una
// unidad, por eso la ruta se anida bajo /api/Unidades (patron documentado en
// IEndpointGroup.RoutePrefix, ejemplo /api/TodoLists/{todoListId}/TodoItems).
public class Personal : IEndpointGroup
{
    public static string? RoutePrefix => "/api/Unidades/{unidadId}/personal";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CrearPersonal);
        groupBuilder.MapGet(ListarPersonal);
        groupBuilder.MapPatch(EditarPersonal, "{id}");
    }

    public static async Task<Created<int>> CrearPersonal(ISender sender, int unidadId, CrearPersonalRequest body)
    {
        var id = await sender.Send(new CrearPersonalCommand
        {
            UnidadRespuestaId = unidadId,
            Nombres = body.Nombres,
            Apellidos = body.Apellidos,
            Documento = body.Documento,
            InstitucionId = body.InstitucionId,
            Especialidad = body.Especialidad,
            Funcion = body.Funcion,
            Certificaciones = body.Certificaciones,
            Disponible = body.Disponible
        });
        return TypedResults.Created($"/api/Unidades/{unidadId}/personal/{id}", id);
    }

    public static async Task<Ok<List<PersonalDto>>> ListarPersonal(ISender sender, int unidadId)
    {
        var personal = await sender.Send(new ListarPersonalPorUnidadQuery { UnidadRespuestaId = unidadId });
        return TypedResults.Ok(personal);
    }

    public static async Task<NoContent> EditarPersonal(ISender sender, int unidadId, int id, EditarPersonalRequest body)
    {
        await sender.Send(new EditarPersonalCommand
        {
            Id = id,
            Nombres = body.Nombres,
            Apellidos = body.Apellidos,
            Documento = body.Documento,
            InstitucionId = body.InstitucionId,
            Especialidad = body.Especialidad,
            Funcion = body.Funcion,
            Certificaciones = body.Certificaciones,
            Disponible = body.Disponible
        });
        return TypedResults.NoContent();
    }
}

public record CrearPersonalRequest(
    string Nombres, string Apellidos, string Documento, int? InstitucionId,
    string? Especialidad, string? Funcion, string? Certificaciones, bool Disponible);

public record EditarPersonalRequest(
    string Nombres, string Apellidos, string Documento, int? InstitucionId,
    string? Especialidad, string? Funcion, string? Certificaciones, bool Disponible);
