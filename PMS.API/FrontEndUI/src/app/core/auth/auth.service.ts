import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { ApiClientService } from '../../shared/services/api-client.service';
import { AuthProfile, ChangePasswordRequest, LoginRequest, LoginResponse } from '../../shared/models/user.model';

const TOKEN_KEY = 'pms_access_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiClientService);
  private readonly router = inject(Router);

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.api.post<LoginResponse, LoginRequest>('Auth/login', request).pipe(
      tap(response => sessionStorage.setItem(TOKEN_KEY, response.token))
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

    try {
      const payload = token.split('.')[1];
      const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
      const decoded = JSON.parse(atob(normalized)) as Record<string, unknown>;
      const claim = decoded[claimName];
      if (Array.isArray(claim)) return claim.map(String);
      return claim == null ? [] : [String(claim)];
    } catch {
      return [];
    }
  }

  hasAnyRole(roles: string[]): boolean {
    const tokenRoles = this.getClaimValues('role');
    return roles.some(role => tokenRoles.includes(role));
  }

  hasAllPermissions(permissions: string[]): boolean {
    const tokenPermissions = this.getClaimValues('permission');
    return permissions.every(permission => tokenPermissions.includes(permission));
  }

  logout(): void {
    sessionStorage.removeItem(TOKEN_KEY);
    void this.router.navigate(['/login']);
  }
}
