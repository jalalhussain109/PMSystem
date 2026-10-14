import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { ProjectDto, ProjectService } from './project.service';

interface ProjectStatusOption {
  value: string;
  label: string;
}

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [DatePipe, FormsModule, RouterLink],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.css'
})
export class ProjectsComponent {
  readonly auth = inject(AuthService);
  private readonly service = inject(ProjectService);
  private requestId = 0;

  readonly projects = signal<ProjectDto[]>([]);
  readonly changingStatusIds = signal<Set<number>>(new Set());
  readonly loading = signal(false);
  readonly error = signal('');
  readonly statusActionError = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);

  constructor() {
    this.reload();
  }

  statusValue(status?: string): string {
    const normalized = status?.replace(/\s+/g, '').toLocaleLowerCase();
    return ({
      draft: 'Draft',
      planning: 'Planning',
      inprogress: 'InProgress',
      onhold: 'OnHold',
      completed: 'Completed',
      cancelled: 'Cancelled'
    } as Record<string, string>)[normalized ?? ''] ?? 'Draft';
  }

  statusLabel(status?: string): string {
    return status === 'InProgress' ? 'In Progress'
      : status === 'OnHold' ? 'On Hold'
        : status ?? 'Draft';
  }

  projectStatusOptions(project: ProjectDto): ProjectStatusOption[] {
    const current = this.statusValue(project.status);
    const transitions: Record<string, string[]> = {
      Draft: ['Planning', 'Cancelled'],
      Planning: ['InProgress', 'Cancelled'],
      InProgress: ['Completed', 'OnHold', 'Cancelled'],
      OnHold: ['InProgress']
    };
    return [current, ...(transitions[current] ?? [])]
      .map(value => ({ value, label: this.statusLabel(value) }));
  }

  changeStatus(projectId: number, newStatus: string): void {
    const project = this.projects().find(item => item.id === projectId);
    if (!project || this.changingStatusIds().has(projectId)
      || this.statusValue(project.status) === newStatus) return;

    this.statusActionError.set('');
    this.changingStatusIds.update(ids => new Set(ids).add(projectId));
    this.service.changeStatus(projectId, newStatus).pipe(finalize(() => {
      this.changingStatusIds.update(ids => {
        const next = new Set(ids);
        next.delete(projectId);
        return next;
      });
    })).subscribe({
      next: updated => this.projects.update(items => items.map(item => item.id === updated.id ? updated : item)),
      error: (err: HttpErrorResponse) => this.statusActionError.set(err.status === 401
        ? 'Your session may have expired. Please sign in again.'
        : err.status === 403
          ? 'Your account does not have permission to change project status.'
          : 'Could not update the project status. The selected transition may not be allowed.')
    });
  }

  deleteProject(id: number): void {
    if (!confirm('Are you sure you want to delete this project?')) return;
    this.loading.set(true);
    this.service.delete(id).pipe(finalize(() => this.loading.set(false))).subscribe({
      next: () => this.reload(this.page()),
      error: (err: HttpErrorResponse) => {
        this.error.set(err.status === 401
          ? 'Your session may have expired. Please sign in again.'
          : err.status === 403
            ? 'Your account does not have permission to delete projects.'
            : 'Failed to delete project.');
      }
    });
  }

  reload(page = this.page()): void {
    const requestId = ++this.requestId;
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');
    this.statusActionError.set('');
    this.service.getAll({ page, pageSize: this.pageSize() }).pipe(finalize(() => {
      if (requestId === this.requestId) this.loading.set(false);
    })).subscribe({
      next: result => {
        if (requestId !== this.requestId) return;
        this.projects.set(result.items);
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
            ? 'Your account does not have permission to view projects.'
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
}
