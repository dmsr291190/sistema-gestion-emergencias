import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import { AuthService } from '../../core/services/auth.service';
import { UnidadesService } from '../../core/services/unidades.service';
import { CrearUnidadRequest, EstadoOperativoUnidad, TipoUnidad, Unidad } from '../../core/models/unidad.model';

// US2: administrar unidades y conocer su disponibilidad (FR-004, FR-005).
// Alta y cambio de estado solo visibles/habilitados para el rol Supervisor.
@Component({
  selector: 'app-unidades',
  standalone: true,
  imports: [FormsModule, CardComponent, CardHeaderComponent, CardBodyComponent],
  template: `
    <div class="row g-4">
      @if (auth.isSupervisor()) {
        <div class="col-md-4">
          <c-card>
            <c-card-header>Nueva unidad</c-card-header>
            <c-card-body>
              <form (ngSubmit)="crear()">
                <div class="mb-2">
                  <label class="form-label">Tipo</label>
                  <select class="form-select" name="tipo" [(ngModel)]="form.tipo">
                    <option [ngValue]="0">Ambulancia</option>
                    <option [ngValue]="1">Bomberos</option>
                    <option [ngValue]="2">Patrullero</option>
                  </select>
                </div>
                <div class="mb-2">
                  <label class="form-label">Identificador</label>
                  <input class="form-control" name="identificador" [(ngModel)]="form.identificador" required placeholder="ej. AMB-02" />
                </div>
                <div class="row">
                  <div class="col-6 mb-3">
                    <label class="form-label">Latitud (opcional)</label>
                    <input class="form-control" type="number" step="0.0001" name="latitud" [(ngModel)]="form.latitud" />
                  </div>
                  <div class="col-6 mb-3">
                    <label class="form-label">Longitud (opcional)</label>
                    <input class="form-control" type="number" step="0.0001" name="longitud" [(ngModel)]="form.longitud" />
                  </div>
                </div>
                @if (error()) {
                  <div class="alert alert-danger py-2">{{ error() }}</div>
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
                <tr><th>Identificador</th><th>Tipo</th><th>Estado</th>@if (auth.isSupervisor()) {<th></th>}</tr>
              </thead>
              <tbody>
                @for (u of unidades(); track u.id) {
                  <tr>
                    <td>{{ u.identificador }}</td>
                    <td>{{ tipoLabel[u.tipo] }}</td>
                    <td>{{ estadoLabel[u.estadoOperativo] }}</td>
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
                  <tr><td colspan="4" class="text-body-secondary">Sin unidades registradas.</td></tr>
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
  readonly error = signal<string | null>(null);

  readonly tipoLabel = { 0: 'Ambulancia', 1: 'Bomberos', 2: 'Patrullero' };
  readonly estadoLabel = { 0: 'Disponible', 1: 'Ocupada', 2: 'Fuera de servicio' };

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
    this.error.set(null);
    this.guardando.set(true);

    this.unidadesService.crear(this.form).subscribe({
      next: () => {
        this.guardando.set(false);
        this.form = { tipo: TipoUnidad.Ambulancia, identificador: '' };
        this.cargar();
      },
      error: () => {
        this.guardando.set(false);
        this.error.set('No se pudo registrar la unidad. Revisa el identificador.');
      }
    });
  }

  cambiarEstado(unidad: Unidad, nuevoEstado: EstadoOperativoUnidad): void {
    this.unidadesService.cambiarEstado(unidad.id, nuevoEstado).subscribe(() => this.cargar());
  }
}
