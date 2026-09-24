import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import * as L from 'leaflet';
import { EmergenciasService } from '../../core/services/emergencias.service';

// FR-003: mapa operativo con las emergencias activas (Leaflet + OpenStreetMap).
// SC-002: la emergencia debe verse en el mapa en menos de 5 segundos tras registrarse
// (aqui se recarga al entrar a la vista; la actualizacion en vivo via SignalR
// queda para T017/T026 completos).
@Component({
  selector: 'app-mapa',
  standalone: true,
  imports: [CardComponent, CardHeaderComponent, CardBodyComponent],
  template: `
    <c-card>
      <c-card-header>Mapa operativo</c-card-header>
      <c-card-body>
        <div #mapContainer style="height: 480px; border-radius: 0.25rem;"></div>
      </c-card-body>
    </c-card>
  `
})
export class MapaComponent implements AfterViewInit, OnDestroy {
  @ViewChild('mapContainer') mapContainer!: ElementRef<HTMLDivElement>;
  private map?: L.Map;

  constructor(private readonly emergenciasService: EmergenciasService) {}

  ngAfterViewInit(): void {
    // Los iconos por defecto de Leaflet se sirven desde /leaflet/ (ver angular.json assets),
    // en vez de depender de una CDN externa.
    L.Icon.Default.mergeOptions({
      iconRetinaUrl: 'leaflet/marker-icon-2x.png',
      iconUrl: 'leaflet/marker-icon.png',
      shadowUrl: 'leaflet/marker-shadow.png'
    });

    // Centro por defecto: Lima, Peru (ajustar segun la zona real de despliegue).
    this.map = L.map(this.mapContainer.nativeElement).setView([-12.05, -77.03], 12);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
      maxZoom: 19
    }).addTo(this.map);

    this.emergenciasService.listar().subscribe((emergencias) => {
      for (const e of emergencias) {
        L.marker([e.latitud, e.longitud])
          .addTo(this.map!)
          .bindPopup(`<strong>${e.tipo}</strong><br>${e.descripcion}`);
      }
    });
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }
}
