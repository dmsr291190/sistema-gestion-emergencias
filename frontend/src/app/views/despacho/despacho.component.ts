import { Component } from '@angular/core';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';

// US3: asignar una o varias unidades a una emergencia (FR-006, FR-007).
// Placeholder de la Fase Foundational; se conecta en US3 (T040-T041).
@Component({
  selector: 'app-despacho',
  standalone: true,
  imports: [CardComponent, CardHeaderComponent, CardBodyComponent],
  template: `
    <c-card>
      <c-card-header>Centro de despacho</c-card-header>
      <c-card-body>
        <p>Aqui se asignaran unidades disponibles a una emergencia seleccionada.</p>
      </c-card-body>
    </c-card>
  `
})
export class DespachoComponent {}
