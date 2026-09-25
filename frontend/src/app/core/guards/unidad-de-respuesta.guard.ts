import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

// FR-120: la pantalla "mi unidad" es exclusiva del rol UnidadDeRespuesta.
export const unidadDeRespuestaGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated() && auth.isUnidadDeRespuesta()) {
    return true;
  }

  return router.createUrlTree(['/dashboard']);
};
