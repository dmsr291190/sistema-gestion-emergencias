import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-config';
import { CrearRecursoRequest, Recurso } from '../models/recurso.model';

// FR-111, FR-112: recursos transportados por una unidad de respuesta (US5).
@Injectable({ providedIn: 'root' })
export class RecursosService {
  constructor(private readonly http: HttpClient) {}

  private baseUrl(unidadId: number): string {
    return `${API_BASE_URL}/api/Unidades/${unidadId}/recursos`;
  }

  listarPorUnidad(unidadId: number): Observable<Recurso[]> {
    return this.http.get<Recurso[]>(this.baseUrl(unidadId));
  }

  crear(unidadId: number, request: CrearRecursoRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl(unidadId), request);
  }

  actualizarCantidad(unidadId: number, id: number, cantidadDisponible: number): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl(unidadId)}/${id}`, { cantidadDisponible });
  }
}
