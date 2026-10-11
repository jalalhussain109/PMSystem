import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { UserDto, UserService } from '../../users/user.service';
import { ProjectDto, ProjectService } from '../project.service';

@Component({
  selector: 'app-project-details',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './project-details.component.html',
  styleUrl: './project-details.component.css'
})
export class ProjectDetailsComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly projectService = inject(ProjectService);
  private readonly userService = inject(UserService);

  readonly project = signal<ProjectDto | null>(null);
  readonly users = signal<UserDto[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly usersError = signal('');

  constructor() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isSafeInteger(id) || id <= 0) {
      this.error.set('Invalid project ID.');
    } else {
      this.loadProject();
    }
    this.loadUsers();
  }

  loadProject(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isSafeInteger(id) || id <= 0) {
      this.error.set('Invalid project ID.');
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.projectService.getById(id).pipe(finalize(() => this.loading.set(false))).subscribe({
      next: project => this.project.set(project),
      error: (err: HttpErrorResponse) => this.error.set(err.status === 404
        ? 'Project not found.'
        : err.status === 401
          ? 'Your session may have expired. Please sign in again.'
          : err.status === 403
            ? 'Your account does not have permission to view this project.'
            : 'Could not load the project. Check the API URL and try again.')
    });
  }

  loadUsers(): void {
    this.usersError.set('');
    this.userService.getAll().subscribe({
      next: users => this.users.set(users),
      error: () => this.usersError.set('Could not load user names. Member IDs are shown instead.')
    });
  }

  managerName(projectManagerId?: number | null): string {
    if (projectManagerId == null) return 'Unassigned';
    const manager = this.users().find(user => user.id === projectManagerId);
    return manager ? this.userDisplayName(manager) : `User #${projectManagerId}`;
  }

  memberNames(memberIds?: number[]): string {
    if (!memberIds?.length) return 'No team members assigned';
    return memberIds.map(id => {
      const member = this.users().find(user => user.id === id);
      return member ? this.userDisplayName(member) : `User #${id}`;
    }).join(', ');
  }

  private userDisplayName(user: UserDto): string {
    return user.displayName?.trim() || user.userName?.trim() || user.email?.trim() || `User #${user.id}`;
  }
}
