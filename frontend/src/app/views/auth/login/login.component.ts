import { Component } from '@angular/core';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';

// FR-012: acceso autenticado por rol (Operador/Supervisor).
// Placeholder de la Fase Foundational; la integracion real con /api/Users/login
// (Identity de la plantilla Jason Taylor) y el AuthService/RoleGuard quedan para
// un commit siguiente (T016, T018 de tasks.md).
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CardComponent, CardHeaderComponent, CardBodyComponent],
  template: `
    <div class="d-flex justify-content-center align-items-center min-vh-100 bg-body-tertiary">
      <c-card style="width: 24rem;">
        <c-card-header>Iniciar sesion — SIGE</c-card-header>
        <c-card-body>
          <p class="text-body-secondary">
            Formulario de login pendiente de conectar con la API (Identity).
          </p>
        </c-card-body>
      </c-card>
    </div>
  `
})
export class LoginComponent {}
