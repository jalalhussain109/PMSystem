import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { UpdateUserDto, UserDto, UserService } from './user.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [DatePipe, FormsModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class UsersComponent {
  private readonly service = inject(UserService);

  readonly users = signal<UserDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly actionError = signal('');
  readonly viewingUser = signal<UserDto | null>(null);
  readonly editingUser = signal<UserDto | null>(null);
  editForm: UpdateUserDto = this.emptyEditForm();

  constructor() {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.error.set('');
    this.service.getAll().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: users => this.users.set(users),
      error: (err: HttpErrorResponse) => this.error.set(err.status === 401
        ? 'Your session may have expired. Please sign in again.'
        : err.status === 403
          ? 'Your account does not have permission to view users.'
          : 'Check the API URL, endpoint, and network connection.')
    });
  }

  viewDetails(user: UserDto): void {
    this.viewingUser.set(user);
  }

  closeDetails(): void {
    this.viewingUser.set(null);
  }

  editDetails(user: UserDto): void {
    this.actionError.set('');
    this.editForm = {
      userName: user.userName ?? '',
      displayName: user.displayName ?? '',
      email: user.email ?? '',
      phoneNumber: user.phoneNumber ?? '',
      isActive: user.isActive
    };
    this.editingUser.set(user);
  }

  closeEdit(): void {
    if (!this.saving()) this.editingUser.set(null);
  }

  saveUser(): void {
    const user = this.editingUser();
    if (!user || this.saving()) return;

    this.saving.set(true);
    this.actionError.set('');
    this.service.update(user.id, this.editForm).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: updatedUser => {
        this.users.update(users => users.map(item => item.id === updatedUser.id ? updatedUser : item));
        this.editingUser.set(null);
      },
      error: (err: HttpErrorResponse) => this.actionError.set(err.status === 401
        ? 'Your session may have expired. Please sign in again.'
        : err.status === 403
          ? 'Your account does not have permission to edit users.'
          : 'Could not save the user. Check the details and try again.')
    });
  }

  private emptyEditForm(): UpdateUserDto {
    return {
      userName: '',
      displayName: '',
      email: '',
      phoneNumber: '',
      isActive: true
    };
  }
}
