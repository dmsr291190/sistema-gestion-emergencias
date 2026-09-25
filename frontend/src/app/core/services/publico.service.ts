import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-config';
import { EmergenciaPublica, EmergenciaPublicaDetalle } from '../models/emergencia-publica.model';

// FR-113: sin autenticacion -- no requiere (ni envia) token.
@Injectable({ providedIn: 'root' })
export class PublicoService {
  private readonly baseUrl = `${API_BASE_URL}/api/Publico`;

  constructor(private readonly http: HttpClient) {}

  listarEmergencias(): Observable<EmergenciaPublica[]> {
    return this.http.get<EmergenciaPublica[]>(`${this.baseUrl}/Emergencias`);
  }

  obtenerEmergencia(codigo: number): Observable<EmergenciaPublicaDetalle> {
    return this.http.get<EmergenciaPublicaDetalle>(`${this.baseUrl}/Emergencias/${codigo}`);
  }
}
