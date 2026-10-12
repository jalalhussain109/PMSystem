import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { UserDto, UserService } from '../user.service';
import { RoleDto, RoleService, UserAccessDto } from '../../roles/role.service';

@Component({
  selector: 'app-assign-user-role',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './assign-user-role.component.html',
  styleUrl: './assign-user-role.component.css'
})
export class AssignUserRoleComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly roleService = inject(RoleService);
  private readonly userService = inject(UserService);

  readonly users = signal<UserDto[]>([]);
  readonly roles = signal<RoleDto[]>([]);
  readonly selectedUserId = signal<number | null>(null);
  readonly selectedRole = signal('');
  readonly userAccess = signal<UserAccessDto | null>(null);
  readonly loadingUsers = signal(false);
  readonly loadingRoles = signal(false);
  readonly loadingAccess = signal(false);
  readonly saving = signal(false);
  readonly usersError = signal('');
  readonly rolesError = signal('');
  readonly accessError = signal('');
  readonly actionError = signal('');
  readonly actionMessage = signal('');

  constructor() {
    this.loadUsers();
    this.loadRoles();
    const userId = Number(this.route.snapshot.queryParamMap.get('userId'));
    if (Number.isSafeInteger(userId) && userId > 0) {
      this.selectedUserId.set(userId);
      this.loadUserAccess(userId);
    }
  }

  loadUsers(): void {
    this.loadingUsers.set(true);
    this.usersError.set('');
    this.userService.getAll().pipe(finalize(() => this.loadingUsers.set(false))).subscribe({
      next: users => this.users.set(users),
      error: (err: HttpErrorResponse) => this.usersError.set(this.errorMessage(err, 'Could not load users.'))
    });
  }

  loadRoles(): void {
    this.loadingRoles.set(true);
    this.rolesError.set('');
    this.roleService.getAll().pipe(finalize(() => this.loadingRoles.set(false))).subscribe({
      next: roles => this.roles.set(roles),
      error: (err: HttpErrorResponse) => this.rolesError.set(this.errorMessage(err, 'Could not load roles.'))
    });
  }

  onUserChange(value: number | null): void {
    const userId = value === null ? null : Number(value);
    this.selectedUserId.set(userId);
    this.userAccess.set(null);
    this.accessError.set('');
    this.actionError.set('');
    this.actionMessage.set('');
    if (userId !== null) this.loadUserAccess(userId);
  }

  loadUserAccess(userId = this.selectedUserId()): void {
    if (userId === null) return;
    this.loadingAccess.set(true);
    this.accessError.set('');
    this.roleService.getUserAccess(userId).pipe(finalize(() => this.loadingAccess.set(false))).subscribe({
      next: access => this.userAccess.set(access),
      error: (err: HttpErrorResponse) => this.accessError.set(this.errorMessage(err, 'Could not load this user’s roles.'))
    });
  }

  assignRole(): void {
    const userId = this.selectedUserId();
    const roleName = this.selectedRole();
    if (userId === null || !roleName || this.saving()) return;
    if (this.userAccess()?.roles.includes(roleName)) {
      this.actionError.set('This user already has the selected role.');
      return;
    }

    this.runAction(
      this.roleService.assignRole(userId, roleName),
      `Role “${roleName}” assigned.`,
      () => {
        this.selectedRole.set('');
        this.loadUserAccess(userId);
      },
      'Could not assign this role.'
    );
  }

  removeRole(roleName: string): void {
    const userId = this.selectedUserId();
    if (userId === null || this.saving()) return;

    this.runAction(
      this.roleService.removeUserRole(userId, roleName),
      `Role “${roleName}” removed.`,
      () => this.loadUserAccess(userId),
      'Could not remove this role.'
    );
  }

  userDisplayName(user: UserDto): string {
    return user.displayName?.trim() || user.userName?.trim() || user.email?.trim() || `User #${user.id}`;
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
    if (error.status === 403) return 'Administrator access is required to assign user roles.';
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
