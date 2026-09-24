import { Component } from '@angular/core';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';

// US2: administrar unidades y conocer su disponibilidad (FR-004, FR-005).
// Placeholder de la Fase Foundational; se conecta en US2 (T034).
@Component({
  selector: 'app-unidades',
  standalone: true,
  imports: [CardComponent, CardHeaderComponent, CardBodyComponent],
  template: `
    <c-card>
      <c-card-header>Unidades</c-card-header>
      <c-card-body>
        <p>Aqui se listaran las unidades de respuesta (ambulancia, bomberos, patrullero) y su disponibilidad.</p>
      </c-card-body>
    </c-card>
  `
})
export class UnidadesComponent {}
