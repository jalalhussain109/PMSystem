import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { finalize } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { ProjectDto, ProjectService } from './project.service';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.css'
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
