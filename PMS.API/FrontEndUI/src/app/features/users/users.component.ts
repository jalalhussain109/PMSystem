import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
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
  readonly auth = inject(AuthService);
  private readonly service = inject(UserService);
  private requestId = 0;

  readonly users = signal<UserDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly actionError = signal('');
  readonly viewingUser = signal<UserDto | null>(null);
  readonly editingUser = signal<UserDto | null>(null);
  readonly creatingUser = signal(false);
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);
  editForm: UserForm = this.emptyEditForm();

  constructor() {
    this.reload();
  }

  reload(page = this.page()): void {
    const requestId = ++this.requestId;
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');
    this.service.getPaged({ page, pageSize: this.pageSize() }).pipe(finalize(() => {
      if (requestId === this.requestId) this.loading.set(false);
    })).subscribe({
      next: result => {
        if (requestId !== this.requestId) return;
        this.users.set(result.items);
        this.totalCount.set(result.totalCount);
        this.page.set(result.page);
        this.pageSize.set(result.pageSize);
        this.totalPages.set(result.totalPages);
      },
      error: (err: HttpErrorResponse) => {
        if (requestId !== this.requestId) return;
        this.error.set(err.status === 401
          ? 'Your session may have expired. Please sign in again.'
          : err.status === 403
            ? 'Your account does not have permission to view users.'
            : 'Check the API URL, endpoint, and network connection.');
      }
    });
  }

  setPageSize(value: number): void {
    this.pageSize.set(Number(value));
    this.reload(1);
  }

  setPageSizeFromEvent(event: Event): void {
    if (event.target instanceof HTMLSelectElement) {
      this.setPageSize(Number(event.target.value));
    }
  }

  previousPage(): void {
    if (this.page() > 1) this.reload(this.page() - 1);
  }

  nextPage(): void {
    if (this.page() < this.totalPages()) this.reload(this.page() + 1);
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
          this.reload(1);
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
