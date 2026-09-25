import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-config';
import { Institucion } from '../models/institucion.model';

// Catálogo único compartido por Usuario/Personal/UnidadRespuesta (CHK038).
@Injectable({ providedIn: 'root' })
export class InstitucionesService {
  private readonly baseUrl = `${API_BASE_URL}/api/Instituciones`;

  constructor(private readonly http: HttpClient) {}

  listar(): Observable<Institucion[]> {
    return this.http.get<Institucion[]>(this.baseUrl);
  }
}
