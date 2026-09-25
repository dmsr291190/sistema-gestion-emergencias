import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import { InstitucionesService } from '../../core/services/instituciones.service';
import { UsuariosService } from '../../core/services/usuarios.service';
import { extraerErroresPorCampo } from '../../core/utils/validation-errors';
import { FormFieldComponent } from '../../shared/form-field/form-field.component';
import { Institucion } from '../../core/models/institucion.model';
import { CrearUsuarioRequest, ROLES_DISPONIBLES, UsuarioAdmin } from '../../core/models/usuario.model';

// US1 (Ampliación Operativa Nacional): administrar usuarios y roles ampliados.
// FR-116: esta pantalla solo es accesible para el rol Administrador (guard de ruta).
@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [FormsModule, DatePipe, CardComponent, CardHeaderComponent, CardBodyComponent, FormFieldComponent],
  template: `
    <div class="row g-4">
      <div class="col-md-4">
        <c-card>
          <c-card-header>Nuevo usuario</c-card-header>
          <c-card-body>
            <form (ngSubmit)="crear()">
              <app-form-field label="Nombre de usuario" [error]="errores()['username']">
                <input class="form-control" name="userName" [(ngModel)]="form.userName" required placeholder="ej. logistica.norte" />
              </app-form-field>
              <app-form-field label="Nombre completo" [error]="errores()['nombrecompleto']">
                <input class="form-control" name="nombreCompleto" [(ngModel)]="form.nombreCompleto" required />
              </app-form-field>
              <app-form-field label="Contraseña" [error]="errores()['password']">
                <input class="form-control" type="password" name="password" [(ngModel)]="form.password" required />
              </app-form-field>
              <app-form-field label="Institución (opcional)">
                <select class="form-select" name="institucionId" [(ngModel)]="form.institucionId">
                  <option [ngValue]="null">-- Sin institución --</option>
                  @for (i of instituciones(); track i.id) {
                    <option [ngValue]="i.id">{{ i.nombre }}</option>
                  }
                </select>
              </app-form-field>
              <app-form-field label="Roles" [error]="errores()['roles']">
                <select class="form-select" multiple name="roles" [(ngModel)]="form.roles" size="7">
                  @for (r of rolesDisponibles; track r) {
                    <option [value]="r">{{ r }}</option>
                  }
                </select>
              </app-form-field>
              @if (errorGeneral()) {
                <div class="alert alert-danger py-2">{{ errorGeneral() }}</div>
              }
              <button class="btn btn-primary" type="submit" [disabled]="guardando()">
                {{ guardando() ? 'Guardando...' : 'Crear usuario' }}
              </button>
            </form>
          </c-card-body>
        </c-card>
      </div>
      <div class="col-md-8">
        <c-card>
          <c-card-header>Usuarios</c-card-header>
          <c-card-body>
            <table class="table table-sm align-middle">
              <thead>
                <tr>
                  <th>Usuario</th><th>Roles</th><th>Estado</th><th>Último acceso</th><th>Intentos fallidos</th><th></th>
                </tr>
              </thead>
              <tbody>
                @for (u of usuarios(); track u.id) {
                  <tr>
                    <td>{{ u.nombreCompleto }}<br /><small class="text-body-secondary">{{ u.userName }}</small></td>
                    <td>{{ u.roles.join(', ') }}</td>
                    <td>
                      @if (u.bloqueado) {
                        <span class="badge bg-danger">Bloqueado</span>
                      } @else if (u.requiereCambioPassword) {
                        <span class="badge bg-warning">Requiere cambio de contraseña</span>
                      } @else {
                        <span class="badge bg-success">Activo</span>
                      }
                    </td>
                    <td>{{ u.ultimoAcceso ? (u.ultimoAcceso | date: 'short') : 'Nunca' }}</td>
                    <td>{{ u.intentosFallidos }}</td>
                    <td class="text-nowrap">
                      @if (u.bloqueado) {
                        <button class="btn btn-sm btn-outline-success" (click)="desbloquear(u)">Desbloquear</button>
                      } @else {
                        <button class="btn btn-sm btn-outline-danger" (click)="bloquear(u)">Bloquear</button>
                      }
                      <button class="btn btn-sm btn-outline-secondary" (click)="forzarCambioPassword(u)">Forzar cambio</button>
                    </td>
                  </tr>
                } @empty {
                  <tr><td colspan="6" class="text-body-secondary">Sin usuarios registrados.</td></tr>
                }
              </tbody>
            </table>
          </c-card-body>
        </c-card>
      </div>
    </div>
  `
})
export class UsuariosComponent implements OnInit {
  readonly usuarios = signal<UsuarioAdmin[]>([]);
  readonly instituciones = signal<Institucion[]>([]);
  readonly guardando = signal(false);
  readonly errorGeneral = signal<string | null>(null);
  readonly errores = signal<Record<string, string>>({});

  readonly rolesDisponibles = ROLES_DISPONIBLES;

  form: CrearUsuarioRequest = { userName: '', nombreCompleto: '', password: '', institucionId: null, roles: [] };

  constructor(
    private readonly usuariosService: UsuariosService,
    private readonly institucionesService: InstitucionesService
  ) {}

  ngOnInit(): void {
    this.cargar();
    this.institucionesService.listar().subscribe((data) => this.instituciones.set(data));
  }

  cargar(): void {
    this.usuariosService.listar().subscribe((data) => this.usuarios.set(data));
  }

  crear(): void {
    this.errorGeneral.set(null);
    this.errores.set({});
    this.guardando.set(true);

    this.usuariosService.crear(this.form).subscribe({
      next: () => {
        this.guardando.set(false);
        this.form = { userName: '', nombreCompleto: '', password: '', institucionId: null, roles: [] };
        this.cargar();
      },
      error: (err: HttpErrorResponse) => {
        this.guardando.set(false);
        const porCampo = extraerErroresPorCampo(err);
        if (Object.keys(porCampo).length > 0) {
          this.errores.set(porCampo);
        } else {
          this.errorGeneral.set('No se pudo crear el usuario.');
        }
      }
    });
  }

  bloquear(usuario: UsuarioAdmin): void {
    this.usuariosService.bloquear(usuario.id).subscribe(() => this.cargar());
  }

  desbloquear(usuario: UsuarioAdmin): void {
    this.usuariosService.desbloquear(usuario.id).subscribe(() => this.cargar());
  }

  forzarCambioPassword(usuario: UsuarioAdmin): void {
    const passwordTemporal = window.prompt(`Contraseña temporal para ${usuario.userName}:`, 'Temporal123!');
    if (!passwordTemporal) return;

    this.usuariosService.forzarCambioPassword(usuario.id, passwordTemporal).subscribe(() => this.cargar());
  }
}
