import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import { EmergenciasService } from '../../core/services/emergencias.service';
import { CrearEmergenciaRequest, Emergencia, EstadoEmergencia, Prioridad } from '../../core/models/emergencia.model';

// US1: registrar y visualizar una emergencia (FR-001, FR-002, FR-015, FR-019).
@Component({
  selector: 'app-emergencias',
  standalone: true,
  imports: [FormsModule, RouterLink, CardComponent, CardHeaderComponent, CardBodyComponent],
  template: `
    <div class="row g-4">
      <div class="col-md-5">
        <c-card>
          <c-card-header>Nueva emergencia</c-card-header>
          <c-card-body>
            <form (ngSubmit)="crear()">
              <div class="mb-2">
                <label class="form-label">Tipo</label>
                <input class="form-control" name="tipo" [(ngModel)]="form.tipo" required />
              </div>
              <div class="mb-2">
                <label class="form-label">Descripcion</label>
                <textarea class="form-control" name="descripcion" [(ngModel)]="form.descripcion" required></textarea>
              </div>
              <div class="row">
                <div class="col-6 mb-2">
                  <label class="form-label">Latitud</label>
                  <input class="form-control" type="number" step="0.0001" name="latitud" [(ngModel)]="form.latitud" required />
                </div>
                <div class="col-6 mb-2">
                  <label class="form-label">Longitud</label>
                  <input class="form-control" type="number" step="0.0001" name="longitud" [(ngModel)]="form.longitud" required />
                </div>
              </div>
              <div class="mb-2">
                <label class="form-label">Prioridad</label>
                <select class="form-select" name="prioridad" [(ngModel)]="form.prioridad">
                  <option [ngValue]="0">Baja</option>
                  <option [ngValue]="1">Media</option>
                  <option [ngValue]="2">Alta</option>
                  <option [ngValue]="3">Critica</option>
                </select>
              </div>
              <div class="mb-2">
                <label class="form-label">Nombre del reportante</label>
                <input class="form-control" name="reportanteNombre" [(ngModel)]="form.reportanteNombre" required />
              </div>
              <div class="mb-3">
                <label class="form-label">Contacto del reportante (opcional)</label>
                <input class="form-control" name="reportanteContacto" [(ngModel)]="form.reportanteContacto" />
              </div>
              @if (error()) {
                <div class="alert alert-danger py-2">{{ error() }}</div>
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
                    <td>{{ estadoLabel[e.estado] }}</td>
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
  readonly error = signal<string | null>(null);

  readonly prioridadLabel = { 0: 'Baja', 1: 'Media', 2: 'Alta', 3: 'Critica' };
  readonly estadoLabel = {
    0: 'Reportada', 1: 'Validada', 2: 'Despachada', 3: 'En ruta', 4: 'En el lugar', 5: 'Atendida', 6: 'Cerrada'
  };

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
    this.error.set(null);
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
      error: () => {
        this.guardando.set(false);
        this.error.set('No se pudo registrar la emergencia. Revisa los campos obligatorios.');
      }
    });
  }
}
