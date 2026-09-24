import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-config';
import { DashboardIndicadores } from '../models/dashboard.model';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  constructor(private readonly http: HttpClient) {}

  obtenerIndicadores(): Observable<DashboardIndicadores> {
    return this.http.get<DashboardIndicadores>(`${API_BASE_URL}/api/Dashboard/indicadores`);
  }
}
