import { NgClass } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { CardBodyComponent, CardComponent, CardHeaderComponent, WidgetStatAComponent } from '@coreui/angular';
import { DashboardService } from '../../core/services/dashboard.service';
import { DashboardIndicadores } from '../../core/models/dashboard.model';
import { ESTADO_EMERGENCIA_COLOR_POR_NOMBRE, PRIORIDAD_COLOR_POR_NOMBRE } from '../../core/models/labels';

// FR-011: dashboard con indicadores operativos basicos.
// SC-006: se recarga al entrar a la vista; la actualizacion en vivo via SignalR
// (sin recarga manual) queda para cuando T017 (cliente SignalR) este completo.
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [NgClass, CardComponent, CardHeaderComponent, CardBodyComponent, WidgetStatAComponent],
  template: `
    <div class="row g-4 mb-4">
      <div class="col-sm-4">
        <c-widget-stat-a [value]="(indicadores()?.unidadesDisponibles ?? 0).toString()" title="Unidades disponibles" [color]="'success'" />
      </div>
      <div class="col-sm-4">
        <c-widget-stat-a [value]="(indicadores()?.unidadesOcupadas ?? 0).toString()" title="Unidades ocupadas" [color]="'warning'" />
      </div>
      <div class="col-sm-4">
        <c-widget-stat-a [value]="(indicadores()?.unidadesFueraDeServicio ?? 0).toString()" title="Fuera de servicio" [color]="'danger'" />
      </div>
    </div>
    <div class="row g-4 mb-4">
      <div class="col-sm-4">
        <c-widget-stat-a [value]="(indicadores()?.personalDesplegado ?? 0).toString()" title="Personal desplegado" [color]="'info'" />
      </div>
      <div class="col-sm-4">
        <c-widget-stat-a [value]="(indicadores()?.recursosMovilizados ?? 0).toString()" title="Recursos movilizados" [color]="'info'" />
      </div>
      <div class="col-sm-4">
        <c-widget-stat-a [value]="tiempoPromedioLabel()" title="Tiempo promedio de atención" [color]="'primary'" />
      </div>
    </div>
    <div class="row g-4 mb-4">
      <div class="col-md-12">
        <c-card>
          <c-card-header>Emergencias activas por ámbito</c-card-header>
          <c-card-body>
            @if (ambitosOrdenados().length === 0) {
              <p class="text-body-secondary">Sin emergencias activas.</p>
            }
            <div class="d-flex gap-4 flex-wrap">
              @for (item of ambitosOrdenados(); track item.clave) {
                <div class="text-center">
                  <div class="fs-4 fw-bold">{{ item.valor }}</div>
                  <div class="text-body-secondary small">{{ item.clave }}</div>
                </div>
              }
            </div>
          </c-card-body>
        </c-card>
      </div>
    </div>
    <div class="row g-4">
      <div class="col-md-6">
        <c-card>
          <c-card-header>Emergencias activas por estado</c-card-header>
          <c-card-body>
            @if (estadosOrdenados().length === 0) {
              <p class="text-body-secondary">Sin emergencias activas.</p>
            }
            <ul class="list-group">
              @for (item of estadosOrdenados(); track item.clave) {
                <li class="list-group-item d-flex justify-content-between">
                  <span [ngClass]="'text-' + estadoColor[item.clave]">{{ item.clave }}</span><strong>{{ item.valor }}</strong>
                </li>
              }
            </ul>
          </c-card-body>
        </c-card>
      </div>
      <div class="col-md-6">
        <c-card>
          <c-card-header>Emergencias activas por prioridad</c-card-header>
          <c-card-body>
            @if (prioridadesOrdenadas().length === 0) {
              <p class="text-body-secondary">Sin emergencias activas.</p>
            }
            <ul class="list-group">
              @for (item of prioridadesOrdenadas(); track item.clave) {
                <li class="list-group-item d-flex justify-content-between">
                  <span [ngClass]="'text-' + prioridadColor[item.clave]">{{ item.clave }}</span><strong>{{ item.valor }}</strong>
                </li>
              }
            </ul>
          </c-card-body>
        </c-card>
      </div>
    </div>
  `
})
export class DashboardComponent implements OnInit {
  readonly indicadores = signal<DashboardIndicadores | null>(null);
  readonly estadoColor = ESTADO_EMERGENCIA_COLOR_POR_NOMBRE;
  readonly prioridadColor = PRIORIDAD_COLOR_POR_NOMBRE;

  constructor(private readonly dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.dashboardService.obtenerIndicadores().subscribe((data) => this.indicadores.set(data));
  }

  private aLista(registro: Record<string, number> | undefined): { clave: string; valor: number }[] {
    return Object.entries(registro ?? {}).map(([clave, valor]) => ({ clave, valor }));
  }

  estadosOrdenados(): { clave: string; valor: number }[] {
    return this.aLista(this.indicadores()?.emergenciasPorEstado);
  }

  prioridadesOrdenadas(): { clave: string; valor: number }[] {
    return this.aLista(this.indicadores()?.emergenciasPorPrioridad);
  }

  ambitosOrdenados(): { clave: string; valor: number }[] {
    return this.aLista(this.indicadores()?.emergenciasPorAmbito);
  }

  tiempoPromedioLabel(): string {
    const minutos = this.indicadores()?.tiempoPromedioAtencionMinutos;
    if (minutos == null) return 'Sin datos';
    return minutos < 60 ? `${minutos.toFixed(0)} min` : `${(minutos / 60).toFixed(1)} h`;
  }
}
