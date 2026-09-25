import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import { MiUnidadService } from '../../core/services/mi-unidad.service';
import { EstadoOperativoUnidad, Unidad } from '../../core/models/unidad.model';
import { ESTADO_OPERATIVO_UNIDAD_LABEL, TIPO_UNIDAD_LABEL } from '../../core/models/labels';

// FR-120: pantalla minima para el rol "Unidad de respuesta" -- solo puede ver y
// actualizar el estado operativo de SU PROPIA unidad, nada mas.
@Component({
  selector: 'app-mi-unidad',
  standalone: true,
  imports: [FormsModule, CardComponent, CardHeaderComponent, CardBodyComponent],
  template: `
    <c-card style="max-width: 480px;">
      <c-card-header>Mi unidad</c-card-header>
      <c-card-body>
        @if (unidad(); as u) {
          <p><strong>Identificador:</strong> {{ u.identificador }}</p>
          <p><strong>Tipo:</strong> {{ tipoLabel[u.tipo] }}</p>
          <label class="form-label">Estado operativo</label>
          <select class="form-select" [ngModel]="u.estadoOperativo" (ngModelChange)="cambiarEstado($event)">
            <option [ngValue]="0">Disponible</option>
            <option [ngValue]="1">Ocupada</option>
            <option [ngValue]="2">Fuera de servicio</option>
          </select>
          @if (actualizado()) {
            <div class="alert alert-success mt-3 py-2">Estado actualizado.</div>
          }
        } @else {
          <p class="text-body-secondary">No tienes ninguna unidad asignada.</p>
        }
      </c-card-body>
    </c-card>
  `
})
export class MiUnidadComponent implements OnInit {
  readonly unidad = signal<Unidad | null>(null);
  readonly actualizado = signal(false);

  readonly tipoLabel = TIPO_UNIDAD_LABEL;
  readonly estadoLabel = ESTADO_OPERATIVO_UNIDAD_LABEL;

  constructor(private readonly miUnidadService: MiUnidadService) {}

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.miUnidadService.obtener().subscribe({
      next: (u) => this.unidad.set(u),
      error: () => this.unidad.set(null)
    });
  }

  cambiarEstado(nuevoEstado: EstadoOperativoUnidad): void {
    const u = this.unidad();
    if (!u) return;

    this.actualizado.set(false);
    this.miUnidadService.cambiarEstado(u.id, nuevoEstado).subscribe(() => {
      this.actualizado.set(true);
      this.cargar();
    });
  }
}
