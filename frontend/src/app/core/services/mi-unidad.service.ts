import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-config';
import { EstadoOperativoUnidad, Unidad } from '../models/unidad.model';

// FR-120: endpoint dedicado para el rol UnidadDeRespuesta (solo su propia unidad).
@Injectable({ providedIn: 'root' })
export class MiUnidadService {
  private readonly baseUrl = `${API_BASE_URL}/api/Unidades`;

  constructor(private readonly http: HttpClient) {}

  obtener(): Observable<Unidad> {
    return this.http.get<Unidad>(`${this.baseUrl}/mia`);
  }

  cambiarEstado(id: number, nuevoEstado: EstadoOperativoUnidad): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/estado`, { nuevoEstado });
  }
}
