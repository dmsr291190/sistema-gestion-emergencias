import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import * as L from 'leaflet';
import { EmergenciasService, FiltrosEmergencias } from '../../core/services/emergencias.service';
import { UnidadesService, FiltrosUnidades } from '../../core/services/unidades.service';
import { TiposEmergenciaService } from '../../core/services/tipos-emergencia.service';
import { crearIconoMarcador } from '../../core/utils/mapa-icons';
import { Ambito, Emergencia, EstadoEmergencia, Prioridad } from '../../core/models/emergencia.model';
import { EstadoOperativoUnidad, TipoUnidad, Unidad } from '../../core/models/unidad.model';
import { TipoEmergencia } from '../../core/models/tipo-emergencia.model';
import { PRIORIDAD_LABEL, TIPO_UNIDAD_LABEL } from '../../core/models/labels';
import { ICONOS_TIPO_UNIDAD } from '../../icons/icon-subset';
import { LeyendaComponent, EstadoCapas } from './leyenda/leyenda.component';

// FR-003: mapa operativo con las emergencias activas y las unidades de respuesta.
// FR-106, FR-107, FR-108, FR-109 (ampliación 002, US4): iconografía por tipo,
// leyenda con capas, filtros, y resumen rápido por marcador sin abandonar el mapa.
// SC-002: la emergencia debe verse en el mapa en menos de 5 segundos tras
// registrarse (aqui se recarga al entrar a la vista/aplicar un filtro; la
// actualizacion en vivo via SignalR queda para T017 completo del MVP).

// Puntos de interés estáticos (bases/hospitales/puertos): catálogo simple de
// demostración -- data-model.md documenta que un módulo completo de gestión de
// estos puntos queda fuera de alcance de esta ampliación salvo que se requiera.
const BASES_DEMO: { nombre: string; lat: number; lon: number }[] = [
  { nombre: 'Base Central Lima', lat: -12.05, lon: -77.05 },
  { nombre: 'Base Regional Norte', lat: -8.11, lon: -79.03 }
];

const HOSPITALES_DEMO: { nombre: string; lat: number; lon: number }[] = [
  { nombre: 'Hospital Nacional', lat: -12.06, lon: -77.04 },
  { nombre: 'Hospital Regional Piura', lat: -5.2, lon: -80.63 }
];

const PUERTOS_DEMO: { nombre: string; lat: number; lon: number }[] = [
  { nombre: 'Puerto del Callao', lat: -12.05, lon: -77.13 },
  { nombre: 'Puerto de Paita', lat: -5.09, lon: -81.11 }
];

const RUTAS_DEMO: [number, number][][] = [
  [
    [-12.05, -77.05],
    [-11.5, -76.8],
    [-11.0, -76.5]
  ]
];

@Component({
  selector: 'app-mapa',
  standalone: true,
  imports: [FormsModule, CardComponent, CardHeaderComponent, CardBodyComponent, LeyendaComponent],
  template: `
    <div class="row g-3">
      <div class="col-md-3">
        <app-leyenda [capas]="capas" (capasChange)="onCapasChange($event)"></app-leyenda>

        <c-card class="mt-3">
          <c-card-header>Filtros</c-card-header>
          <c-card-body>
            <label class="form-label small">Tipo de emergencia</label>
            <select class="form-select form-select-sm mb-2" [(ngModel)]="filtroTipoEmergenciaId" (ngModelChange)="recargarEmergencias()">
              <option [ngValue]="null">Todos</option>
              @for (t of tipos; track t.id) {
                <option [ngValue]="t.id">{{ t.nombre }}</option>
              }
            </select>

            <label class="form-label small">Prioridad</label>
            <select class="form-select form-select-sm mb-2" [(ngModel)]="filtroPrioridad" (ngModelChange)="recargarEmergencias()">
              <option [ngValue]="null">Todas</option>
              <option [ngValue]="0">Baja</option>
              <option [ngValue]="1">Media</option>
              <option [ngValue]="2">Alta</option>
              <option [ngValue]="3">Crítica</option>
            </select>

            <label class="form-label small">Ámbito</label>
            <select class="form-select form-select-sm mb-2" [(ngModel)]="filtroAmbito" (ngModelChange)="recargarEmergencias()">
              <option [ngValue]="null">Todos</option>
              <option [ngValue]="0">Terrestre</option>
              <option [ngValue]="1">Marítimo</option>
              <option [ngValue]="2">Aéreo</option>
              <option [ngValue]="3">Mixto</option>
            </select>

            <label class="form-label small">Tipo de unidad</label>
            <select class="form-select form-select-sm mb-2" [(ngModel)]="filtroTipoUnidad" (ngModelChange)="recargarUnidades()">
              <option [ngValue]="null">Todos</option>
              @for (t of tipoUnidadValores; track t) {
                <option [ngValue]="t">{{ tipoUnidadLabel[t] }}</option>
              }
            </select>

            <label class="form-label small">Disponibilidad de unidad</label>
            <select class="form-select form-select-sm" [(ngModel)]="filtroEstadoOperativo" (ngModelChange)="recargarUnidades()">
              <option [ngValue]="null">Todas</option>
              <option [ngValue]="0">Disponible</option>
              <option [ngValue]="1">Ocupada</option>
              <option [ngValue]="2">Fuera de servicio</option>
            </select>
          </c-card-body>
        </c-card>
      </div>
      <div class="col-md-9">
        <c-card>
          <c-card-header>Mapa operativo</c-card-header>
          <c-card-body>
            <div #mapContainer style="height: 560px; border-radius: 0.25rem;"></div>
          </c-card-body>
        </c-card>
      </div>
    </div>
  `
})
export class MapaComponent implements AfterViewInit, OnDestroy {
  @ViewChild('mapContainer') mapContainer!: ElementRef<HTMLDivElement>;
  private map?: L.Map;

  readonly tipoUnidadLabel = TIPO_UNIDAD_LABEL;
  readonly tipoUnidadValores = Object.values(TipoUnidad).filter((v) => typeof v === 'number') as TipoUnidad[];

  tipos: TipoEmergencia[] = [];

  filtroTipoEmergenciaId: number | null = null;
  filtroPrioridad: Prioridad | null = null;
  filtroAmbito: Ambito | null = null;
  filtroTipoUnidad: TipoUnidad | null = null;
  filtroEstadoOperativo: EstadoOperativoUnidad | null = null;

  capas: EstadoCapas = {
    emergencias: true,
    unidades: true,
    bases: false,
    hospitales: false,
    puertos: false,
    rutas: false,
    historico: false
  };

  private capaEmergencias = L.layerGroup();
  private capaUnidades = L.layerGroup();
  private capaBases = L.layerGroup();
  private capaHospitales = L.layerGroup();
  private capaPuertos = L.layerGroup();
  private capaRutas = L.layerGroup();
  private capaHistorico = L.layerGroup();

  constructor(
    private readonly emergenciasService: EmergenciasService,
    private readonly unidadesService: UnidadesService,
    private readonly tiposEmergenciaService: TiposEmergenciaService,
    private readonly router: Router
  ) {}

  ngAfterViewInit(): void {
    // Los iconos por defecto de Leaflet se sirven desde /leaflet/ (ver angular.json assets),
    // en vez de depender de una CDN externa.
    L.Icon.Default.mergeOptions({
      iconRetinaUrl: 'leaflet/marker-icon-2x.png',
      iconUrl: 'leaflet/marker-icon.png',
      shadowUrl: 'leaflet/marker-shadow.png'
    });

    // Centro por defecto: Lima, Peru (ajustar segun la zona real de despliegue).
    this.map = L.map(this.mapContainer.nativeElement).setView([-9.5, -76.5], 6);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
      maxZoom: 19
    }).addTo(this.map);

    this.capaEmergencias.addTo(this.map);
    this.capaUnidades.addTo(this.map);
    this.aplicarVisibilidadCapas();
    this.cargarPuntosEstaticos();

    this.tiposEmergenciaService.listar(true).subscribe((data) => (this.tipos = data));

    this.recargarEmergencias();
    this.recargarUnidades();
  }

  onCapasChange(nuevas: EstadoCapas): void {
    this.capas = nuevas;
    this.aplicarVisibilidadCapas();
  }

  private aplicarVisibilidadCapas(): void {
    if (!this.map) return;

    const aplicar = (capa: L.LayerGroup, visible: boolean) => {
      if (visible) {
        if (!this.map!.hasLayer(capa)) capa.addTo(this.map!);
      } else if (this.map!.hasLayer(capa)) {
        this.map!.removeLayer(capa);
      }
    };

    aplicar(this.capaEmergencias, this.capas.emergencias);
    aplicar(this.capaUnidades, this.capas.unidades);
    aplicar(this.capaBases, this.capas.bases);
    aplicar(this.capaHospitales, this.capas.hospitales);
    aplicar(this.capaPuertos, this.capas.puertos);
    aplicar(this.capaRutas, this.capas.rutas);
    aplicar(this.capaHistorico, this.capas.historico);
  }

  recargarEmergencias(): void {
    const filtros: FiltrosEmergencias = {
      tipoEmergenciaId: this.filtroTipoEmergenciaId,
      prioridad: this.filtroPrioridad,
      ambito: this.filtroAmbito
    };

    this.emergenciasService.listar(filtros).subscribe((emergencias) => {
      this.capaEmergencias.clearLayers();
      this.capaHistorico.clearLayers();

      for (const e of emergencias) {
        const marker = L.marker([e.ubicacion.latitud, e.ubicacion.longitud], {
          icon: crearIconoMarcador(e.tipoEmergencia?.icono, e.tipoEmergencia?.color)
        });
        marker.bindPopup(this.crearPopupEmergencia(e));

        if (e.estado === EstadoEmergencia.Cerrada) {
          marker.addTo(this.capaHistorico);
        } else {
          marker.addTo(this.capaEmergencias);
        }
      }
    });
  }

  recargarUnidades(): void {
    const filtros: FiltrosUnidades = {
      tipo: this.filtroTipoUnidad,
      estadoOperativo: this.filtroEstadoOperativo
    };

    this.unidadesService.listar(filtros).subscribe((unidades) => {
      this.capaUnidades.clearLayers();

      for (const u of unidades) {
        if (u.latitud == null || u.longitud == null) continue;

        const icono = crearIconoMarcador(ICONOS_TIPO_UNIDAD[TipoUnidad[u.tipo]], '#321fdb');
        const marker = L.marker([u.latitud, u.longitud], { icon: icono });
        marker.bindPopup(this.crearPopupUnidad(u));
        marker.addTo(this.capaUnidades);
      }
    });
  }

  private crearPopupEmergencia(e: Emergencia): HTMLElement {
    const div = document.createElement('div');
    div.innerHTML = `
      <strong>${e.tipoEmergencia?.nombre ?? '(sin tipo)'}</strong><br>
      <span class="badge bg-secondary">${PRIORIDAD_LABEL[e.prioridad]}</span><br>
      ${e.descripcion}<br>
      <small>${e.ubicacion.distrito ?? ''} ${e.ubicacion.provincia ?? ''}</small><br>
    `;
    const boton = document.createElement('button');
    boton.className = 'btn btn-sm btn-primary mt-2';
    boton.textContent = 'Ver detalle completo';
    boton.addEventListener('click', () => {
      this.map?.closePopup();
      this.router.navigate(['/emergencias', e.id]);
    });
    div.appendChild(boton);
    return div;
  }

  private crearPopupUnidad(u: Unidad): HTMLElement {
    const div = document.createElement('div');
    div.innerHTML = `
      <strong>${u.identificador}</strong><br>
      ${this.tipoUnidadLabel[u.tipo]}<br>
    `;
    const boton = document.createElement('button');
    boton.className = 'btn btn-sm btn-primary mt-2';
    boton.textContent = 'Ver detalle completo';
    boton.addEventListener('click', () => {
      this.map?.closePopup();
      this.router.navigate(['/unidades', u.id]);
    });
    div.appendChild(boton);
    return div;
  }

  private cargarPuntosEstaticos(): void {
    for (const b of BASES_DEMO) {
      L.marker([b.lat, b.lon], { icon: crearIconoMarcador('cilBuilding', '#8a93a2') })
        .bindPopup(`<strong>${b.nombre}</strong>`)
        .addTo(this.capaBases);
    }
    for (const h of HOSPITALES_DEMO) {
      L.marker([h.lat, h.lon], { icon: crearIconoMarcador('cilHospital', '#e55353') })
        .bindPopup(`<strong>${h.nombre}</strong>`)
        .addTo(this.capaHospitales);
    }
    for (const p of PUERTOS_DEMO) {
      L.marker([p.lat, p.lon], { icon: crearIconoMarcador('cilBoatAlt', '#3399ff') })
        .bindPopup(`<strong>${p.nombre}</strong>`)
        .addTo(this.capaPuertos);
    }
    for (const ruta of RUTAS_DEMO) {
      L.polyline(ruta, { color: '#321fdb', dashArray: '6 6' }).addTo(this.capaRutas);
    }
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }
}
