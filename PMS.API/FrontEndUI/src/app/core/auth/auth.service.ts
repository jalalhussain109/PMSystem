import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { ApiClientService } from '../../shared/services/api-client.service';
import { AuthProfile, ChangePasswordRequest, LoginRequest, LoginResponse } from '../../shared/models/user.model';

const TOKEN_KEY = 'pms_access_token';
const ROLES_KEY = 'pms_user_roles';
const PERMISSIONS_KEY = 'pms_user_permissions';

export interface UserPermissions {
  userId: number;
  userName: string;
  roles: string[];
  permissions: string[];
}


@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiClientService);
  private readonly router = inject(Router);

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.api.post<LoginResponse, LoginRequest>('Auth/login', request).pipe(
      tap(response => {
        sessionStorage.setItem(TOKEN_KEY, response.token);
        const claims = this.decodeToken(response.token);
        this.storeAccessValues(
          this.claimValues(claims, ['role', 'roles']),
          this.claimValues(claims, ['permission', 'permissions'])
        );
      })
    );
  }

  getProfile(): Observable<AuthProfile> {
    return this.api.get<AuthProfile>('Auth/profile');
  }

  changePassword(request: ChangePasswordRequest): Observable<{ message: string }> {
    return this.api.post<{ message: string }, ChangePasswordRequest>('Auth/change-password', request);
  }

  getToken(): string | null {
    return sessionStorage.getItem(TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getClaimValues(claimName: string): string[] {
    const token = this.getToken();
    if (!token) return [];

    return this.claimValues(this.decodeToken(token), [claimName]);
  }

  getRoles(): string[] {
    return this.readAccessValues(ROLES_KEY, () => {
      const token = this.getToken();
      const claims = token ? this.decodeToken(token) : {};
      return this.claimValues(claims, [
        'role',
        'roles'
      ]);
    });
  }

  getPermissions(): string[] {
    return this.readAccessValues(PERMISSIONS_KEY, () => {
      const token = this.getToken();
      const claims = token ? this.decodeToken(token) : {};
      return this.claimValues(claims, ['permission', 'permissions']);
    });
  }

  hasAnyRole(roles: string[]): boolean {
    const userRoles = new Set(this.getRoles().map(role => role.toLocaleLowerCase()));
    return roles.some(role => userRoles.has(role.toLocaleLowerCase()));
  }

  hasAllPermissions(permissions: string[]): boolean {
    const userPermissions = new Set(this.getPermissions().map(permission => permission.toLocaleLowerCase()));
    return permissions.every(p=> userPermissions.has(p.toLocaleLowerCase()));
  }

  logout(): void {
    sessionStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(ROLES_KEY);
    localStorage.removeItem(PERMISSIONS_KEY);
    void this.router.navigate(['/login']);
  }

  private decodeToken(token: string): Record<string, unknown> {
    try {
      const payload = token.split('.')[1];
      if (!payload) return {};
      const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
      const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, '=');
      return JSON.parse(atob(padded)) as Record<string, unknown>;
    } catch {
      return {};
    }
  }

  private claimValues(claims: Record<string, unknown>, claimNames: string[]): string[] {
    const matchingNames = new Set(claimNames.map(name => name.toLocaleLowerCase()));
    return Object.entries(claims)
      .filter(([name]) => matchingNames.has(name.toLocaleLowerCase()))
      .flatMap(([, value]) => Array.isArray(value) ? value.map(String) : value == null ? [] : [String(value)])
      .filter((value, index, values) => values.findIndex(item => item.toLocaleLowerCase() === value.toLocaleLowerCase()) === index);
  }

  private storeAccessValues(roles: string[], permissions: string[]): void {
    localStorage.setItem(ROLES_KEY, JSON.stringify(roles));
    localStorage.setItem(PERMISSIONS_KEY, JSON.stringify(permissions));
  }

 isAdmin(): boolean {
  const roles = localStorage.getItem(ROLES_KEY);
  if (!roles) return false;
  const userRoles: string[] = JSON.parse(roles);
  return userRoles.some(role => role.toLowerCase() === 'admin');
}

  private readAccessValues(key: string, fallback: () => string[]): string[] {
    const storedValue = localStorage.getItem(key);
    if (storedValue) {
      try {
        const parsed: unknown = JSON.parse(storedValue);
        if (Array.isArray(parsed) && parsed.every(value => typeof value === 'string')) return parsed;
      } catch {
        localStorage.removeItem(key);
      }
    }

    const values = fallback();
    localStorage.setItem(key, JSON.stringify(values));
    return values;
  }
}
