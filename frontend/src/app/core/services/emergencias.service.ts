import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-config';
import { CrearEmergenciaRequest, Emergencia, EmergenciaDetalle } from '../models/emergencia.model';

@Injectable({ providedIn: 'root' })
export class EmergenciasService {
  private readonly baseUrl = `${API_BASE_URL}/api/Emergencias`;

  constructor(private readonly http: HttpClient) {}

  listar(): Observable<Emergencia[]> {
    return this.http.get<Emergencia[]>(this.baseUrl);
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
}
