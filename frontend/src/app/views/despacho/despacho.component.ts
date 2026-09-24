import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import { EmergenciasService } from '../../core/services/emergencias.service';
import { UnidadesService } from '../../core/services/unidades.service';
import { Emergencia, EstadoEmergencia } from '../../core/models/emergencia.model';
import { EstadoOperativoUnidad, Unidad } from '../../core/models/unidad.model';

// US3: asignar una o varias unidades a una emergencia (FR-006, FR-007, FR-009).
@Component({
  selector: 'app-despacho',
  standalone: true,
  imports: [FormsModule, CardComponent, CardHeaderComponent, CardBodyComponent],
  template: `
    <c-card>
      <c-card-header>Centro de despacho</c-card-header>
      <c-card-body>
        <p class="text-body-secondary">
          Solo se listan emergencias ya "Validadas" (no "Reportada" ni "Cerrada") y
          unidades "Disponibles" — la API revalida ambas condiciones al confirmar.
        </p>
        <div class="row mb-3">
          <div class="col-md-5">
            <label class="form-label">Emergencia</label>
            <select class="form-select" name="emergenciaId" [(ngModel)]="emergenciaId">
              <option [ngValue]="null">Selecciona una emergencia...</option>
              @for (e of emergenciasAsignables(); track e.id) {
                <option [ngValue]="e.id">#{{ e.id }} — {{ e.tipo }} ({{ estadoLabel[e.estado] }})</option>
              }
            </select>
          </div>
          <div class="col-md-5">
            <label class="form-label">Unidad disponible</label>
            <select class="form-select" name="unidadId" [(ngModel)]="unidadId">
              <option [ngValue]="null">Selecciona una unidad...</option>
              @for (u of unidadesDisponibles(); track u.id) {
                <option [ngValue]="u.id">{{ u.identificador }}</option>
              }
            </select>
          </div>
          <div class="col-md-2 d-flex align-items-end">
            <button class="btn btn-primary w-100" (click)="asignar()" [disabled]="!emergenciaId || !unidadId || asignando()">
              {{ asignando() ? 'Asignando...' : 'Asignar' }}
            </button>
          </div>
        </div>
        @if (error()) {
          <div class="alert alert-danger py-2">{{ error() }}</div>
        }
        @if (mensajeExito()) {
          <div class="alert alert-success py-2">{{ mensajeExito() }}</div>
        }
      </c-card-body>
    </c-card>
  `
})
export class DespachoComponent implements OnInit {
  readonly emergenciasAsignables = signal<Emergencia[]>([]);
  readonly unidadesDisponibles = signal<Unidad[]>([]);
  readonly asignando = signal(false);
  readonly error = signal<string | null>(null);
  readonly mensajeExito = signal<string | null>(null);

  readonly estadoLabel = {
    0: 'Reportada', 1: 'Validada', 2: 'Despachada', 3: 'En ruta', 4: 'En el lugar', 5: 'Atendida', 6: 'Cerrada'
  };

  emergenciaId: number | null = null;
  unidadId: number | null = null;

  constructor(
    private readonly emergenciasService: EmergenciasService,
    private readonly unidadesService: UnidadesService
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  private cargar(): void {
    this.emergenciasService.listar().subscribe((data) =>
      this.emergenciasAsignables.set(
        data.filter((e) => e.estado !== EstadoEmergencia.Reportada && e.estado !== EstadoEmergencia.Cerrada)
      )
    );
    this.unidadesService.listar().subscribe((data) =>
      this.unidadesDisponibles.set(data.filter((u) => u.estadoOperativo === EstadoOperativoUnidad.Disponible))
    );
  }

  asignar(): void {
    if (!this.emergenciaId || !this.unidadId) return;

    this.error.set(null);
    this.mensajeExito.set(null);
    this.asignando.set(true);

    this.emergenciasService.asignarUnidad(this.emergenciaId, this.unidadId).subscribe({
      next: () => {
        this.asignando.set(false);
        this.mensajeExito.set('Unidad asignada correctamente.');
        this.emergenciaId = null;
        this.unidadId = null;
        this.cargar();
      },
      error: (err: HttpErrorResponse) => {
        this.asignando.set(false);
        // El backend responde { codigo, mensaje } en los conflictos de negocio
        // (ej. UNIDAD_NO_DISPONIBLE, EMERGENCIA_NO_VALIDADA) — FR-007, FR-009.
        this.error.set(err.error?.mensaje ?? 'No se pudo asignar la unidad.');
        this.cargar();
      }
    });
  }
}
