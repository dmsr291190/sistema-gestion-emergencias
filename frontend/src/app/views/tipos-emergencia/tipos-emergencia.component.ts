import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import { TiposEmergenciaService } from '../../core/services/tipos-emergencia.service';
import { extraerErroresPorCampo } from '../../core/utils/validation-errors';
import { FormFieldComponent } from '../../shared/form-field/form-field.component';
import { Ambito, Prioridad } from '../../core/models/emergencia.model';
import { CrearTipoEmergenciaRequest, TipoEmergencia } from '../../core/models/tipo-emergencia.model';

// US3 (Ampliación Operativa Nacional): administración simple del catálogo de
// tipos de emergencia (crear/editar/activar/desactivar). FR-101, FR-102.
// Solo accesible para el rol Administrador (guard de ruta).
@Component({
  selector: 'app-tipos-emergencia',
  standalone: true,
  imports: [FormsModule, CardComponent, CardHeaderComponent, CardBodyComponent, FormFieldComponent],
  template: `
    <div class="row g-4">
      <div class="col-md-4">
        <c-card>
          <c-card-header>Nuevo tipo de emergencia</c-card-header>
          <c-card-body>
            <form (ngSubmit)="crear()">
              <app-form-field label="Nombre" [error]="errores()['nombre']">
                <input class="form-control" name="nombre" [(ngModel)]="form.nombre" required />
              </app-form-field>
              <app-form-field label="Ámbito">
                <select class="form-select" name="ambito" [(ngModel)]="form.ambito">
                  <option [ngValue]="0">Terrestre</option>
                  <option [ngValue]="1">Marítimo</option>
                  <option [ngValue]="2">Aéreo</option>
                  <option [ngValue]="3">Mixto</option>
                </select>
              </app-form-field>
              <app-form-field label="Prioridad por defecto">
                <select class="form-select" name="prioridadPorDefecto" [(ngModel)]="form.prioridadPorDefecto">
                  <option [ngValue]="0">Baja</option>
                  <option [ngValue]="1">Media</option>
                  <option [ngValue]="2">Alta</option>
                  <option [ngValue]="3">Critica</option>
                </select>
              </app-form-field>
              <app-form-field label="Ícono (nombre de @coreui/icons, opcional)">
                <input class="form-control" name="icono" [(ngModel)]="form.icono" placeholder="ej. cilFire" />
              </app-form-field>
              <app-form-field label="Color (opcional)">
                <input class="form-control" type="color" name="color" [(ngModel)]="form.color" />
              </app-form-field>
              @if (errorGeneral()) {
                <div class="alert alert-danger py-2">{{ errorGeneral() }}</div>
              }
              <button class="btn btn-primary" type="submit" [disabled]="guardando()">
                {{ guardando() ? 'Guardando...' : 'Crear tipo' }}
              </button>
            </form>
          </c-card-body>
        </c-card>
      </div>
      <div class="col-md-8">
        <c-card>
          <c-card-header>Catálogo de tipos de emergencia</c-card-header>
          <c-card-body>
            <table class="table table-sm align-middle">
              <thead>
                <tr><th>Nombre</th><th>Ámbito</th><th>Prioridad</th><th>Estado</th><th></th></tr>
              </thead>
              <tbody>
                @for (t of tipos(); track t.id) {
                  <tr>
                    <td>{{ t.nombre }}</td>
                    <td>{{ ambitoLabel[t.ambito] }}</td>
                    <td>{{ prioridadLabel[t.prioridadPorDefecto] }}</td>
                    <td>
                      @if (t.activo) {
                        <span class="badge bg-success">Activo</span>
                      } @else {
                        <span class="badge bg-secondary">Inactivo</span>
                      }
                    </td>
                    <td>
                      <button class="btn btn-sm btn-outline-secondary" (click)="alternarActivo(t)">
                        {{ t.activo ? 'Desactivar' : 'Activar' }}
                      </button>
                    </td>
                  </tr>
                } @empty {
                  <tr><td colspan="5" class="text-body-secondary">Sin tipos registrados.</td></tr>
                }
              </tbody>
            </table>
          </c-card-body>
        </c-card>
      </div>
    </div>
  `
})
export class TiposEmergenciaComponent implements OnInit {
  readonly tipos = signal<TipoEmergencia[]>([]);
  readonly guardando = signal(false);
  readonly errorGeneral = signal<string | null>(null);
  readonly errores = signal<Record<string, string>>({});

  readonly ambitoLabel: Record<number, string> = { 0: 'Terrestre', 1: 'Marítimo', 2: 'Aéreo', 3: 'Mixto' };
  readonly prioridadLabel: Record<number, string> = { 0: 'Baja', 1: 'Media', 2: 'Alta', 3: 'Crítica' };

  form: CrearTipoEmergenciaRequest = { nombre: '', ambito: Ambito.Terrestre, prioridadPorDefecto: Prioridad.Media, icono: '', color: '#6c757d' };

  constructor(private readonly tiposEmergenciaService: TiposEmergenciaService) {}

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.tiposEmergenciaService.listar().subscribe((data) => this.tipos.set(data));
  }

  crear(): void {
    this.errorGeneral.set(null);
    this.errores.set({});
    this.guardando.set(true);

    this.tiposEmergenciaService.crear(this.form).subscribe({
      next: () => {
        this.guardando.set(false);
        this.form = { nombre: '', ambito: Ambito.Terrestre, prioridadPorDefecto: Prioridad.Media, icono: '', color: '#6c757d' };
        this.cargar();
      },
      error: (err: HttpErrorResponse) => {
        this.guardando.set(false);
        const porCampo = extraerErroresPorCampo(err);
        if (Object.keys(porCampo).length > 0) {
          this.errores.set(porCampo);
        } else {
          this.errorGeneral.set('No se pudo crear el tipo de emergencia.');
        }
      }
    });
  }

  alternarActivo(tipo: TipoEmergencia): void {
    this.tiposEmergenciaService
      .editar(tipo.id, {
        nombre: tipo.nombre,
        ambito: tipo.ambito,
        icono: tipo.icono,
        color: tipo.color,
        prioridadPorDefecto: tipo.prioridadPorDefecto,
        activo: !tipo.activo
      })
      .subscribe(() => this.cargar());
  }
}
