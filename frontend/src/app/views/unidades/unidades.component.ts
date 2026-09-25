import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import { AuthService } from '../../core/services/auth.service';
import { UnidadesService } from '../../core/services/unidades.service';
import { extraerErroresPorCampo } from '../../core/utils/validation-errors';
import { FormFieldComponent } from '../../shared/form-field/form-field.component';
import { CrearUnidadRequest, EstadoOperativoUnidad, TipoUnidad, Unidad } from '../../core/models/unidad.model';
import { ESTADO_OPERATIVO_UNIDAD_LABEL, TIPO_UNIDAD_LABEL } from '../../core/models/labels';

// US2: administrar unidades y conocer su disponibilidad (FR-004, FR-005).
// Alta y cambio de estado solo visibles/habilitados para el rol Supervisor.
// FR-021, FR-025: validacion por campo y estructura de campo compartida.
@Component({
  selector: 'app-unidades',
  standalone: true,
  imports: [FormsModule, RouterLink, CardComponent, CardHeaderComponent, CardBodyComponent, FormFieldComponent],
  template: `
    <div class="row g-4">
      @if (auth.isSupervisor()) {
        <div class="col-md-4">
          <c-card>
            <c-card-header>Nueva unidad</c-card-header>
            <c-card-body>
              <form (ngSubmit)="crear()">
                <app-form-field label="Tipo" [error]="errores()['tipo']">
                  <select class="form-select" name="tipo" [(ngModel)]="form.tipo">
                    <option [ngValue]="0">Ambulancia</option>
                    <option [ngValue]="1">Bomberos</option>
                    <option [ngValue]="2">Patrullero</option>
                  </select>
                </app-form-field>
                <app-form-field label="Identificador" [error]="errores()['identificador']">
                  <input class="form-control" name="identificador" [(ngModel)]="form.identificador" required placeholder="ej. AMB-02" />
                </app-form-field>
                <div class="row">
                  <div class="col-6">
                    <app-form-field label="Latitud (opcional)" [error]="errores()['latitud']">
                      <input class="form-control" type="number" step="0.0001" name="latitud" [(ngModel)]="form.latitud" />
                    </app-form-field>
                  </div>
                  <div class="col-6">
                    <app-form-field label="Longitud (opcional)" [error]="errores()['longitud']">
                      <input class="form-control" type="number" step="0.0001" name="longitud" [(ngModel)]="form.longitud" />
                    </app-form-field>
                  </div>
                </div>
                @if (errorGeneral()) {
                  <div class="alert alert-danger py-2">{{ errorGeneral() }}</div>
                }
                <button class="btn btn-primary" type="submit" [disabled]="guardando()">
                  {{ guardando() ? 'Guardando...' : 'Registrar unidad' }}
                </button>
              </form>
            </c-card-body>
          </c-card>
        </div>
      }
      <div [class]="auth.isSupervisor() ? 'col-md-8' : 'col-md-12'">
        <c-card>
          <c-card-header>Unidades de respuesta</c-card-header>
          <c-card-body>
            <table class="table table-sm">
              <thead>
                <tr><th>Identificador</th><th>Tipo</th><th>Estado</th><th></th>@if (auth.isSupervisor()) {<th></th>}</tr>
              </thead>
              <tbody>
                @for (u of unidades(); track u.id) {
                  <tr>
                    <td>{{ u.identificador }}</td>
                    <td>{{ tipoLabel[u.tipo] }}</td>
                    <td>{{ estadoLabel[u.estadoOperativo] }}</td>
                    <td><a [routerLink]="['/unidades', u.id]">Ver detalle</a></td>
                    @if (auth.isSupervisor()) {
                      <td>
                        <select class="form-select form-select-sm" [ngModel]="u.estadoOperativo"
                                (ngModelChange)="cambiarEstado(u, $event)">
                          <option [ngValue]="0">Disponible</option>
                          <option [ngValue]="1">Ocupada</option>
                          <option [ngValue]="2">Fuera de servicio</option>
                        </select>
                      </td>
                    }
                  </tr>
                } @empty {
                  <tr><td colspan="5" class="text-body-secondary">Sin unidades registradas.</td></tr>
                }
              </tbody>
            </table>
          </c-card-body>
        </c-card>
      </div>
    </div>
  `
})
export class UnidadesComponent implements OnInit {
  readonly unidades = signal<Unidad[]>([]);
  readonly guardando = signal(false);
  readonly errorGeneral = signal<string | null>(null);
  readonly errores = signal<Record<string, string>>({});

  readonly tipoLabel = TIPO_UNIDAD_LABEL;
  readonly estadoLabel = ESTADO_OPERATIVO_UNIDAD_LABEL;

  form: CrearUnidadRequest = { tipo: TipoUnidad.Ambulancia, identificador: '' };

  constructor(
    readonly auth: AuthService,
    private readonly unidadesService: UnidadesService
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.unidadesService.listar().subscribe((data) => this.unidades.set(data));
  }

  crear(): void {
    this.errorGeneral.set(null);
    this.errores.set({});
    this.guardando.set(true);

    this.unidadesService.crear(this.form).subscribe({
      next: () => {
        this.guardando.set(false);
        this.form = { tipo: TipoUnidad.Ambulancia, identificador: '' };
        this.cargar();
      },
      error: (err: HttpErrorResponse) => {
        this.guardando.set(false);
        const porCampo = extraerErroresPorCampo(err);
        if (Object.keys(porCampo).length > 0) {
          this.errores.set(porCampo);
        } else {
          this.errorGeneral.set('No se pudo registrar la unidad.');
        }
      }
    });
  }

  cambiarEstado(unidad: Unidad, nuevoEstado: EstadoOperativoUnidad): void {
    this.unidadesService.cambiarEstado(unidad.id, nuevoEstado).subscribe(() => this.cargar());
  }
}
