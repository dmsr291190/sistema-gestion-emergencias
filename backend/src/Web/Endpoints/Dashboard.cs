using Sige.Application.Dashboard.Queries;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Sige.Web.Endpoints;

public class Dashboard : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(ObtenerIndicadores, "indicadores");
    }

    public static async Task<Ok<DashboardIndicadoresDto>> ObtenerIndicadores(ISender sender)
    {
        var indicadores = await sender.Send(new ObtenerIndicadoresDashboardQuery());
        return TypedResults.Ok(indicadores);
    }
}
