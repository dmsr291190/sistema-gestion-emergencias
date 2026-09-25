import { NgFor } from '@angular/common';
import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CardBodyComponent, CardComponent, CardHeaderComponent, CollapseDirective } from '@coreui/angular';

export interface EstadoCapas {
  emergencias: boolean;
  unidades: boolean;
  bases: boolean;
  hospitales: boolean;
  puertos: boolean;
  rutas: boolean;
  historico: boolean;
}

// FR-107: leyenda plegable del mapa con las capas activables/desactivables
// (emergencias, unidades, bases, hospitales, puertos, rutas, histórico) y
// referencia de prioridad/estado/ámbito.
@Component({
  selector: 'app-leyenda',
  standalone: true,
  imports: [NgFor, CardComponent, CardHeaderComponent, CardBodyComponent, CollapseDirective],
  template: `
    <c-card>
      <c-card-header role="button" (click)="abierta.set(!abierta())">
        Leyenda y capas {{ abierta() ? '▲' : '▼' }}
      </c-card-header>
      <div cCollapse [visible]="abierta()">
        <c-card-body>
          <h6>Capas</h6>
          <div class="form-check" *ngFor="let capa of capasDisponibles">
            <input
              class="form-check-input"
              type="checkbox"
              [id]="'capa-' + capa.clave"
              [checked]="capas[capa.clave]"
              (change)="alternar(capa.clave)"
            />
            <label class="form-check-label" [for]="'capa-' + capa.clave">{{ capa.label }}</label>
          </div>

          <h6 class="mt-3">Prioridad</h6>
          <div class="d-flex gap-2 flex-wrap small">
            <span class="badge bg-secondary">Baja</span>
            <span class="badge bg-info">Media</span>
            <span class="badge bg-warning">Alta</span>
            <span class="badge bg-danger">Crítica</span>
          </div>

          <h6 class="mt-3">Ámbito</h6>
          <div class="d-flex gap-2 flex-wrap small">
            <span class="badge bg-dark">Terrestre</span>
            <span class="badge" style="background:#3399ff;">Marítimo</span>
            <span class="badge bg-primary">Aéreo</span>
            <span class="badge bg-secondary">Mixto</span>
          </div>
        </c-card-body>
      </div>
    </c-card>
  `
})
export class LeyendaComponent {
  @Input() capas!: EstadoCapas;
  @Output() capasChange = new EventEmitter<EstadoCapas>();

  readonly abierta = signal(true);

  readonly capasDisponibles: { clave: keyof EstadoCapas; label: string }[] = [
    { clave: 'emergencias', label: 'Emergencias' },
    { clave: 'unidades', label: 'Unidades' },
    { clave: 'bases', label: 'Bases' },
    { clave: 'hospitales', label: 'Hospitales' },
    { clave: 'puertos', label: 'Puertos' },
    { clave: 'rutas', label: 'Rutas' },
    { clave: 'historico', label: 'Histórico (cerradas)' }
  ];

  alternar(clave: keyof EstadoCapas): void {
    this.capasChange.emit({ ...this.capas, [clave]: !this.capas[clave] });
  }
}
