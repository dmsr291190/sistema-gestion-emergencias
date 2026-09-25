import { NgClass } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import { EmergenciasService } from '../../core/services/emergencias.service';
import { TiposEmergenciaService } from '../../core/services/tipos-emergencia.service';
import { extraerErroresPorCampo } from '../../core/utils/validation-errors';
import { FormFieldComponent } from '../../shared/form-field/form-field.component';
import { Ambito, CrearEmergenciaRequest, Emergencia, Prioridad } from '../../core/models/emergencia.model';
import { TipoEmergencia } from '../../core/models/tipo-emergencia.model';
import { ESTADO_EMERGENCIA_COLOR, ESTADO_EMERGENCIA_LABEL, PRIORIDAD_LABEL } from '../../core/models/labels';

// US1 (MVP): registrar y visualizar una emergencia (FR-001, FR-002, FR-015, FR-019).
// US3 (ampliación 002): catálogo nacional de tipos + cobertura geográfica completa
// (FR-101, FR-104, FR-105); US6: afectados/heridos/desaparecidos/fallecidos/evacuados
// (FR-125).
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
              <app-form-field label="Tipo de emergencia" [error]="errores()['tipoemergenciaid']">
                <select class="form-select" name="tipoEmergenciaId" [(ngModel)]="form.tipoEmergenciaId" required>
                  <option [ngValue]="0" disabled>-- Selecciona un tipo --</option>
                  @for (t of tipos(); track t.id) {
                    <option [ngValue]="t.id">{{ t.nombre }}</option>
                  }
                </select>
              </app-form-field>
              <app-form-field label="Descripcion" [error]="errores()['descripcion']">
                <textarea class="form-control" name="descripcion" [(ngModel)]="form.descripcion" required></textarea>
              </app-form-field>

              <div class="form-check mb-2">
                <input class="form-check-input" type="checkbox" id="sinDireccionFormal"
                       name="sinDireccionFormal" [(ngModel)]="form.ubicacion.sinDireccionFormal" />
                <label class="form-check-label" for="sinDireccionFormal">
                  Sin dirección formal (zona marítima o remota)
                </label>
              </div>

              <app-form-field label="Ámbito">
                <select class="form-select" name="ambito" [(ngModel)]="form.ubicacion.ambito">
                  <option [ngValue]="0">Terrestre</option>
                  <option [ngValue]="1">Marítimo</option>
                  <option [ngValue]="2">Aéreo</option>
                  <option [ngValue]="3">Mixto</option>
                </select>
              </app-form-field>

              @if (!form.ubicacion.sinDireccionFormal) {
                <app-form-field label="Departamento" [error]="errores()['ubicacion.departamento']">
                  <input class="form-control" name="departamento" [(ngModel)]="form.ubicacion.departamento" />
                </app-form-field>
                <app-form-field label="Provincia" [error]="errores()['ubicacion.provincia']">
                  <input class="form-control" name="provincia" [(ngModel)]="form.ubicacion.provincia" />
                </app-form-field>
                <app-form-field label="Distrito" [error]="errores()['ubicacion.distrito']">
                  <input class="form-control" name="distrito" [(ngModel)]="form.ubicacion.distrito" />
                </app-form-field>
                <app-form-field label="Centro poblado (opcional)">
                  <input class="form-control" name="centroPoblado" [(ngModel)]="form.ubicacion.centroPoblado" />
                </app-form-field>
                <app-form-field label="Dirección (opcional)">
                  <input class="form-control" name="direccion" [(ngModel)]="form.ubicacion.direccion" />
                </app-form-field>
                <app-form-field label="Referencia (opcional)">
                  <input class="form-control" name="referencia" [(ngModel)]="form.ubicacion.referencia" />
                </app-form-field>
              }

              <div class="row">
                <div class="col-6">
                  <app-form-field label="Latitud" [error]="errores()['ubicacion.latitud']">
                    <input class="form-control" type="number" step="0.0001" name="latitud" [(ngModel)]="form.ubicacion.latitud" required />
                  </app-form-field>
                </div>
                <div class="col-6">
                  <app-form-field label="Longitud" [error]="errores()['ubicacion.longitud']">
                    <input class="form-control" type="number" step="0.0001" name="longitud" [(ngModel)]="form.ubicacion.longitud" required />
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

              <h6 class="mt-3">Afectados</h6>
              <div class="row">
                <div class="col-4"><app-form-field label="Heridos"><input class="form-control" type="number" min="0" name="heridos" [(ngModel)]="form.heridos" /></app-form-field></div>
                <div class="col-4"><app-form-field label="Desaparecidos"><input class="form-control" type="number" min="0" name="desaparecidos" [(ngModel)]="form.desaparecidos" /></app-form-field></div>
                <div class="col-4"><app-form-field label="Fallecidos"><input class="form-control" type="number" min="0" name="fallecidos" [(ngModel)]="form.fallecidos" /></app-form-field></div>
              </div>
              <div class="row">
                <div class="col-6"><app-form-field label="Afectados totales"><input class="form-control" type="number" min="0" name="afectados" [(ngModel)]="form.afectados" /></app-form-field></div>
                <div class="col-6"><app-form-field label="Evacuados"><input class="form-control" type="number" min="0" name="evacuados" [(ngModel)]="form.evacuados" /></app-form-field></div>
              </div>

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
                    <td>{{ e.tipoEmergencia?.nombre ?? '(sin tipo)' }}</td>
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
  readonly tipos = signal<TipoEmergencia[]>([]);
  readonly guardando = signal(false);
  readonly errorGeneral = signal<string | null>(null);
  readonly errores = signal<Record<string, string>>({});

  readonly prioridadLabel = PRIORIDAD_LABEL;
  readonly estadoLabel = ESTADO_EMERGENCIA_LABEL;
  readonly estadoColor = ESTADO_EMERGENCIA_COLOR;

  form: CrearEmergenciaRequest = this.formVacio();

  constructor(
    private readonly emergenciasService: EmergenciasService,
    private readonly tiposEmergenciaService: TiposEmergenciaService
  ) {}

  ngOnInit(): void {
    this.cargar();
    this.tiposEmergenciaService.listar(true).subscribe((data) => this.tipos.set(data));
  }

  private formVacio(): CrearEmergenciaRequest {
    return {
      tipoEmergenciaId: 0,
      descripcion: '',
      ubicacion: { latitud: 0, longitud: 0, ambito: Ambito.Terrestre, sinDireccionFormal: false },
      afectados: 0,
      heridos: 0,
      desaparecidos: 0,
      fallecidos: 0,
      evacuados: 0,
      prioridad: Prioridad.Media,
      reportanteNombre: '',
      reportanteContacto: ''
    };
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
        this.form = this.formVacio();
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
