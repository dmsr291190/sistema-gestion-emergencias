import { NgClass } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import { EmergenciasService } from '../../core/services/emergencias.service';
import { extraerErroresPorCampo } from '../../core/utils/validation-errors';
import { FormFieldComponent } from '../../shared/form-field/form-field.component';
import { CrearEmergenciaRequest, Emergencia, Prioridad } from '../../core/models/emergencia.model';
import { ESTADO_EMERGENCIA_COLOR, ESTADO_EMERGENCIA_LABEL, PRIORIDAD_LABEL } from '../../core/models/labels';

// US1: registrar y visualizar una emergencia (FR-001, FR-002, FR-015, FR-019).
// FR-021, FR-022, FR-025: validacion por campo, boton con estado de carga, y
// estructura de campo compartida (app-form-field) — Historia de Usuario 6.
@Component({
  selector: 'app-emergencias',
  standalone: true,
  imports: [NgClass, FormsModule, RouterLink, CardComponent, CardHeaderComponent, CardBodyComponent, FormFieldComponent],
  template: `
    <div class="row g-4">
      <div class="col-md-5">
        <c-card>
          <c-card-header>Nueva emergencia</c-card-header>
          <c-card-body>
            <form (ngSubmit)="crear()">
              <app-form-field label="Tipo" [error]="errores()['tipo']">
                <input class="form-control" name="tipo" [(ngModel)]="form.tipo" required />
              </app-form-field>
              <app-form-field label="Descripcion" [error]="errores()['descripcion']">
                <textarea class="form-control" name="descripcion" [(ngModel)]="form.descripcion" required></textarea>
              </app-form-field>
              <div class="row">
                <div class="col-6">
                  <app-form-field label="Latitud" [error]="errores()['latitud']">
                    <input class="form-control" type="number" step="0.0001" name="latitud" [(ngModel)]="form.latitud" required />
                  </app-form-field>
                </div>
                <div class="col-6">
                  <app-form-field label="Longitud" [error]="errores()['longitud']">
                    <input class="form-control" type="number" step="0.0001" name="longitud" [(ngModel)]="form.longitud" required />
                  </app-form-field>
                </div>
              </div>
              <app-form-field label="Prioridad" [error]="errores()['prioridad']">
                <select class="form-select" name="prioridad" [(ngModel)]="form.prioridad">
                  <option [ngValue]="0">Baja</option>
                  <option [ngValue]="1">Media</option>
                  <option [ngValue]="2">Alta</option>
                  <option [ngValue]="3">Critica</option>
                </select>
              </app-form-field>
              <app-form-field label="Nombre del reportante" [error]="errores()['reportantenombre']">
                <input class="form-control" name="reportanteNombre" [(ngModel)]="form.reportanteNombre" required />
              </app-form-field>
              <app-form-field label="Contacto del reportante (opcional)" [error]="errores()['reportantecontacto']">
                <input class="form-control" name="reportanteContacto" [(ngModel)]="form.reportanteContacto" />
              </app-form-field>
              @if (errorGeneral()) {
                <div class="alert alert-danger py-2">{{ errorGeneral() }}</div>
              }
              <button class="btn btn-primary" type="submit" [disabled]="guardando()">
                {{ guardando() ? 'Guardando...' : 'Registrar emergencia' }}
              </button>
            </form>
          </c-card-body>
        </c-card>
      </div>
      <div class="col-md-7">
        <c-card>
          <c-card-header>Emergencias registradas</c-card-header>
          <c-card-body>
            <table class="table table-sm">
              <thead>
                <tr><th>Tipo</th><th>Prioridad</th><th>Estado</th><th></th></tr>
              </thead>
              <tbody>
                @for (e of emergencias(); track e.id) {
                  <tr>
                    <td>{{ e.tipo }}</td>
                    <td>{{ prioridadLabel[e.prioridad] }}</td>
                    <td [ngClass]="'text-' + estadoColor[e.estado]">{{ estadoLabel[e.estado] }}</td>
                    <td><a [routerLink]="['/emergencias', e.id]">Ver detalle</a></td>
                  </tr>
                } @empty {
                  <tr><td colspan="4" class="text-body-secondary">Sin emergencias registradas todavia.</td></tr>
                }
              </tbody>
            </table>
          </c-card-body>
        </c-card>
      </div>
    </div>
  `
})
export class EmergenciasComponent implements OnInit {
  readonly emergencias = signal<Emergencia[]>([]);
  readonly guardando = signal(false);
  readonly errorGeneral = signal<string | null>(null);
  readonly errores = signal<Record<string, string>>({});

  readonly prioridadLabel = PRIORIDAD_LABEL;
  readonly estadoLabel = ESTADO_EMERGENCIA_LABEL;
  readonly estadoColor = ESTADO_EMERGENCIA_COLOR;

  form: CrearEmergenciaRequest = {
    tipo: '',
    descripcion: '',
    latitud: 0,
    longitud: 0,
    prioridad: Prioridad.Media,
    reportanteNombre: '',
    reportanteContacto: ''
  };

  constructor(private readonly emergenciasService: EmergenciasService) {}

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.emergenciasService.listar().subscribe((data) => this.emergencias.set(data));
  }

  crear(): void {
    this.errorGeneral.set(null);
    this.errores.set({});
    this.guardando.set(true);

    this.emergenciasService.crear(this.form).subscribe({
      next: () => {
        this.guardando.set(false);
        this.form = {
          tipo: '', descripcion: '', latitud: 0, longitud: 0,
          prioridad: Prioridad.Media, reportanteNombre: '', reportanteContacto: ''
        };
        this.cargar();
      },
      error: (err: HttpErrorResponse) => {
        this.guardando.set(false);
        const porCampo = extraerErroresPorCampo(err);
        if (Object.keys(porCampo).length > 0) {
          this.errores.set(porCampo);
        } else {
          this.errorGeneral.set('No se pudo registrar la emergencia.');
        }
      }
    });
  }
}
