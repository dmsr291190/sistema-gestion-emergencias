import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

// FR-120a: el rol Visualizador se limita a mapa + dashboard; no accede a
// emergencias, unidades, despacho ni administración de usuarios.
export const noVisualizadorGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isVisualizador()) {
    return router.createUrlTree(['/dashboard']);
  }

  return true;
};
