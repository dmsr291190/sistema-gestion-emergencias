import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    // Requerido por CollapseDirective de @coreui/angular (usada en la leyenda
    // del mapa, US4): inyecta AnimationBuilder internamente; sin este
    // provider, Angular lanza NullInjectorError al renderizarla y toda la
    // pantalla del mapa queda en blanco.
    provideAnimations()
  ]
};
