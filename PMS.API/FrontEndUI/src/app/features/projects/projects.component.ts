import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { ProjectDto, ProjectService, UpdateProjectDto } from './project.service';

const SAMPLE_PROJECTS: ProjectDto[] = [
  { id: 101, projectCode: 'WEB-101', name: 'Website Redesign', description: 'Refresh the company website and improve accessibility.', status: 'In Progress', startDate: '2026-09-01', endDate: '2026-11-15' },
  { id: 102, projectCode: 'APP-102', name: 'Mobile App Launch', description: 'Prepare the mobile app for its first public release.', status: 'Planning', startDate: '2026-10-01', endDate: '2026-12-20' },
  { id: 103, projectCode: 'CRM-103', name: 'Customer Portal', description: 'Build a self-service portal for customer account management.', status: 'In Progress', startDate: '2026-08-15', endDate: '2026-12-05' },
  { id: 104, projectCode: 'ANL-104', name: 'Analytics Dashboard', description: 'Give teams a shared view of product and business metrics.', status: 'Completed', startDate: '2026-06-01', endDate: '2026-09-30' }
];

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [DatePipe, FormsModule],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.css'
})
export class ProjectsComponent {
  private readonly service = inject(ProjectService);
  readonly projects = signal<ProjectDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly actionError = signal('');
  readonly showingSampleData = signal(false);
  readonly viewingProject = signal<ProjectDto | null>(null);
  readonly editingProject = signal<ProjectDto | null>(null);
  editForm: UpdateProjectDto = this.emptyEditForm();

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

  viewDetails(project: ProjectDto): void {
    this.viewingProject.set(project);
  }

  closeDetails(): void {
    this.viewingProject.set(null);
  }

  editDetails(project: ProjectDto): void {
    this.actionError.set('');
    this.editForm = {
      projectCode: project.projectCode ?? '',
      projectName: project.name,
      description: project.description ?? '',
      startDate: this.toDateInputValue(project.startDate),
      endDate: this.toDateInputValue(project.endDate),
      priority: project.priority ?? 1,
      projectManagerId: project.projectManagerId ?? null
    };
    this.editingProject.set(project);
  }

  closeEdit(): void {
    if (!this.saving()) this.editingProject.set(null);
  }

  saveProject(): void {
    const project = this.editingProject();
    if (!project || this.saving()) return;

    if (this.showingSampleData()) {
      this.projects.update(items => items.map(item => item.id === project.id ? {
        ...item,
        projectCode: this.editForm.projectCode,
        name: this.editForm.projectName,
        description: this.editForm.description,
        startDate: this.editForm.startDate,
        endDate: this.editForm.endDate,
        priority: this.editForm.priority
      } : item));
      this.editingProject.set(null);
      return;
    }

    this.saving.set(true);
    this.actionError.set('');
    this.service.update(project.id, this.editForm).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: updatedProject => {
        this.projects.update(items => items.map(item => item.id === project.id ? updatedProject : item));
        this.editingProject.set(null);
      },
      error: (err: HttpErrorResponse) => this.actionError.set(err.status === 401 ? 'Your session may have expired. Please sign in again.' : err.status === 403 ? 'Your account does not have permission to edit projects.' : 'Could not save the project. Check the details and try again.')
    });
  }

  private emptyEditForm(): UpdateProjectDto {
    return {
      projectCode: '',
      projectName: '',
      description: '',
      startDate: null,
      endDate: null,
      priority: 1,
      projectManagerId: null
    };
  }

  private toDateInputValue(value?: string | null): string | null {
    return value ? value.slice(0, 10) : null;
  }
}
