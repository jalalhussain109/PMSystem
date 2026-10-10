import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { finalize } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { ProjectDto, ProjectService } from './project.service';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [DatePipe],
  template: `
    <div class="page-heading"><div><span class="eyebrow">WORK MANAGEMENT</span><h2>Projects</h2><p class="muted">View and organize projects in your workspace.</p></div><button class="primary-button compact" type="button" (click)="reload()">↻ Refresh</button></div>
    @if (loading()) { <div class="empty-state"><span class="loader"></span><p>Loading projects…</p></div> }
    @else if (error()) { <div class="notice error-notice"><strong>Could not load projects</strong><p>{{ error() }}</p><small>Confirm that GET /api/Projects is available and the account has Projects.View permission.</small><button class="secondary-button" (click)="reload()">Try again</button></div> }
    @else if (projects().length === 0) { <div class="empty-state"><span class="empty-icon">◫</span><h3>No projects to display</h3><p>The API returned no projects, or this endpoint needs to be connected to your backend.</p></div> }
    @else { <div class="table-wrap"><table><thead><tr><th>PROJECT</th><th>DESCRIPTION</th><th>STATUS</th><th>END DATE</th></tr></thead><tbody>@for (project of projects(); track project.id) { <tr><td><strong>{{ project.name }}</strong><small class="table-id">Project #{{ project.id }}</small></td><td>{{ project.description || '—' }}</td><td><span class="status-pill">{{ project.status || 'Active' }}</span></td><td>{{ project.endDate ? (project.endDate | date:'mediumDate') : '—' }}</td></tr> }</tbody></table></div> }
  `
})
export class ProjectsComponent {
  private readonly service = inject(ProjectService);
  readonly projects = signal<ProjectDto[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  constructor() { this.reload(); }
  reload(): void {
    this.loading.set(true); this.error.set('');
    this.service.getAll().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: items => this.projects.set(items ?? []),
      error: (err: HttpErrorResponse) => this.error.set(err.status === 401 ? 'Your session may have expired. Please sign in again.' : err.status === 403 ? 'Your account does not have permission to view projects.' : 'Check the API URL, endpoint, and network connection.')
    });
  }
}
