import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

// FR-116: la administración de usuarios se restringe exclusivamente al rol Administrador.
export const administradorGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated() && auth.isAdministrador()) {
    return true;
  }

  return router.createUrlTree(['/dashboard']);
};
