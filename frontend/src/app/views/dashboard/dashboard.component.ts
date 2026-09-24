import { Component, OnInit, signal } from '@angular/core';
import { CardBodyComponent, CardComponent, CardHeaderComponent, WidgetStatAComponent } from '@coreui/angular';
import { DashboardService } from '../../core/services/dashboard.service';
import { DashboardIndicadores } from '../../core/models/dashboard.model';

// FR-011: dashboard con indicadores operativos basicos.
// SC-006: se recarga al entrar a la vista; la actualizacion en vivo via SignalR
// (sin recarga manual) queda para cuando T017 (cliente SignalR) este completo.
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CardComponent, CardHeaderComponent, CardBodyComponent, WidgetStatAComponent],
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
                  <span>{{ item.clave }}</span><strong>{{ item.valor }}</strong>
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
                  <span>{{ item.clave }}</span><strong>{{ item.valor }}</strong>
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
}
