import { Component, Input } from '@angular/core';

// FR-025: componente unico de campo de formulario (etiqueta + control proyectado +
// mensaje de error), para que login, nueva emergencia, alta de unidad y despacho no
// repitan cada uno su propio marcado de label/control/error.
@Component({
  selector: 'app-form-field',
  standalone: true,
  template: `
    <div class="mb-3">
      <label class="form-label">{{ label }}</label>
      <ng-content></ng-content>
      @if (error) {
        <div class="invalid-feedback d-block">{{ error }}</div>
      }
    </div>
  `
})
export class FormFieldComponent {
  @Input() label = '';
  @Input() error: string | null | undefined = null;
}
