import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-config';
import { CrearUnidadRequest, EstadoOperativoUnidad, TipoUnidad, Unidad } from '../models/unidad.model';

// FR-108: filtros opcionales del mapa avanzado (US4).
export interface FiltrosUnidades {
  tipo?: TipoUnidad | null;
  estadoOperativo?: EstadoOperativoUnidad | null;
  institucionId?: number | null;
}

@Injectable({ providedIn: 'root' })
export class UnidadesService {
  private readonly baseUrl = `${API_BASE_URL}/api/Unidades`;

  constructor(private readonly http: HttpClient) {}

  listar(filtros?: FiltrosUnidades): Observable<Unidad[]> {
    const params: Record<string, string> = {};
    if (filtros?.tipo != null) params['tipo'] = String(filtros.tipo);
    if (filtros?.estadoOperativo != null) params['estadoOperativo'] = String(filtros.estadoOperativo);
    if (filtros?.institucionId != null) params['institucionId'] = String(filtros.institucionId);

    return this.http.get<Unidad[]>(this.baseUrl, { params });
  }

  crear(request: CrearUnidadRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl, request);
  }

  cambiarEstado(id: number, nuevoEstado: EstadoOperativoUnidad): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/estado`, { nuevoEstado });
  }
}
