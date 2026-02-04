import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const redirectAuthGuard: CanActivateFn = () => {
  const router = inject(Router);
  const token = localStorage.getItem('auth_token');

  if (token) {
    return router.createUrlTree(['/users']);
  }

  return true;
};
