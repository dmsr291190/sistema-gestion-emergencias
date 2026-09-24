import { HttpErrorResponse } from '@angular/common/http';

// El backend (FluentValidation + ValidationBehaviour) responde 400 con un
// ValidationProblemDetails: { errors: { NombreDeCampo: ["mensaje1", ...] } }.
// Esta funcion lo convierte en un mapa simple campo(minusculas) -> primer mensaje,
// para mostrarlo junto al campo correspondiente (FR-021).
export function extraerErroresPorCampo(err: HttpErrorResponse): Record<string, string> {
  const errores = err.error?.errors as Record<string, string[]> | undefined;
  if (!errores) return {};

  const resultado: Record<string, string> = {};
  for (const [campo, mensajes] of Object.entries(errores)) {
    resultado[campo.toLowerCase()] = mensajes[0];
  }
  return resultado;
}
