import { DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import { EmergenciasService } from '../../../core/services/emergencias.service';
import { EmergenciaDetalle, EstadoEmergencia } from '../../../core/models/emergencia.model';

// US1 (detalle), US4 (timeline, FR-008).
@Component({
  selector: 'app-emergencia-detalle',
  standalone: true,
  imports: [DatePipe, RouterLink, CardComponent, CardHeaderComponent, CardBodyComponent],
  template: `
    @if (emergencia(); as e) {
      <c-card>
        <c-card-header class="d-flex justify-content-between align-items-center">
          <span>Emergencia #{{ e.id }} — {{ e.tipo }}</span>
          @if (e.estado === EstadoEmergencia.Reportada) {
            <button class="btn btn-sm btn-success" (click)="validar()" [disabled]="validando()">
              {{ validando() ? 'Validando...' : 'Validar' }}
            </button>
          }
        </c-card-header>
        <c-card-body>
          <p>{{ e.descripcion }}</p>
          <p><strong>Reportante:</strong> {{ e.reportanteNombre }} @if (e.reportanteContacto) { ({{ e.reportanteContacto }}) }</p>
          <p><strong>Ubicacion:</strong> {{ e.latitud }}, {{ e.longitud }}</p>
          <p><strong>Estado actual:</strong> {{ estadoLabel[e.estado] }}</p>

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
  readonly validando = signal(false);
  readonly EstadoEmergencia = EstadoEmergencia;

  readonly estadoLabel = {
    0: 'Reportada', 1: 'Validada', 2: 'Despachada', 3: 'En ruta', 4: 'En el lugar', 5: 'Atendida', 6: 'Cerrada'
  };

  constructor(
    private readonly route: ActivatedRoute,
    private readonly emergenciasService: EmergenciasService
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  private cargar(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.emergenciasService.obtener(id).subscribe((data) => this.emergencia.set(data));
  }

  validar(): void {
    const id = this.emergencia()?.id;
    if (!id) return;

    this.validando.set(true);
    this.emergenciasService.validar(id).subscribe({
      next: () => {
        this.validando.set(false);
        this.cargar();
      },
      error: () => this.validando.set(false)
    });
  }
}
