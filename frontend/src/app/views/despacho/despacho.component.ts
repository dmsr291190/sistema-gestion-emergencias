import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import { EmergenciasService } from '../../core/services/emergencias.service';
import { UnidadesService } from '../../core/services/unidades.service';
import { FormFieldComponent } from '../../shared/form-field/form-field.component';
import { Emergencia, EstadoEmergencia } from '../../core/models/emergencia.model';
import { ESTADO_EMERGENCIA_LABEL } from '../../core/models/labels';
import { EstadoOperativoUnidad, Unidad } from '../../core/models/unidad.model';

// US3: asignar una o varias unidades a una emergencia (FR-006, FR-007, FR-009).
// FR-025: misma estructura de campo (app-form-field) que el resto de formularios.
@Component({
  selector: 'app-despacho',
  standalone: true,
  imports: [FormsModule, CardComponent, CardHeaderComponent, CardBodyComponent, FormFieldComponent],
  template: `
    <c-card>
      <c-card-header>Centro de despacho</c-card-header>
      <c-card-body>
        <p class="text-body-secondary">
          Solo se listan emergencias ya "Validadas" (no "Reportada" ni "Cerrada") y
          unidades "Disponibles" — la API revalida ambas condiciones al confirmar.
        </p>
        <div class="row">
          <div class="col-md-5">
            <app-form-field label="Emergencia">
              <select class="form-select" name="emergenciaId" [(ngModel)]="emergenciaId">
                <option [ngValue]="null">Selecciona una emergencia...</option>
                @for (e of emergenciasAsignables(); track e.id) {
                  <option [ngValue]="e.id">#{{ e.id }} — {{ e.tipoEmergencia?.nombre ?? '(sin tipo)' }} ({{ estadoLabel[e.estado] }})</option>
                }
              </select>
            </app-form-field>
          </div>
          <div class="col-md-5">
            <app-form-field label="Unidad disponible">
              <select class="form-select" name="unidadId" [(ngModel)]="unidadId">
                <option [ngValue]="null">Selecciona una unidad...</option>
                @for (u of unidadesDisponibles(); track u.id) {
                  <option [ngValue]="u.id">{{ u.identificador }}</option>
                }
              </select>
            </app-form-field>
          </div>
          <div class="col-md-2 d-flex align-items-start pt-4 mt-1">
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

  readonly estadoLabel = ESTADO_EMERGENCIA_LABEL;

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
