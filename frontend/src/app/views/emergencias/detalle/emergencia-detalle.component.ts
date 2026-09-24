import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import { Observable } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { EmergenciasService } from '../../../core/services/emergencias.service';
import { EmergenciaDetalle, EstadoAsignacion, EstadoEmergencia } from '../../../core/models/emergencia.model';

// US1 (detalle), US4 (avance de estado, cierre/reapertura, timeline — FR-008, FR-016, FR-017, FR-018).
@Component({
  selector: 'app-emergencia-detalle',
  standalone: true,
  imports: [DatePipe, RouterLink, CardComponent, CardHeaderComponent, CardBodyComponent],
  template: `
    @if (emergencia(); as e) {
      <c-card>
        <c-card-header class="d-flex justify-content-between align-items-center">
          <span>Emergencia #{{ e.id }} — {{ e.tipo }}</span>
          <div class="d-flex gap-2">
            @if (e.estado === EstadoEmergencia.Reportada) {
              <button class="btn btn-sm btn-success" (click)="validar()" [disabled]="procesando()">
                {{ procesando() ? 'Validando...' : 'Validar' }}
              </button>
            }
            @if (e.estado === EstadoEmergencia.Atendida && auth.isSupervisor()) {
              <button class="btn btn-sm btn-primary" (click)="cerrar()" [disabled]="procesando()">
                {{ procesando() ? 'Cerrando...' : 'Cerrar emergencia' }}
              </button>
            }
            @if (e.estado === EstadoEmergencia.Cerrada && auth.isSupervisor()) {
              <button class="btn btn-sm btn-warning" (click)="reabrir()" [disabled]="procesando()">
                {{ procesando() ? 'Reabriendo...' : 'Reabrir' }}
              </button>
            }
          </div>
        </c-card-header>
        <c-card-body>
          <p>{{ e.descripcion }}</p>
          <p><strong>Reportante:</strong> {{ e.reportanteNombre }} @if (e.reportanteContacto) { ({{ e.reportanteContacto }}) }</p>
          <p><strong>Ubicacion:</strong> {{ e.latitud }}, {{ e.longitud }}</p>
          <p><strong>Estado actual:</strong> {{ estadoLabel[e.estado] }}</p>

          @if (error()) {
            <div class="alert alert-danger py-2">{{ error() }}</div>
          }

          <h6 class="mt-4">Unidades asignadas</h6>
          @if (e.asignaciones.length === 0) {
            <p class="text-body-secondary">Sin unidades asignadas todavia. Ve a "Despacho" para asignar una.</p>
          } @else {
            <ul class="list-group mb-3">
              @for (a of e.asignaciones; track a.id) {
                <li class="list-group-item d-flex justify-content-between align-items-center">
                  <span>{{ a.unidadIdentificador }} — {{ asignacionLabel[a.estadoAsignacion] }}</span>
                  @if (a.estadoAsignacion !== EstadoAsignacion.Atendida) {
                    <button class="btn btn-sm btn-outline-primary" (click)="avanzarAsignacion(a.id, a.estadoAsignacion)" [disabled]="procesando()">
                      Avanzar a "{{ siguienteEstadoLabel(a.estadoAsignacion) }}"
                    </button>
                  }
                </li>
              }
            </ul>
          }

          <h6 class="mt-4">Linea de tiempo</h6>
          <ul class="list-group">
            @for (ev of e.timeline; track ev.id) {
              <li class="list-group-item">
                <strong>{{ ev.tipoEvento }}</strong>
                @if (ev.estadoAnterior) { — {{ ev.estadoAnterior }} → {{ ev.estadoNuevo }} }
                <span class="text-body-secondary d-block small">{{ ev.fechaHora | date: 'medium' }}</span>
              </li>
            }
          </ul>

          <a class="btn btn-link mt-3 ps-0" routerLink="/emergencias">&larr; Volver al listado</a>
        </c-card-body>
      </c-card>
    }
  `
})
export class EmergenciaDetalleComponent implements OnInit {
  readonly emergencia = signal<EmergenciaDetalle | null>(null);
  readonly procesando = signal(false);
  readonly error = signal<string | null>(null);
  readonly EstadoEmergencia = EstadoEmergencia;
  readonly EstadoAsignacion = EstadoAsignacion;

  readonly estadoLabel = {
    0: 'Reportada', 1: 'Validada', 2: 'Despachada', 3: 'En ruta', 4: 'En el lugar', 5: 'Atendida', 6: 'Cerrada'
  };
  readonly asignacionLabel = { 0: 'Despachada', 1: 'En ruta', 2: 'En el lugar', 3: 'Atendida' };

  constructor(
    private readonly route: ActivatedRoute,
    private readonly emergenciasService: EmergenciasService,
    readonly auth: AuthService
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  private cargar(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.emergenciasService.obtener(id).subscribe((data) => this.emergencia.set(data));
  }

  siguienteEstadoLabel(actual: EstadoAsignacion): string {
    return this.asignacionLabel[(actual + 1) as EstadoAsignacion];
  }

  private ejecutar(accion: () => Observable<unknown>): void {
    this.error.set(null);
    this.procesando.set(true);
    accion().subscribe({
      next: () => {
        this.procesando.set(false);
        this.cargar();
      },
      error: (err: HttpErrorResponse) => {
        this.procesando.set(false);
        this.error.set(err.error?.mensaje ?? 'No se pudo completar la accion.');
      }
    });
  }

  validar(): void {
    const id = this.emergencia()?.id;
    if (id) this.ejecutar(() => this.emergenciasService.validar(id));
  }

  cerrar(): void {
    const id = this.emergencia()?.id;
    if (id) this.ejecutar(() => this.emergenciasService.cerrar(id));
  }

  reabrir(): void {
    const id = this.emergencia()?.id;
    if (id) this.ejecutar(() => this.emergenciasService.reabrir(id));
  }

  avanzarAsignacion(asignacionId: number, estadoActual: EstadoAsignacion): void {
    const siguiente = (estadoActual + 1) as EstadoAsignacion;
    this.ejecutar(() => this.emergenciasService.cambiarEstadoAsignacion(asignacionId, siguiente));
  }
}
