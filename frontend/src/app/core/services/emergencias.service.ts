import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-config';
import { Ambito, CrearEmergenciaRequest, Emergencia, EmergenciaDetalle, EstadoAsignacion, EstadoEmergencia, Prioridad } from '../models/emergencia.model';

// FR-108: filtros opcionales del mapa avanzado (US4).
export interface FiltrosEmergencias {
  tipoEmergenciaId?: number | null;
  prioridad?: Prioridad | null;
  estado?: EstadoEmergencia | null;
  ambito?: Ambito | null;
  departamento?: string | null;
  provincia?: string | null;
}

@Injectable({ providedIn: 'root' })
export class EmergenciasService {
  private readonly baseUrl = `${API_BASE_URL}/api/Emergencias`;

  constructor(private readonly http: HttpClient) {}

  listar(filtros?: FiltrosEmergencias): Observable<Emergencia[]> {
    const params: Record<string, string> = {};
    if (filtros?.tipoEmergenciaId != null) params['tipoEmergenciaId'] = String(filtros.tipoEmergenciaId);
    if (filtros?.prioridad != null) params['prioridad'] = String(filtros.prioridad);
    if (filtros?.estado != null) params['estado'] = String(filtros.estado);
    if (filtros?.ambito != null) params['ambito'] = String(filtros.ambito);
    if (filtros?.departamento) params['departamento'] = filtros.departamento;
    if (filtros?.provincia) params['provincia'] = filtros.provincia;

    return this.http.get<Emergencia[]>(this.baseUrl, { params });
  }

  obtener(id: number): Observable<EmergenciaDetalle> {
    return this.http.get<EmergenciaDetalle>(`${this.baseUrl}/${id}`);
  }

  crear(request: CrearEmergenciaRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl, request);
  }

  validar(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/validar`, {});
  }

  asignarUnidad(emergenciaId: number, unidadId: number): Observable<number> {
    return this.http.post<number>(`${this.baseUrl}/${emergenciaId}/asignaciones`, { unidadId });
  }

  cambiarEstadoAsignacion(asignacionId: number, nuevoEstado: EstadoAsignacion): Observable<void> {
    return this.http.patch<void>(`${API_BASE_URL}/api/Asignaciones/${asignacionId}/estado`, { nuevoEstado });
  }

  cerrar(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/cerrar`, {});
  }

  reabrir(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/reabrir`, {});
  }
}
