import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { IconSetService } from '@coreui/icons-angular';

import { iconSubset } from './icons/icon-subset';
import { ThemeService } from './core/services/theme.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('frontend');

  constructor() {
    inject(IconSetService).icons = { ...iconSubset };
    // FR-128: se inyecta aquí (raíz de la app) para que el tema persistido se
    // aplique a CUALQUIER pantalla (login, vista pública, layout autenticado),
    // no solo a las que están dentro de DefaultLayoutComponent.
    inject(ThemeService);
  }
}
