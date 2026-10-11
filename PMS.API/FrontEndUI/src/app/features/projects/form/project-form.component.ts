import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { UserDto, UserService } from '../../users/user.service';
import { CreateProjectDto, ProjectDto, ProjectService } from '../project.service';

@Component({
  selector: 'app-project-form',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './project-form.component.html',
  styleUrl: './project-form.component.css'
})
export class ProjectFormComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly projectService = inject(ProjectService);
  private readonly userService = inject(UserService);

  readonly project = signal<ProjectDto | null>(null);
  readonly creating = signal(true);
  readonly loading = signal(false);
  readonly users = signal<UserDto[]>([]);
  readonly usersLoading = signal(false);
  readonly usersError = signal('');
  readonly saving = signal(false);
  readonly pageError = signal('');
  readonly actionError = signal('');

  form: CreateProjectDto = this.emptyForm();
  dropdownOpen = false;

  constructor() {
    const idValue = this.route.snapshot.paramMap.get('id');
    this.creating.set(idValue === null);
    if (idValue !== null) {
      const id = Number(idValue);
      if (!Number.isSafeInteger(id) || id <= 0) {
        this.pageError.set('Invalid project ID.');
      } else {
        this.loadProject(id);
      }
    }
    this.loadUsers();
  }

  loadProject(id: number): void {
    this.loading.set(true);
    this.pageError.set('');
    this.projectService.getById(id).pipe(finalize(() => this.loading.set(false))).subscribe({
      next: project => {
        this.project.set(project);
        this.form = {
          projectCode: project.projectCode ?? '',
          projectName: project.name,
          description: project.description ?? '',
          startDate: this.toDateInputValue(project.startDate),
          endDate: this.toDateInputValue(project.endDate),
          priority: project.priority ?? 1,
          projectManagerId: project.projectManagerId ?? null,
          memberIds: [...(project.memberIds ?? [])]
        };
      },
      error: (err: HttpErrorResponse) => this.pageError.set(err.status === 404
        ? 'Project not found.'
        : err.status === 401
          ? 'Your session may have expired. Please sign in again.'
          : err.status === 403
            ? 'Your account does not have permission to view this project.'
            : 'Could not load the project. Check the API URL and try again.')
    });
  }

  loadUsers(): void {
    this.usersLoading.set(true);
    this.usersError.set('');
    this.userService.getAll().pipe(finalize(() => this.usersLoading.set(false))).subscribe({
      next: users => this.users.set(users),
      error: (err: HttpErrorResponse) => this.usersError.set(err.status === 401
        ? 'Your session may have expired. Please sign in again.'
        : err.status === 403
          ? 'Your account does not have permission to view users.'
          : 'Could not load users. Check the API URL and network connection.')
    });
  }

  userDisplayName(user: UserDto): string {
    return user.displayName?.trim() || user.userName?.trim() || user.email?.trim() || `User #${user.id}`;
  }

  selectableTeamMembers(): UserDto[] {
    return this.users().filter(user => user.isActive || this.form.memberIds.includes(user.id));
  }

  selectedTeamMemberLabel(): string {
    const selectedIds = this.form.memberIds;
    if (!selectedIds.length) return 'Select team members';
    if (selectedIds.length === 1) {
      const member = this.users().find(user => user.id === selectedIds[0]);
      return member ? this.userDisplayName(member) : '1 member selected';
    }
    return `${selectedIds.length} members selected`;
  }

  toggleTeamDropdown(): void {
    if (!this.usersLoading() && !this.usersError()) this.dropdownOpen = !this.dropdownOpen;
  }

  setTeamMember(userId: number, selected: boolean): void {
    const memberIds = new Set(this.form.memberIds);
    if (selected) memberIds.add(userId);
    else memberIds.delete(userId);
    this.form = { ...this.form, memberIds: [...memberIds] };
  }

  submit(): void {
    if (this.saving() || this.loading() || this.usersLoading() || this.usersError()) return;

    this.saving.set(true);
    this.actionError.set('');
    const project = this.project();
    const request = project
      ? this.projectService.update(project.id, this.form)
      : this.projectService.create(this.form);

    request.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => void this.router.navigate(['/projects']),
      error: (err: HttpErrorResponse) => this.actionError.set(err.status === 401
        ? 'Your session may have expired. Please sign in again.'
        : err.status === 403
          ? `Your account does not have permission to ${this.creating() ? 'create' : 'edit'} projects.`
          : `Could not ${this.creating() ? 'create' : 'save'} the project. Check the details and try again.`)
    });
  }

  private emptyForm(): CreateProjectDto {
    return {
      projectCode: '',
      projectName: '',
      description: '',
      startDate: null,
      endDate: null,
      priority: 1,
      projectManagerId: null,
      memberIds: []
    };
  }

  private toDateInputValue(value?: string | null): string | null {
    return value ? value.slice(0, 10) : null;
  }
}
