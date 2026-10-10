import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { finalize } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { ProjectDto, ProjectService } from './project.service';

const SAMPLE_PROJECTS: ProjectDto[] = [
  { id: 101, name: 'Website Redesign', description: 'Refresh the company website and improve accessibility.', status: 'In Progress', startDate: '2026-09-01', endDate: '2026-11-15' },
  { id: 102, name: 'Mobile App Launch', description: 'Prepare the mobile app for its first public release.', status: 'Planning', startDate: '2026-10-01', endDate: '2026-12-20' },
  { id: 103, name: 'Customer Portal', description: 'Build a self-service portal for customer account management.', status: 'In Progress', startDate: '2026-08-15', endDate: '2026-12-05' },
  { id: 104, name: 'Analytics Dashboard', description: 'Give teams a shared view of product and business metrics.', status: 'Completed', startDate: '2026-06-01', endDate: '2026-09-30' }
];

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
  readonly showingSampleData = signal(false);
  constructor() { this.reload(); }
  reload(): void {
    this.loading.set(true);
    this.error.set('');
    this.showingSampleData.set(false);
    this.service.getAll().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: items => {
        const projects = items ?? [];
        this.showingSampleData.set(projects.length === 0);
        this.projects.set(projects.length ? projects : SAMPLE_PROJECTS);
      },
      error: (err: HttpErrorResponse) => this.error.set(err.status === 401 ? 'Your session may have expired. Please sign in again.' : err.status === 403 ? 'Your account does not have permission to view projects.' : 'Check the API URL, endpoint, and network connection.')
    });
  }
}
