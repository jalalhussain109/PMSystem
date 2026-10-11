import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ProjectDto, ProjectService } from './project.service';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.css'
})
export class ProjectsComponent {
  private readonly service = inject(ProjectService);
  private requestId = 0;

  readonly projects = signal<ProjectDto[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);

  constructor() {
    this.reload();
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
    this.service.getPaged(page, this.pageSize()).pipe(finalize(() => {
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
