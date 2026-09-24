import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-config';
import { CrearUnidadRequest, EstadoOperativoUnidad, Unidad } from '../models/unidad.model';

@Injectable({ providedIn: 'root' })
export class UnidadesService {
  private readonly baseUrl = `${API_BASE_URL}/api/Unidades`;

  constructor(private readonly http: HttpClient) {}

  listar(): Observable<Unidad[]> {
    return this.http.get<Unidad[]>(this.baseUrl);
  }

  crear(request: CrearUnidadRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl, request);
  }

  cambiarEstado(id: number, nuevoEstado: EstadoOperativoUnidad): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/estado`, { nuevoEstado });
  }
}
