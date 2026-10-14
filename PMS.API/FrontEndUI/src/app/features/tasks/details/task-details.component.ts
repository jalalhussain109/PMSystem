import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { ProjectDto, ProjectService } from '../../projects/project.service';
import { UserDto, UserService } from '../../users/user.service';
import { TaskDto, TaskService } from '../task.service';

@Component({
  selector: 'app-task-details',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './task-details.component.html',
  styleUrl: './task-details.component.css'
})
export class TaskDetailsComponent {
  readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly taskService = inject(TaskService);
  private readonly projectService = inject(ProjectService);
  private readonly userService = inject(UserService);

  readonly task = signal<TaskDto | null>(null);
  readonly projects = signal<ProjectDto[]>([]);
  readonly users = signal<UserDto[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly lookupsError = signal('');

  constructor() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isSafeInteger(id) || id <= 0) {
      this.error.set('Invalid task ID.');
    } else {
      this.loadTask(id);
    }
    this.loadLookups();
  }

  loadTask(id = Number(this.route.snapshot.paramMap.get('id'))): void {
    if (!Number.isSafeInteger(id) || id <= 0) {
      this.error.set('Invalid task ID.');
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.taskService.getById(id).pipe(finalize(() => this.loading.set(false))).subscribe({
      next: task => this.task.set(task),
      error: (err: HttpErrorResponse) => this.error.set(err.status === 404
        ? 'Task not found.'
        : err.status === 401
          ? 'Your session may have expired. Please sign in again.'
          : err.status === 403
            ? 'Your account does not have permission to view this task.'
            : 'Could not load the task. Check the API URL and try again.')
    });
  }

  loadLookups(): void {
    this.lookupsError.set('');
    this.projectService.getLookup().subscribe({
      next: projects => this.projects.set(projects),
      error: () => this.lookupsError.set('Could not load project and user names. IDs are shown when names are unavailable.')
    });
    this.userService.getAll().subscribe({
      next: users => this.users.set(users),
      error: () => this.lookupsError.set('Could not load project and user names. IDs are shown when names are unavailable.')
    });
  }

  projectName(task: TaskDto): string {
    if (task.projectName) return task.projectName;
    const project = this.projects().find(item => item.id === task.projectId);
    return project ? project.name : task.projectId == null ? '—' : `Project #${task.projectId}`;
  }

  assigneeName(assignedTo?: number | null): string {
    if (assignedTo == null) return 'Unassigned';
    const user = this.users().find(item => item.id === assignedTo);
    return user
      ? user.displayName?.trim() || user.userName?.trim() || user.email?.trim() || `User #${user.id}`
      : `User #${assignedTo}`;
  }

  priorityName(priority?: number): string {
    return priority === undefined ? '—' : ['Low', 'Medium', 'High', 'Critical'][priority] ?? String(priority);
  }

  statusLabel(status?: string): string {
    return status === 'InProgress' ? 'In Progress'
      : status === 'OnHold' ? 'On Hold'
        : status ?? 'Open';
  }
}
