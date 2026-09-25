import { NgClass } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';
import { AuthService } from '../../../core/services/auth.service';
import { UnidadesService } from '../../../core/services/unidades.service';
import { PersonalService } from '../../../core/services/personal.service';
import { RecursosService } from '../../../core/services/recursos.service';
import { FormFieldComponent } from '../../../shared/form-field/form-field.component';
import { Unidad } from '../../../core/models/unidad.model';
import { CrearPersonalRequest, Personal } from '../../../core/models/personal.model';
import { CategoriaRecurso, CrearRecursoRequest, Recurso } from '../../../core/models/recurso.model';
import { ESTADO_OPERATIVO_UNIDAD_LABEL, TIPO_UNIDAD_LABEL } from '../../../core/models/labels';

// US5: personal y recursos de una unidad, visibles al seleccionarla desde
// despacho o el mapa (FR-110, FR-111, FR-112). Alta restringida a
// CoordinadorLogistico/JefeDeUnidad (el backend ya lo exige; aquí solo se
// oculta el formulario para los demás roles).
@Component({
  selector: 'app-unidad-detalle',
  standalone: true,
  imports: [NgClass, FormsModule, RouterLink, CardComponent, CardHeaderComponent, CardBodyComponent, FormFieldComponent],
  template: `
    @if (unidad(); as u) {
      <c-card class="mb-4">
        <c-card-header>Unidad {{ u.identificador }}</c-card-header>
        <c-card-body>
          <p><strong>Tipo:</strong> {{ tipoLabel[u.tipo] }}</p>
          <p><strong>Estado operativo:</strong> {{ estadoLabel[u.estadoOperativo] }}</p>
          <a class="btn btn-link ps-0" routerLink="/unidades">&larr; Volver al listado</a>
        </c-card-body>
      </c-card>

      <div class="row g-4">
        <div class="col-md-6">
          <c-card>
            <c-card-header>Personal</c-card-header>
            <c-card-body>
              <table class="table table-sm">
                <thead><tr><th>Nombre</th><th>Especialidad</th><th>Disponible</th></tr></thead>
                <tbody>
                  @for (p of personal(); track p.id) {
                    <tr>
                      <td>{{ p.nombres }} {{ p.apellidos }}</td>
                      <td>{{ p.especialidad }}</td>
                      <td>{{ p.disponible ? 'Sí' : 'No' }}</td>
                    </tr>
                  } @empty {
                    <tr><td colspan="3" class="text-body-secondary">Sin personal asignado.</td></tr>
                  }
                </tbody>
              </table>

              @if (puedeGestionar()) {
                <hr />
                <form (ngSubmit)="crearPersonal()">
                  <app-form-field label="Nombres"><input class="form-control" name="nombres" [(ngModel)]="formPersonal.nombres" required /></app-form-field>
                  <app-form-field label="Apellidos"><input class="form-control" name="apellidos" [(ngModel)]="formPersonal.apellidos" required /></app-form-field>
                  <app-form-field label="Documento"><input class="form-control" name="documento" [(ngModel)]="formPersonal.documento" required /></app-form-field>
                  <app-form-field label="Especialidad"><input class="form-control" name="especialidad" [(ngModel)]="formPersonal.especialidad" /></app-form-field>
                  <app-form-field label="Función"><input class="form-control" name="funcion" [(ngModel)]="formPersonal.funcion" /></app-form-field>
                  <button class="btn btn-sm btn-primary" type="submit">Agregar personal</button>
                </form>
              }
            </c-card-body>
          </c-card>
        </div>

        <div class="col-md-6">
          <c-card>
            <c-card-header>Recursos</c-card-header>
            <c-card-body>
              <table class="table table-sm">
                <thead><tr><th>Recurso</th><th>Cantidad</th><th>Disponible</th></tr></thead>
                <tbody>
                  @for (r of recursos(); track r.id) {
                    <tr [ngClass]="r.bajoStock ? 'table-danger' : ''">
                      <td>{{ r.nombre }} <span class="text-body-secondary small">({{ r.codigo }})</span></td>
                      <td>{{ r.cantidad }} {{ r.unidadMedida }}</td>
                      <td>
                        @if (puedeGestionar()) {
                          <input class="form-control form-control-sm" style="width:80px" type="number" min="0"
                                 [ngModel]="r.cantidadDisponible" (ngModelChange)="actualizarCantidad(r, $event)" />
                        } @else {
                          {{ r.cantidadDisponible }}
                        }
                        @if (r.bajoStock) { <span class="badge bg-danger ms-1">Bajo stock</span> }
                      </td>
                    </tr>
                  } @empty {
                    <tr><td colspan="3" class="text-body-secondary">Sin recursos registrados.</td></tr>
                  }
                </tbody>
              </table>

              @if (puedeGestionar()) {
                <hr />
                <form (ngSubmit)="crearRecurso()">
                  <app-form-field label="Código"><input class="form-control" name="codigo" [(ngModel)]="formRecurso.codigo" required /></app-form-field>
                  <app-form-field label="Nombre"><input class="form-control" name="nombreRecurso" [(ngModel)]="formRecurso.nombre" required /></app-form-field>
                  <app-form-field label="Categoría">
                    <select class="form-select" name="categoria" [(ngModel)]="formRecurso.categoria">
                      <option [ngValue]="0">Equipos</option>
                      <option [ngValue]="1">Herramientas</option>
                      <option [ngValue]="2">Víveres</option>
                      <option [ngValue]="3">Líquidos</option>
                      <option [ngValue]="4">Estructuras</option>
                      <option [ngValue]="5">Material médico</option>
                      <option [ngValue]="6">Equipo de rescate</option>
                    </select>
                  </app-form-field>
                  <div class="row">
                    <div class="col-4"><app-form-field label="Cantidad"><input class="form-control" type="number" min="0" name="cantidad" [(ngModel)]="formRecurso.cantidad" /></app-form-field></div>
                    <div class="col-4"><app-form-field label="Disponible"><input class="form-control" type="number" min="0" name="disponible" [(ngModel)]="formRecurso.cantidadDisponible" /></app-form-field></div>
                    <div class="col-4"><app-form-field label="Mínima"><input class="form-control" type="number" min="0" name="minima" [(ngModel)]="formRecurso.cantidadMinima" /></app-form-field></div>
                  </div>
                  @if (errorRecurso()) {
                    <div class="alert alert-danger py-2">{{ errorRecurso() }}</div>
                  }
                  <button class="btn btn-sm btn-primary" type="submit">Agregar recurso</button>
                </form>
              }
            </c-card-body>
          </c-card>
        </div>
      </div>
    }
  `
})
export class UnidadDetalleComponent implements OnInit {
  readonly unidad = signal<Unidad | null>(null);
  readonly personal = signal<Personal[]>([]);
  readonly recursos = signal<Recurso[]>([]);
  readonly errorRecurso = signal<string | null>(null);

  readonly tipoLabel = TIPO_UNIDAD_LABEL;
  readonly estadoLabel = ESTADO_OPERATIVO_UNIDAD_LABEL;

  private unidadId = 0;

  formPersonal: CrearPersonalRequest = { nombres: '', apellidos: '', documento: '', institucionId: null, especialidad: '', funcion: '', disponible: true };
  formRecurso: CrearRecursoRequest = { codigo: '', nombre: '', categoria: CategoriaRecurso.Equipos, cantidad: 0, cantidadDisponible: 0, cantidadMinima: 0 };

  constructor(
    private readonly route: ActivatedRoute,
    private readonly unidadesService: UnidadesService,
    private readonly personalService: PersonalService,
    private readonly recursosService: RecursosService,
    readonly auth: AuthService
  ) {}

  ngOnInit(): void {
    this.unidadId = Number(this.route.snapshot.paramMap.get('id'));
    this.unidadesService.obtener(this.unidadId).subscribe((u) => this.unidad.set(u));
    this.cargarPersonal();
    this.cargarRecursos();
  }

  puedeGestionar(): boolean {
    return this.auth.isCoordinadorLogistico() || this.auth.isJefeDeUnidad();
  }

  private cargarPersonal(): void {
    this.personalService.listarPorUnidad(this.unidadId).subscribe((data) => this.personal.set(data));
  }

  private cargarRecursos(): void {
    this.recursosService.listarPorUnidad(this.unidadId).subscribe((data) => this.recursos.set(data));
  }

  crearPersonal(): void {
    this.personalService.crear(this.unidadId, this.formPersonal).subscribe(() => {
      this.formPersonal = { nombres: '', apellidos: '', documento: '', institucionId: null, especialidad: '', funcion: '', disponible: true };
      this.cargarPersonal();
    });
  }

  crearRecurso(): void {
    this.errorRecurso.set(null);
    this.recursosService.crear(this.unidadId, this.formRecurso).subscribe({
      next: () => {
        this.formRecurso = { codigo: '', nombre: '', categoria: CategoriaRecurso.Equipos, cantidad: 0, cantidadDisponible: 0, cantidadMinima: 0 };
        this.cargarRecursos();
      },
      error: () => this.errorRecurso.set('No se pudo crear el recurso (revisa que la cantidad disponible no supere la cantidad total).')
    });
  }

  actualizarCantidad(recurso: Recurso, nuevaCantidad: number): void {
    this.recursosService.actualizarCantidad(this.unidadId, recurso.id, nuevaCantidad).subscribe(() => this.cargarRecursos());
  }
}
