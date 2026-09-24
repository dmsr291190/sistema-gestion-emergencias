import { Component } from '@angular/core';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';

// FR-011: dashboard con indicadores operativos basicos.
// Placeholder de la Fase Foundational; los indicadores reales se conectan en US5 (T052-T055).
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CardComponent, CardHeaderComponent, CardBodyComponent],
  template: `
    <c-card>
      <c-card-header>Dashboard</c-card-header>
      <c-card-body>
        <p>Indicadores operativos (emergencias activas por estado/prioridad, unidades disponibles vs. ocupadas).</p>
        <p class="text-body-secondary">Pendiente de conectar a <code>GET /dashboard/indicadores</code> (US5).</p>
      </c-card-body>
    </c-card>
  `
})
export class DashboardComponent {}
