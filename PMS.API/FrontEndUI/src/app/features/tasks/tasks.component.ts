import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { ProjectDto, ProjectService } from '../projects/project.service';
import { TaskDto, TaskService } from './task.service';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [DatePipe, FormsModule, RouterLink],
  templateUrl: './tasks.component.html',
  styleUrl: './tasks.component.css'
})
export class TasksComponent {
  readonly auth = inject(AuthService);
  private readonly service = inject(TaskService);
  private readonly projectService = inject(ProjectService);
  private requestId = 0;

  readonly tasks = signal<TaskDto[]>([]);
  readonly projects = signal<ProjectDto[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly projectsLoading = signal(false);
  readonly projectsError = signal('');
  readonly projectFilter = signal<number | null>(null);
  readonly titleFilter = signal('');
  readonly startDateFilter = signal('');
  readonly endDateFilter = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);

  constructor() {
    this.reload();
    this.loadProjects();
  }

  reload(page = this.page()): void {
    const requestId = ++this.requestId;
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');
    this.service.getAll({
      page,
      pageSize: this.pageSize(),
      search: this.titleFilter().trim() || undefined,
      projectId: this.projectFilter() ?? undefined,
      startFrom: this.startDateFilter() || undefined,
      dueTo: this.endDateFilter() || undefined
    }).pipe(finalize(() => {
      if (requestId === this.requestId) this.loading.set(false);
    })).subscribe({
      next: result => {
        if (requestId !== this.requestId) return;
        this.tasks.set(result.items);
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
            ? 'Your account does not have permission to view tasks.'
            : 'Check the API URL, endpoint, and network connection.');
      }
    });
  }

  loadProjects(): void {
    this.projectsLoading.set(true);
    this.projectsError.set('');
    this.projectService.getLookup().pipe(finalize(() => this.projectsLoading.set(false))).subscribe({
      next: projects => {
        
        this.projects.set(projects)
      },
      error: (err: HttpErrorResponse) => this.projectsError.set(err.status === 401
        ? 'Your session may have expired. Please sign in again.'
        : err.status === 403
          ? 'Your account does not have permission to view projects.'
          : 'Could not load projects for filtering.')
    });
  }

  clearFilters(): void {
    this.projectFilter.set(null);
    this.titleFilter.set('');
    this.startDateFilter.set('');
    this.endDateFilter.set('');
    this.reload(1);
  }

  setProjectFilter(value: number | null): void {
    this.projectFilter.set(value === null ? null : Number(value));
    this.reload(1);
  }

  setTitleFilter(value: string): void {
    this.titleFilter.set(value);
    this.reload(1);
  }

  setStartDateFilter(value: string): void {
    this.startDateFilter.set(value);
    this.reload(1);
  }

  setEndDateFilter(value: string): void {
    this.endDateFilter.set(value);
    this.reload(1);
  }

  setPageSize(value: number): void {
    this.pageSize.set(Number(value));
    this.reload(1);
  }

  previousPage(): void {
    if (this.page() > 1) this.reload(this.page() - 1);
  }

  nextPage(): void {
    if (this.page() < this.totalPages()) this.reload(this.page() + 1);
  }
}
