import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { RoleDto, RoleService } from './role.service';

const AVAILABLE_PERMISSIONS = [
  'Projects.View',
  'Projects.Create',
  'Projects.Edit',
  'Projects.Delete',
  'Tasks.View',
  'Tasks.Create',
  'Tasks.Edit',
  'Tasks.Delete',
  'Users.View',
  'Users.Create',
  'Users.Edit',
  'Users.Delete'
];

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './roles.component.html',
  styleUrl: './roles.component.css'
})
export class RolesComponent {
  private readonly roleService = inject(RoleService);

  readonly roles = signal<RoleDto[]>([]);
  readonly selectedPermission = signal('');
  readonly activeRole = signal('');
  readonly loadingRoles = signal(false);
  readonly saving = signal(false);
  readonly rolesError = signal('');
  readonly actionError = signal('');
  readonly actionMessage = signal('');
  readonly availablePermissions = AVAILABLE_PERMISSIONS;
  readonly editingRoleName = signal('');
  editRoleValue = '';
  newRoleName = '';

  constructor() {
    this.loadRoles();
  }

  loadRoles(): void {
    this.loadingRoles.set(true);
    this.rolesError.set('');
    this.roleService.getAll().pipe(finalize(() => this.loadingRoles.set(false))).subscribe({
      next: roles => this.roles.set(roles),
      error: (err: HttpErrorResponse) => this.rolesError.set(this.errorMessage(err, 'Could not load roles.'))
    });
  }

  createRole(): void {
    const name = this.newRoleName.trim();
    if (!name || this.saving()) return;

    this.runAction(
      this.roleService.create(name),
      'Role created.',
      () => {
        this.newRoleName = '';
        this.loadRoles();
      },
      'Could not create the role.'
    );
  }

  startEditRole(roleName: string): void {
    this.actionError.set('');
    this.actionMessage.set('');
    this.editingRoleName.set(roleName);
    this.editRoleValue = roleName;
  }

  cancelEditRole(): void {
    this.editingRoleName.set('');
    this.editRoleValue = '';
  }

  saveRoleName(): void {
    const currentName = this.editingRoleName();
    const newName = this.editRoleValue.trim();
    if (!currentName || !newName || newName === currentName || this.saving()) return;

    this.runAction(
      this.roleService.rename(currentName, newName),
      `Role renamed to “${newName}”.`,
      () => {
        this.editingRoleName.set('');
        this.editRoleValue = '';
        if (this.activeRole() === currentName) this.activeRole.set(newName);
        this.loadRoles();
      },
      'Could not rename the role.'
    );
  }

  deleteRole(roleName: string): void {
    if (this.saving() || !window.confirm(`Delete the role “${roleName}”? This cannot be undone.`)) return;

    this.runAction(
      this.roleService.delete(roleName),
      `Role “${roleName}” deleted.`,
      () => {
        if (this.activeRole() === roleName) this.activeRole.set('');
        this.loadRoles();
      },
      'Could not delete the role.'
    );
  }

  addPermission(roleName: string): void {
    const permission = this.selectedPermission();
    if (!permission || this.saving()) return;

    this.runAction(
      this.roleService.addPermission(roleName, permission),
      `Permission “${permission}” added to ${roleName}.`,
      () => {
        this.selectedPermission.set('');
        this.loadRoles();
      },
      'Could not add this permission.'
    );
  }

  removePermission(roleName: string, permission: string): void {
    if (this.saving()) return;

    this.runAction(
      this.roleService.removePermission(roleName, permission),
      `Permission “${permission}” removed from ${roleName}.`,
      () => this.loadRoles(),
      'Could not remove this permission.'
    );
  }

  availablePermissionsFor(role: RoleDto): string[] {
    return this.availablePermissions.filter(permission => !role.permissions.includes(permission));
  }

  private runAction(
    request: ReturnType<RoleService['create']>,
    successMessage: string,
    onSuccess: () => void,
    failureMessage: string
  ): void {
    this.saving.set(true);
    this.actionError.set('');
    this.actionMessage.set('');
    request.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.actionMessage.set(successMessage);
        onSuccess();
      },
      error: (err: HttpErrorResponse) => this.actionError.set(this.errorMessage(err, failureMessage))
    });
  }

  private errorMessage(error: HttpErrorResponse, fallback: string): string {
    if (error.status === 401) return 'Your session may have expired. Please sign in again.';
    if (error.status === 403) return 'Administrator access is required to manage roles and permissions.';
    const serverMessage = typeof error.error === 'string'
      ? error.error
      : Array.isArray(error.error)
        ? error.error.join(' ')
        : Array.isArray(error.error?.errors)
          ? error.error.errors.join(' ')
          : '';
    return serverMessage || fallback;
  }
}
