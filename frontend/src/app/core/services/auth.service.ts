import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { API_BASE_URL } from '../api-config';

interface LoginResponse {
  tokenType: string;
  accessToken: string;
  expiresIn: number;
  refreshToken: string;
}

const TOKEN_STORAGE_KEY = 'sige_access_token';

// FR-012: acceso autenticado; el rol real (Operador/Supervisor) se resuelve leyendo
// el claim de rol del token en el backend, aqui solo se guarda el token.
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_STORAGE_KEY));

  readonly isAuthenticated = computed(() => !!this._token());

  constructor(private readonly http: HttpClient) {}

  get token(): string | null {
    return this._token();
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${API_BASE_URL}/api/Users/login`, { email, password })
      .pipe(
        tap((response) => {
          this._token.set(response.accessToken);
          localStorage.setItem(TOKEN_STORAGE_KEY, response.accessToken);
        })
      );
  }

  logout(): void {
    this._token.set(null);
    localStorage.removeItem(TOKEN_STORAGE_KEY);
  }
}
