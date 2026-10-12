import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from '../../shared/services/api-client.service';

export interface RoleDto {
  role: string;
  permissions: string[];
}

export interface UserAccessDto {
  roles: string[];
  permissions: string[];
}

@Injectable({ providedIn: 'root' })
export class RoleService {
  private readonly api = inject(ApiClientService);

  getAll(): Observable<RoleDto[]> {
    return this.api.get<RoleDto[]>('Roles');
  }

  create(name: string): Observable<void> {
    return this.api.post<void, { name: string }>('Roles', { name });
  }

  rename(currentName: string, newName: string): Observable<void> {
    return this.api.put<void, { name: string }>(
      `Roles/${encodeURIComponent(currentName)}`,
      { name: newName }
    );
  }

  delete(roleName: string): Observable<void> {
    return this.api.delete<void>(`Roles/${encodeURIComponent(roleName)}`);
  }

  addPermission(roleName: string, permission: string): Observable<void> {
    return this.api.post<void, { permission: string }>(
      `Roles/${encodeURIComponent(roleName)}/permissions`,
      { permission }
    );
  }

  removePermission(roleName: string, permission: string): Observable<void> {
    return this.api.deleteWithBody<void, { permission: string }>(
      `Roles/${encodeURIComponent(roleName)}/permissions`,
      { permission }
    );
  }

  getUserAccess(userId: number): Observable<UserAccessDto> {
    return this.api.get<UserAccessDto>(`users/${userId}/permissions`);
  }

  assignRole(userId: number, roleName: string): Observable<void> {
    return this.api.post<void, { roleName: string }>(`users/${userId}/roles`, { roleName });
  }

  removeUserRole(userId: number, roleName: string): Observable<void> {
    return this.api.delete<void>(`users/${userId}/roles/${encodeURIComponent(roleName)}`);
  }
}
