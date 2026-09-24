import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, switchMap, tap } from 'rxjs';
import { API_BASE_URL } from '../api-config';

interface LoginResponse {
  tokenType: string;
  accessToken: string;
  expiresIn: number;
  refreshToken: string;
}

interface MeResponse {
  id: string | null;
  roles: string[];
}

const TOKEN_STORAGE_KEY = 'sige_access_token';
const ROLES_STORAGE_KEY = 'sige_roles';

// FR-012: acceso autenticado por rol (Operador/Supervisor). El access token de
// Identity es opaco (no un JWT auto-contenido), por eso el rol se consulta a
// GET /api/Users/me despues del login, en vez de decodificar el token en el cliente.
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_STORAGE_KEY));
  private readonly _roles = signal<string[]>(JSON.parse(localStorage.getItem(ROLES_STORAGE_KEY) ?? '[]'));

  readonly isAuthenticated = computed(() => !!this._token());
  readonly roles = computed(() => this._roles());
  readonly isSupervisor = computed(() => this._roles().includes('Supervisor'));

  constructor(private readonly http: HttpClient) {}

  get token(): string | null {
    return this._token();
  }

  login(email: string, password: string): Observable<MeResponse> {
    return this.http
      .post<LoginResponse>(`${API_BASE_URL}/api/Users/login`, { email, password })
      .pipe(
        tap((response) => {
          this._token.set(response.accessToken);
          localStorage.setItem(TOKEN_STORAGE_KEY, response.accessToken);
        }),
        switchMap(() => this.http.get<MeResponse>(`${API_BASE_URL}/api/Users/me`)),
        tap((me) => {
          this._roles.set(me.roles);
          localStorage.setItem(ROLES_STORAGE_KEY, JSON.stringify(me.roles));
        })
      );
  }

  logout(): void {
    this._token.set(null);
    this._roles.set([]);
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    localStorage.removeItem(ROLES_STORAGE_KEY);
  }
}
