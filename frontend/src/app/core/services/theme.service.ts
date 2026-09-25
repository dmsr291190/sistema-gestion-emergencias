import { Injectable, signal } from '@angular/core';

const THEME_STORAGE_KEY = 'sige_theme';

// FR-128: tema oscuro consistente en toda la aplicación (login, mapa,
// formularios, tablas, modales, dashboard, vista pública, administración de
// usuarios). Se usa el mecanismo nativo de CoreUI (atributo
// data-coreui-theme="dark" en <html>), no un sistema de theming propio
// (research.md §7) -- CoreUI/Bootstrap 5.3+ ya adaptan sus propias clases
// utilitarias (text-success, text-warning, etc.) a modo oscuro manteniendo
// contraste, incluidas las usadas en core/models/labels.ts.
@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly isDark = signal(this.leerPreferenciaGuardada());

  constructor() {
    this.aplicar(this.isDark());
  }

  toggle(): void {
    this.isDark.set(!this.isDark());
    this.aplicar(this.isDark());
  }

  private leerPreferenciaGuardada(): boolean {
    return localStorage.getItem(THEME_STORAGE_KEY) === 'dark';
  }

  private aplicar(oscuro: boolean): void {
    document.documentElement.setAttribute('data-coreui-theme', oscuro ? 'dark' : 'light');
    localStorage.setItem(THEME_STORAGE_KEY, oscuro ? 'dark' : 'light');
  }
}
