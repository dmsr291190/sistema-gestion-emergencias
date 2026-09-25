import { NgClass, NgFor } from '@angular/common';
import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CardBodyComponent, CardComponent, CardHeaderComponent, CollapseDirective } from '@coreui/angular';
import { ESTADO_EMERGENCIA_COLOR, ESTADO_EMERGENCIA_LABEL, TIPO_UNIDAD_LABEL } from '../../../core/models/labels';
import { TipoUnidad } from '../../../core/models/unidad.model';

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
  imports: [NgFor, NgClass, CardComponent, CardHeaderComponent, CardBodyComponent, CollapseDirective],
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
            <!-- FR-128 (revisión de contraste, US9): text-bg-dark (no solo bg-dark)
                 empareja fondo y texto de forma segura en ambos temas. -->
            <span class="badge text-bg-dark">Terrestre</span>
            <span class="badge" style="background:#3399ff;">Marítimo</span>
            <span class="badge bg-primary">Aéreo</span>
            <span class="badge bg-secondary">Mixto</span>
          </div>

          <!-- FR-107 (hallazgo HIGH de Converge): faltaba la referencia de
               Estado y de Tipo de unidad exigidas explícitamente por FR-107. -->
          <h6 class="mt-3">Estado de la emergencia</h6>
          <!-- Texto de color (no badge): algunos tokens de estadoColor (ej.
               "body-emphasis" para Cerrada, US9) no tienen una utilidad
               compuesta text-bg-* válida en Bootstrap. -->
          <div class="d-flex gap-2 flex-wrap small">
            <span *ngFor="let estado of estadosDisponibles" [ngClass]="'text-' + estadoColor[estado]">
              ● {{ estadoLabel[estado] }}
            </span>
          </div>

          <h6 class="mt-3">Tipo de unidad</h6>
          <div class="d-flex gap-2 flex-wrap small">
            <span *ngFor="let tipo of tiposUnidadDisponibles">{{ tipoUnidadLabel[tipo] }}</span>
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

  // FR-107: referencia de Estado y Tipo de unidad (hallazgo HIGH de Converge).
  readonly estadoLabel = ESTADO_EMERGENCIA_LABEL;
  readonly estadoColor = ESTADO_EMERGENCIA_COLOR;
  readonly estadosDisponibles = Object.keys(ESTADO_EMERGENCIA_LABEL).map(Number);

  readonly tipoUnidadLabel = TIPO_UNIDAD_LABEL;
  readonly tiposUnidadDisponibles = Object.values(TipoUnidad).filter((v) => typeof v === 'number') as TipoUnidad[];

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
