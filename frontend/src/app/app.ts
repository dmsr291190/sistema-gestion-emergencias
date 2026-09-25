import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { IconSetService } from '@coreui/icons-angular';

import { iconSubset } from './icons/icon-subset';

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
  }
}
