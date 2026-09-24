import { Component } from '@angular/core';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';

// US1: registrar y visualizar una emergencia. Placeholder de la Fase Foundational;
// el formulario y el detalle se conectan en US1 (T025, T027-T028).
@Component({
  selector: 'app-emergencias',
  standalone: true,
  imports: [CardComponent, CardHeaderComponent, CardBodyComponent],
  template: `
    <c-card>
      <c-card-header>Emergencias</c-card-header>
      <c-card-body>
        <p>Aqui se listaran las emergencias registradas y el formulario de "Nueva emergencia".</p>
      </c-card-body>
    </c-card>
  `
})
export class EmergenciasComponent {}
