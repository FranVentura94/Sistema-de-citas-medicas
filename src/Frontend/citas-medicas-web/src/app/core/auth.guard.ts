import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { Rol } from './config';

// Solo deja entrar si hay sesión iniciada
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  if (auth.estaAutenticado()) return true;
  return inject(Router).createUrlTree(['/login'], { queryParams: { volver: state.url } });
};

// Solo deja entrar si el usuario tiene alguno de los roles de la ruta (data: { roles: [...] })
export const rolGuard: CanActivateFn = route => {
  const roles = (route.data['roles'] ?? []) as Rol[];
  if (roles.length === 0 || inject(AuthService).tieneRol(...roles)) return true;
  return inject(Router).createUrlTree(['/dashboard']);
};

// Si ya inició sesión, no tiene sentido ver el login
export const invitadoGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.estaAutenticado() ? inject(Router).createUrlTree(['/dashboard']) : true;
};
