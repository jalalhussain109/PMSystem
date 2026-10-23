import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const permissionGuard: CanActivateFn = route => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const permissions = (route.data['permissions'] as string[] | undefined) ?? [];
  let roles = auth.getRoles(); 
  if (roles.some(role => role.toLowerCase() === 'admin')) {
    return true;
  }
  const permissionAllowed = permissions.length === 0 || auth.hasAllPermissions(permissions);
  roles = (route.data['roles'] as string[] | undefined) ?? [];
  const roleAllowed = roles.length === 0 || auth.hasAnyRole(roles);
  return permissionAllowed && roleAllowed ? true : router.createUrlTree(['/dashboard']);
};
