import { DatePipe, NgClass } from '@angular/common';
import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import * as L from 'leaflet';
import { PublicoService } from '../../core/services/publico.service';
import { ThemeService } from '../../core/services/theme.service';
import { crearIconoMarcador } from '../../core/utils/mapa-icons';
import { EmergenciaPublica, EmergenciaPublicaDetalle } from '../../core/models/emergencia-publica.model';
import { PRIORIDAD_LABEL, ESTADO_EMERGENCIA_LABEL, ESTADO_EMERGENCIA_COLOR } from '../../core/models/labels';

// US7 (Ampliación Operativa Nacional): vista pública sin autenticación (FR-113).
// Fuera del layout autenticado (sin sidebar/menú de administración) y sin
// authGuard en la ruta (app.routes.ts). FR-114/FR-115: el backend ya redacta
// los datos sensibles y la ubicación exacta; esta vista solo consume
// PublicoService, nunca EmergenciasService (el DTO interno).
@Component({
  selector: 'app-publico',
  standalone: true,
  imports: [FormsModule, NgClass, DatePipe],
  template: `
    <div class="sige-publico">
      <header class="sige-publico-header d-flex justify-content-between align-items-start">
        <div>
          <h1>SIGE — Monitoreo Nacional de Emergencias</h1>
          <p class="text-body-secondary">
            Vista pública, sin necesidad de iniciar sesión. Última actualización: {{ ahora | date: 'medium' }}.
          </p>
        </div>
        <button type="button" class="btn btn-sm btn-outline-secondary" (click)="theme.toggle()">
          {{ theme.isDark() ? 'Tema claro' : 'Tema oscuro' }}
        </button>
      </header>

      <div class="row g-3 px-3">
        <div class="col-md-3">
          <label class="form-label small">Tipo</label>
          <select class="form-select form-select-sm mb-2" [(ngModel)]="filtroTipo" (ngModelChange)="aplicarFiltros()">
            <option [ngValue]="null">Todos</option>
            @for (t of tiposDisponibles(); track t) {
              <option [ngValue]="t">{{ t }}</option>
            }
          </select>

          <label class="form-label small">Prioridad</label>
          <select class="form-select form-select-sm mb-3" [(ngModel)]="filtroPrioridad" (ngModelChange)="aplicarFiltros()">
            <option [ngValue]="null">Todas</option>
            <option [ngValue]="0">Baja</option>
            <option [ngValue]="1">Media</option>
            <option [ngValue]="2">Alta</option>
            <option [ngValue]="3">Crítica</option>
          </select>

          <div class="small text-body-secondary">
            <p><strong>Leyenda</strong></p>
            <p>🔴 Crítica &nbsp; 🟠 Alta &nbsp; 🔵 Media &nbsp; ⚪ Baja</p>
          </div>

          @if (seleccion(); as sel) {
            <div class="card mt-3">
              <div class="card-body">
                <h6>Emergencia #{{ sel.codigo }}</h6>
                <p>{{ sel.tipoNombre }} — <span [ngClass]="'text-' + estadoColor[sel.estado]">{{ estadoLabel[sel.estado] }}</span></p>
                <p class="small text-body-secondary">{{ sel.ubicacion.distrito ?? 'Ubicación aproximada' }}, {{ sel.ubicacion.provincia }}</p>
                <h6 class="mt-3">Línea de tiempo</h6>
                <ul class="list-group list-group-flush small">
                  @for (ev of sel.timeline; track $index) {
                    <li class="list-group-item px-0">{{ ev.tipoEvento }} <span class="text-body-secondary d-block">{{ ev.fechaHora | date: 'short' }}</span></li>
                  }
                </ul>
              </div>
            </div>
          }
        </div>

        <div class="col-md-9">
          <div #mapContainer style="height: 480px; border-radius: 0.25rem;"></div>

          <table class="table table-sm mt-3">
            <thead><tr><th>Código</th><th>Tipo</th><th>Prioridad</th><th>Estado</th><th>Ubicación aprox.</th><th></th></tr></thead>
            <tbody>
              @for (e of emergenciasFiltradas(); track e.codigo) {
                <tr>
                  <td>{{ e.codigo }}</td>
                  <td>{{ e.tipoNombre }}</td>
                  <td>{{ prioridadLabel[e.prioridad] }}</td>
                  <td [ngClass]="'text-' + estadoColor[e.estado]">{{ estadoLabel[e.estado] }}</td>
                  <td>{{ e.ubicacion.distrito ?? '—' }}</td>
                  <td><button class="btn btn-sm btn-outline-primary" (click)="verResumen(e.codigo)">Ver resumen</button></td>
                </tr>
              } @empty {
                <tr><td colspan="6" class="text-body-secondary">No hay emergencias activas en este momento.</td></tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .sige-publico { max-width: 1200px; margin: 0 auto; padding: 1.5rem; }
    .sige-publico-header { padding: 0 1rem 1rem; }
  `]
})
export class PublicoComponent implements AfterViewInit, OnDestroy {
  @ViewChild('mapContainer') mapContainer!: ElementRef<HTMLDivElement>;
  private map?: L.Map;
  private capaMarcadores = L.layerGroup();

  readonly ahora = new Date();
  readonly emergencias = signal<EmergenciaPublica[]>([]);
  readonly emergenciasFiltradas = signal<EmergenciaPublica[]>([]);
  readonly seleccion = signal<EmergenciaPublicaDetalle | null>(null);

  readonly prioridadLabel = PRIORIDAD_LABEL;
  readonly estadoLabel = ESTADO_EMERGENCIA_LABEL;
  readonly estadoColor = ESTADO_EMERGENCIA_COLOR;

  filtroTipo: string | null = null;
  filtroPrioridad: number | null = null;

  constructor(
    private readonly publicoService: PublicoService,
    readonly theme: ThemeService
  ) {}

  ngAfterViewInit(): void {
    L.Icon.Default.mergeOptions({
      iconRetinaUrl: 'leaflet/marker-icon-2x.png',
      iconUrl: 'leaflet/marker-icon.png',
      shadowUrl: 'leaflet/marker-shadow.png'
    });

    this.map = L.map(this.mapContainer.nativeElement).setView([-9.5, -76.5], 6);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
      maxZoom: 19
    }).addTo(this.map);
    this.capaMarcadores.addTo(this.map);

    this.cargar();
  }

  tiposDisponibles(): string[] {
    return [...new Set(this.emergencias().map((e) => e.tipoNombre).filter((t): t is string => !!t))];
  }

  private cargar(): void {
    this.publicoService.listarEmergencias().subscribe((data) => {
      this.emergencias.set(data);
      this.aplicarFiltros();
    });
  }

  aplicarFiltros(): void {
    const filtradas = this.emergencias().filter(
      (e) =>
        (this.filtroTipo == null || e.tipoNombre === this.filtroTipo) &&
        (this.filtroPrioridad == null || e.prioridad === this.filtroPrioridad)
    );
    this.emergenciasFiltradas.set(filtradas);
    this.pintarMarcadores(filtradas);
  }

  private pintarMarcadores(emergencias: EmergenciaPublica[]): void {
    this.capaMarcadores.clearLayers();
    for (const e of emergencias) {
      const marker = L.marker([e.ubicacion.latitudAproximada, e.ubicacion.longitudAproximada], {
        icon: crearIconoMarcador(e.tipoIcono, e.tipoColor)
      });
      marker.on('click', () => this.verResumen(e.codigo));
      marker.bindPopup(`<strong>${e.tipoNombre ?? '(sin tipo)'}</strong><br>${e.ubicacion.distrito ?? ''}`);
      marker.addTo(this.capaMarcadores);
    }
  }

  verResumen(codigo: number): void {
    this.publicoService.obtenerEmergencia(codigo).subscribe((data) => this.seleccion.set(data));
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }
}
