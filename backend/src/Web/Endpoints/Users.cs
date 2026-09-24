using Sige.Application.Common.Interfaces;
using Sige.Infrastructure.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Sige.Web.Endpoints;

public class Users : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapIdentityApi<ApplicationUser>();

        groupBuilder.MapPost(Logout, "logout").RequireAuthorization();
        groupBuilder.MapGet(Me, "me").RequireAuthorization();
    }

    // Los access tokens de Identity son opacos (no un JWT auto-contenido), asi que
    // el frontend no puede leer el rol del token directamente; este endpoint expone
    // el rol del usuario autenticado (FR-012) para decidir que mostrar en la UI.
    [EndpointSummary("Usuario autenticado actual")]
    public static Ok<MeResponse> Me(IUser user)
    {
        return TypedResults.Ok(new MeResponse(user.Id, user.Roles ?? []));
    }

    [EndpointSummary("Log out")]
    [EndpointDescription("Logs out the current user by clearing the authentication cookie.")]
    public static async Task<Results<Ok, UnauthorizedHttpResult>> Logout(SignInManager<ApplicationUser> signInManager, [FromBody] object empty)
    {
        if (empty != null)
        {
            await signInManager.SignOutAsync();
            return TypedResults.Ok();
        }

        return TypedResults.Unauthorized();
    }
}

public record MeResponse(string? Id, List<string> Roles);
