import { Routes } from '@angular/router';
import { DefaultLayoutComponent } from './layout/default-layout/default-layout.component';
import { authGuard } from './core/guards/auth.guard';
import { administradorGuard } from './core/guards/administrador.guard';
import { unidadDeRespuestaGuard } from './core/guards/unidad-de-respuesta.guard';
import { noVisualizadorGuard } from './core/guards/no-visualizador.guard';
import { LoginComponent } from './views/auth/login/login.component';
import { DashboardComponent } from './views/dashboard/dashboard.component';
import { MapaComponent } from './views/mapa/mapa.component';
import { EmergenciasComponent } from './views/emergencias/emergencias.component';
import { EmergenciaDetalleComponent } from './views/emergencias/detalle/emergencia-detalle.component';
import { UnidadesComponent } from './views/unidades/unidades.component';
import { DespachoComponent } from './views/despacho/despacho.component';
import { UsuariosComponent } from './views/usuarios/usuarios.component';
import { MiUnidadComponent } from './views/mi-unidad/mi-unidad.component';
import { TiposEmergenciaComponent } from './views/tipos-emergencia/tipos-emergencia.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: DefaultLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'mapa', component: MapaComponent },
      { path: 'emergencias', component: EmergenciasComponent, canActivate: [noVisualizadorGuard] },
      { path: 'emergencias/:id', component: EmergenciaDetalleComponent, canActivate: [noVisualizadorGuard] },
      { path: 'unidades', component: UnidadesComponent, canActivate: [noVisualizadorGuard] },
      { path: 'despacho', component: DespachoComponent, canActivate: [noVisualizadorGuard] },
      // US1, FR-116: administración de usuarios y roles ampliados (solo Administrador).
      { path: 'usuarios', component: UsuariosComponent, canActivate: [administradorGuard] },
      // US3, FR-101: catálogo administrable de tipos de emergencia (solo Administrador).
      { path: 'tipos-emergencia', component: TiposEmergenciaComponent, canActivate: [administradorGuard] },
      // FR-120: pantalla mínima para el rol "Unidad de respuesta".
      { path: 'mi-unidad', component: MiUnidadComponent, canActivate: [unidadDeRespuestaGuard] }
    ]
  }
];
