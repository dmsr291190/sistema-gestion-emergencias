import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import * as L from 'leaflet';
import { EmergenciasService } from '../../core/services/emergencias.service';
import { UnidadesService } from '../../core/services/unidades.service';

// FR-003: mapa operativo con las emergencias activas y las unidades de respuesta.
// SC-002: la emergencia debe verse en el mapa en menos de 5 segundos tras registrarse
// (aqui se recarga al entrar a la vista; la actualizacion en vivo via SignalR
// queda para T017 completo).
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
  private unidadIcon?: L.Icon;

  constructor(
    private readonly emergenciasService: EmergenciasService,
    private readonly unidadesService: UnidadesService
  ) {}

  ngAfterViewInit(): void {
    // Los iconos por defecto de Leaflet se sirven desde /leaflet/ (ver angular.json assets),
    // en vez de depender de una CDN externa.
    L.Icon.Default.mergeOptions({
      iconRetinaUrl: 'leaflet/marker-icon-2x.png',
      iconUrl: 'leaflet/marker-icon.png',
      shadowUrl: 'leaflet/marker-shadow.png'
    });

    this.unidadIcon = L.icon({
      iconUrl: 'leaflet/marker-icon.png',
      iconRetinaUrl: 'leaflet/marker-icon-2x.png',
      shadowUrl: 'leaflet/marker-shadow.png',
      iconSize: [20, 33],
      iconAnchor: [10, 33],
      className: 'sige-unidad-marker'
    });

    // Centro por defecto: Lima, Peru (ajustar segun la zona real de despliegue).
    this.map = L.map(this.mapContainer.nativeElement).setView([-12.05, -77.03], 12);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
      maxZoom: 19
    }).addTo(this.map);

    this.emergenciasService.listar().subscribe((emergencias) => {
      for (const e of emergencias) {
        L.marker([e.ubicacion.latitud, e.ubicacion.longitud])
          .addTo(this.map!)
          .bindPopup(`<strong>Emergencia: ${e.tipoEmergencia?.nombre ?? '(sin tipo)'}</strong><br>${e.descripcion}`);
      }
    });

    this.unidadesService.listar().subscribe((unidades) => {
      for (const u of unidades) {
        if (u.latitud == null || u.longitud == null) continue;
        L.marker([u.latitud, u.longitud], { icon: this.unidadIcon })
          .addTo(this.map!)
          .bindPopup(`<strong>Unidad: ${u.identificador}</strong>`);
      }
    });
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }
}
