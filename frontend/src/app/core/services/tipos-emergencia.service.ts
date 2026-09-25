import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-config';
import { CrearTipoEmergenciaRequest, EditarTipoEmergenciaRequest, TipoEmergencia } from '../models/tipo-emergencia.model';

// FR-101, FR-102: catálogo administrable de tipos de emergencia (US3).
@Injectable({ providedIn: 'root' })
export class TiposEmergenciaService {
  private readonly baseUrl = `${API_BASE_URL}/api/TiposEmergencia`;

  constructor(private readonly http: HttpClient) {}

  listar(soloActivos = false): Observable<TipoEmergencia[]> {
    return this.http.get<TipoEmergencia[]>(this.baseUrl, { params: { soloActivos } });
  }

  crear(request: CrearTipoEmergenciaRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl, request);
  }

  editar(id: number, request: EditarTipoEmergenciaRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}`, request);
  }
}
