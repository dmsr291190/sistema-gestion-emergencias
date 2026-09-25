using Sige.Application.Recursos.Commands.ActualizarCantidadRecurso;
using Sige.Application.Recursos.Commands.CrearRecurso;
using Sige.Application.Recursos.Queries;
using Sige.Domain.Enums;
using Sige.Web.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Sige.Web.Endpoints;

// contracts/rest-api.md (seccion Recursos). Anidado bajo /api/Unidades, igual
// que Personal.cs.
public class Recursos : IEndpointGroup
{
    public static string? RoutePrefix => "/api/Unidades/{unidadId}/recursos";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CrearRecurso);
        groupBuilder.MapGet(ListarRecursos);
        groupBuilder.MapPatch(ActualizarCantidadRecurso, "{id}");
    }

    public static async Task<Created<int>> CrearRecurso(ISender sender, int unidadId, CrearRecursoRequest body)
    {
        var id = await sender.Send(new CrearRecursoCommand
        {
            UnidadRespuestaId = unidadId,
            Codigo = body.Codigo,
            Nombre = body.Nombre,
            Categoria = body.Categoria,
            UnidadMedida = body.UnidadMedida,
            Cantidad = body.Cantidad,
            CantidadDisponible = body.CantidadDisponible,
            CantidadMinima = body.CantidadMinima
        });
        return TypedResults.Created($"/api/Unidades/{unidadId}/recursos/{id}", id);
    }

    public static async Task<Ok<List<RecursoDto>>> ListarRecursos(ISender sender, int unidadId)
    {
        var recursos = await sender.Send(new ListarRecursosPorUnidadQuery { UnidadRespuestaId = unidadId });
        return TypedResults.Ok(recursos);
    }

    public static async Task<NoContent> ActualizarCantidadRecurso(ISender sender, int unidadId, int id, ActualizarCantidadRecursoRequest body)
    {
        await sender.Send(new ActualizarCantidadRecursoCommand { Id = id, CantidadDisponible = body.CantidadDisponible });
        return TypedResults.NoContent();
    }
}

public record CrearRecursoRequest(
    string Codigo, string Nombre, CategoriaRecurso Categoria, string? UnidadMedida,
    int Cantidad, int CantidadDisponible, int CantidadMinima);

public record ActualizarCantidadRecursoRequest(int CantidadDisponible);
