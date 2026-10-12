import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { CreateUserDto, UpdateUserDto, UserDto, UserService } from './user.service';

interface UserForm extends UpdateUserDto {
  password: string;
}

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [DatePipe, RouterModule, FormsModule, RouterLink],
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
  readonly creatingUser = signal(false);
  editForm: UserForm = this.emptyEditForm();

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
    this.creatingUser.set(false);
    this.editForm = {
      userName: user.userName ?? '',
      displayName: user.displayName ?? '',
      email: user.email ?? '',
      phoneNumber: user.phoneNumber ?? '',
      isActive: user.isActive,
      password: ''
    };
    this.editingUser.set(user);
  }

  addUser(): void {
    this.actionError.set('');
    this.editingUser.set(null);
    this.editForm = this.emptyEditForm();
    this.creatingUser.set(true);
  }

  closeEdit(): void {
    if (!this.saving()) {
      this.editingUser.set(null);
      this.creatingUser.set(false);
    }
  }

  saveUser(): void {
    const user = this.editingUser();
    const isCreating = this.creatingUser();
    if ((!user && !isCreating) || this.saving()) return;

    this.saving.set(true);
    this.actionError.set('');
    const request = user
      ? this.service.update(user.id, this.editForm)
      : this.service.create(this.toCreateUserDto(this.editForm));
    request.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: savedUser => {
        if (user) {
          this.users.update(users => users.map(item => item.id === savedUser.id ? savedUser : item));
        } else {
          this.users.update(users => [savedUser, ...users]);
        }
        this.editingUser.set(null);
        this.creatingUser.set(false);
      },
      error: (err: HttpErrorResponse) => this.actionError.set(err.status === 401
        ? 'Your session may have expired. Please sign in again.'
        : err.status === 403
          ? `Your account does not have permission to ${isCreating ? 'create' : 'edit'} users.`
          : `Could not ${isCreating ? 'create' : 'save'} the user. Check the details and try again.`)
    });
  }

  private emptyEditForm(): UserForm {
    return {
      userName: '',
      displayName: '',
      email: '',
      phoneNumber: '',
      isActive: true,
      password: ''
    };
  }

  private toCreateUserDto(form: UserForm): CreateUserDto {
    return {
      userName: form.userName,
      displayName: form.displayName,
      email: form.email,
      phoneNumber: form.phoneNumber,
      password: form.password
    };
  }
}
