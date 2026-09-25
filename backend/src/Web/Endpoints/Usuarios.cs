using Sige.Application.Common.Interfaces;
using Sige.Application.Usuarios.Commands.BloquearUsuario;
using Sige.Application.Usuarios.Commands.CambiarMiPassword;
using Sige.Application.Usuarios.Commands.CrearUsuario;
using Sige.Application.Usuarios.Commands.DesbloquearUsuario;
using Sige.Application.Usuarios.Commands.EditarUsuario;
using Sige.Application.Usuarios.Commands.ForzarCambioPassword;
using Sige.Application.Usuarios.Queries;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Sige.Web.Endpoints;

// contracts/rest-api.md (seccion Administracion de usuarios). Prefijo real:
// /api/Usuarios. US1 de la Ampliacion Operativa Nacional.
public class Usuarios : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CrearUsuario);
        groupBuilder.MapGet(ListarUsuarios);
        groupBuilder.MapPatch(EditarUsuario, "{id}");
        groupBuilder.MapPost(Bloquear, "{id}/bloquear");
        groupBuilder.MapPost(Desbloquear, "{id}/desbloquear");
        groupBuilder.MapPost(ForzarCambioPassword, "{id}/forzar-cambio-password");
        groupBuilder.MapPost(CambiarMiPassword, "me/cambiar-password");
    }

    public static async Task<Created<string>> CrearUsuario(ISender sender, CrearUsuarioCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Usuarios/{id}", id);
    }

    public static async Task<Ok<List<UsuarioAdminDto>>> ListarUsuarios(ISender sender)
    {
        var usuarios = await sender.Send(new ListarUsuariosQuery());
        return TypedResults.Ok(usuarios);
    }

    public static async Task<NoContent> EditarUsuario(ISender sender, string id, EditarUsuarioRequest body)
    {
        await sender.Send(new EditarUsuarioCommand { UserId = id, NombreCompleto = body.NombreCompleto, InstitucionId = body.InstitucionId, Roles = body.Roles });
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> Bloquear(ISender sender, string id)
    {
        await sender.Send(new BloquearUsuarioCommand { UserId = id });
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> Desbloquear(ISender sender, string id)
    {
        await sender.Send(new DesbloquearUsuarioCommand { UserId = id });
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> ForzarCambioPassword(ISender sender, string id, ForzarCambioPasswordRequest body)
    {
        await sender.Send(new ForzarCambioPasswordCommand { UserId = id, PasswordTemporal = body.PasswordTemporal });
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> CambiarMiPassword(ISender sender, CambiarMiPasswordCommand command)
    {
        await sender.Send(command);
        return TypedResults.NoContent();
    }
}

public record EditarUsuarioRequest(string NombreCompleto, int? InstitucionId, List<string> Roles);

public record ForzarCambioPasswordRequest(string PasswordTemporal);
