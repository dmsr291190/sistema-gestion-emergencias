import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-config';
import { CrearPersonalRequest, Personal } from '../models/personal.model';

// FR-110: personal asociado a una unidad de respuesta (US5).
@Injectable({ providedIn: 'root' })
export class PersonalService {
  constructor(private readonly http: HttpClient) {}

  private baseUrl(unidadId: number): string {
    return `${API_BASE_URL}/api/Unidades/${unidadId}/personal`;
  }

  listarPorUnidad(unidadId: number): Observable<Personal[]> {
    return this.http.get<Personal[]>(this.baseUrl(unidadId));
  }

  crear(unidadId: number, request: CrearPersonalRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl(unidadId), request);
  }

  editar(unidadId: number, id: number, request: CrearPersonalRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl(unidadId)}/${id}`, request);
  }
}
