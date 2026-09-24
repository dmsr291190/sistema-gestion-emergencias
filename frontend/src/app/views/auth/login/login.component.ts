import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import { AuthService } from '../../../core/services/auth.service';

// FR-012: acceso autenticado por rol (Operador/Supervisor). Conectado a
// POST /api/Users/login (Identity de la plantilla Jason Taylor).
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CardComponent, CardHeaderComponent, CardBodyComponent],
  template: `
    <div class="d-flex justify-content-center align-items-center min-vh-100 bg-body-tertiary">
      <c-card style="width: 24rem;">
        <c-card-header>Iniciar sesion — SIGE</c-card-header>
        <c-card-body>
          <form (ngSubmit)="onSubmit()">
            <div class="mb-3">
              <label class="form-label">Correo</label>
              <input class="form-control" type="email" name="email" [(ngModel)]="email" required />
            </div>
            <div class="mb-3">
              <label class="form-label">Contrasena</label>
              <input class="form-control" type="password" name="password" [(ngModel)]="password" required />
            </div>
            @if (error()) {
              <div class="alert alert-danger py-2">{{ error() }}</div>
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
  readonly error = signal<string | null>(null);

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  onSubmit(): void {
    this.error.set(null);
    this.loading.set(true);

    this.auth.login(this.email, this.password).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigateByUrl('/dashboard');
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Correo o contrasena incorrectos.');
      }
    });
  }
}
