import { Routes } from '@angular/router';
import { DefaultLayoutComponent } from './layout/default-layout/default-layout.component';
import { LoginComponent } from './views/auth/login/login.component';
import { DashboardComponent } from './views/dashboard/dashboard.component';
import { MapaComponent } from './views/mapa/mapa.component';
import { EmergenciasComponent } from './views/emergencias/emergencias.component';
import { UnidadesComponent } from './views/unidades/unidades.component';
import { DespachoComponent } from './views/despacho/despacho.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: DefaultLayoutComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'mapa', component: MapaComponent },
      { path: 'emergencias', component: EmergenciasComponent },
      { path: 'unidades', component: UnidadesComponent },
      { path: 'despacho', component: DespachoComponent }
    ]
  }
];
