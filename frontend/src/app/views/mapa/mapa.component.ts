import { Component } from '@angular/core';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';

// FR-003: mapa operativo con emergencias y unidades (Leaflet + OpenStreetMap).
// Placeholder de la Fase Foundational; la integracion con Leaflet se conecta en US1 (T026).
@Component({
  selector: 'app-mapa',
  standalone: true,
  imports: [CardComponent, CardHeaderComponent, CardBodyComponent],
  template: `
    <c-card>
      <c-card-header>Mapa operativo</c-card-header>
      <c-card-body>
        <p>Aqui se integrara Leaflet + OpenStreetMap con las emergencias activas y las unidades de respuesta.</p>
      </c-card-body>
    </c-card>
  `
})
export class MapaComponent {}
