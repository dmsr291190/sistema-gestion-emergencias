import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-config';
import { CrearUsuarioRequest, EditarUsuarioRequest, UsuarioAdmin } from '../models/usuario.model';

// FR-118: administración de usuarios y roles (US1, ampliación 002).
@Injectable({ providedIn: 'root' })
export class UsuariosService {
  private readonly baseUrl = `${API_BASE_URL}/api/Usuarios`;

  constructor(private readonly http: HttpClient) {}

  listar(): Observable<UsuarioAdmin[]> {
    return this.http.get<UsuarioAdmin[]>(this.baseUrl);
  }

  crear(request: CrearUsuarioRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  editar(id: string, request: EditarUsuarioRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}`, request);
  }

  bloquear(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/bloquear`, {});
  }

  desbloquear(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/desbloquear`, {});
  }

  forzarCambioPassword(id: string, passwordTemporal: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/forzar-cambio-password`, { passwordTemporal });
  }

  cambiarMiPassword(passwordActual: string, passwordNueva: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/me/cambiar-password`, { passwordActual, passwordNueva });
  }
}
