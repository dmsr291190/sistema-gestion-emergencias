import { HttpErrorResponse } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import { AuthService } from '../../../core/services/auth.service';
import { FormFieldComponent } from '../../../shared/form-field/form-field.component';

// FR-012: acceso autenticado por rol (Operador/Supervisor). Conectado a
// POST /api/Users/login (Identity de la plantilla Jason Taylor).
// FR-021, FR-025: validacion y estructura de campo consistentes con el resto del MVP.
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CardComponent, CardHeaderComponent, CardBodyComponent, FormFieldComponent],
  template: `
    <div class="d-flex justify-content-center align-items-center min-vh-100 bg-body-tertiary">
      <c-card style="width: 24rem;">
        <c-card-header>Iniciar sesion — SIGE</c-card-header>
        <c-card-body>
          <form (ngSubmit)="onSubmit()">
            <app-form-field label="Correo" [error]="errores()['email']">
              <input class="form-control" type="email" name="email" [(ngModel)]="email" required />
            </app-form-field>
            <app-form-field label="Contrasena" [error]="errores()['password']">
              <input class="form-control" type="password" name="password" [(ngModel)]="password" required />
            </app-form-field>
            @if (errorGeneral()) {
              <div class="alert alert-danger py-2">{{ errorGeneral() }}</div>
            }
            <button class="btn btn-primary w-100" type="submit" [disabled]="loading()">
              {{ loading() ? 'Ingresando...' : 'Ingresar' }}
            </button>
          </form>
          <p class="text-body-secondary small mt-3 mb-0">
            Demo: operador&#64;sige.local / Operador123! — supervisor&#64;sige.local / Supervisor123!
          </p>
        </c-card-body>
      </c-card>
    </div>
  `
})
export class LoginComponent {
  email = '';
  password = '';
  readonly loading = signal(false);
  readonly errorGeneral = signal<string | null>(null);
  readonly errores = signal<Record<string, string>>({});

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  onSubmit(): void {
    this.errorGeneral.set(null);
    this.errores.set({});

    // FR-021: validacion minima en el cliente antes de llamar a la API.
    const camposFaltantes: Record<string, string> = {};
    if (!this.email) camposFaltantes['email'] = 'El correo es obligatorio.';
    if (!this.password) camposFaltantes['password'] = 'La contrasena es obligatoria.';
    if (Object.keys(camposFaltantes).length > 0) {
      this.errores.set(camposFaltantes);
      return;
    }

    this.loading.set(true);
    this.auth.login(this.email, this.password).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigateByUrl('/dashboard');
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.errorGeneral.set(
          err.status === 401 ? 'Correo o contrasena incorrectos.' : 'No se pudo iniciar sesion.'
        );
      }
    });
  }
}
