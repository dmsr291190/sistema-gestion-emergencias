using Microsoft.AspNetCore.SignalR;

namespace Sige.Web.Hubs;

// contracts/rest-api.md: eventos EmergenciaActualizada y UnidadActualizada (FR-014,
// SC-002, SC-006). El hub no expone metodos propios; los eventos se emiten desde los
// endpoints tras cada operacion exitosa.
public class OperacionesHub : Hub
{
}
