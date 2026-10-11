import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { CreateProjectDto, ProjectDto, ProjectService } from './project.service';
import { UserDto, UserService } from '../users/user.service';

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
  private readonly userService = inject(UserService);
  readonly projects = signal<ProjectDto[]>([]);
  readonly projectManagers = signal<UserDto[]>([]);
  readonly loading = signal(false);
  readonly managersLoading = signal(false);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly managersError = signal('');
  readonly actionError = signal('');
  readonly showingSampleData = signal(false);
  readonly viewingProject = signal<ProjectDto | null>(null);
  readonly editingProject = signal<ProjectDto | null>(null);
  readonly creatingProject = signal(false);
  readonly teamDropdownOpen = signal(false);
  editForm: CreateProjectDto = this.emptyProjectForm();

  constructor() {
    this.reload();
    this.loadProjectManagers();
  }

  loadProjectManagers(): void {
    this.managersLoading.set(true);
    this.managersError.set('');
    this.userService.getAll().pipe(finalize(() => this.managersLoading.set(false))).subscribe({
      next: users => this.projectManagers.set(users),
      error: (err: HttpErrorResponse) => this.managersError.set(err.status === 401
        ? 'Your session may have expired. Please sign in again.'
        : err.status === 403
          ? 'Your account does not have permission to view users.'
          : 'Could not load users. Check the API URL and network connection.')
    });
  }

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

  getProjectManagerName(projectManagerId?: number | null): string {
    if (projectManagerId == null) return 'Unassigned';
    const manager = this.projectManagers().find(user => user.id === projectManagerId);
    return manager ? this.userDisplayName(manager) : `User #${projectManagerId}`;
  }

  getProjectMemberNames(memberIds?: number[]): string {
    if (!memberIds?.length) return 'No team members assigned';
    return memberIds.map(id => {
      const member = this.projectManagers().find(user => user.id === id);
      return member ? this.userDisplayName(member) : `User #${id}`;
    }).join(', ');
  }

  selectableTeamMembers(): UserDto[] {
    const selectedIds = this.editForm.memberIds;
    return this.projectManagers().filter(user => user.isActive || selectedIds.includes(user.id));
  }

  toggleTeamDropdown(): void {
    if (this.managersLoading() || this.managersError()) return;
    this.teamDropdownOpen.update(isOpen => !isOpen);
  }

  setTeamMember(userId: number, selected: boolean): void {
    const memberIds = new Set(this.editForm.memberIds);
    if (selected) memberIds.add(userId);
    else memberIds.delete(userId);
    this.editForm.memberIds = [...memberIds];
  }

  selectedTeamMemberLabel(): string {
    const memberIds = this.editForm.memberIds;
    if (!memberIds.length) return 'Select team members';
    if (memberIds.length === 1) {
      const member = this.projectManagers().find(user => user.id === memberIds[0]);
      return member ? this.userDisplayName(member) : '1 member selected';
    }
    return `${memberIds.length} members selected`;
  }

  userDisplayName(user: UserDto): string {
    return user.displayName?.trim() || user.userName?.trim() || user.email?.trim() || `User #${user.id}`;
  }

  editDetails(project: ProjectDto): void {
    this.actionError.set('');
    this.teamDropdownOpen.set(false);
    this.creatingProject.set(false);
    this.editForm = {
      projectCode: project.projectCode ?? '',
      projectName: project.name,
      description: project.description ?? '',
      startDate: this.toDateInputValue(project.startDate),
      endDate: this.toDateInputValue(project.endDate),
      priority: project.priority ?? 1,
      projectManagerId: project.projectManagerId ?? null,
      memberIds: project.memberIds ?? []
    };
    this.editingProject.set(project);
  }

  addProject(): void {
    this.actionError.set('');
    this.teamDropdownOpen.set(false);
    this.editingProject.set(null);
    this.editForm = this.emptyProjectForm();
    this.creatingProject.set(true);
  }

  closeEdit(): void {
    if (!this.saving()) {
      this.editingProject.set(null);
      this.creatingProject.set(false);
      this.teamDropdownOpen.set(false);
    }
  }

  saveProject(): void {
    const project = this.editingProject();
    const isCreating = this.creatingProject();
    if ((!project && !isCreating) || this.saving()) return;

    if (!isCreating && project && this.showingSampleData()) {
      this.projects.update(items => items.map(item => item.id === project.id ? {
        ...item,
        projectCode: this.editForm.projectCode,
        name: this.editForm.projectName,
        description: this.editForm.description,
        startDate: this.editForm.startDate,
        endDate: this.editForm.endDate,
        priority: this.editForm.priority,
        projectManagerId: this.editForm.projectManagerId,
        memberIds: this.editForm.memberIds
      } : item));
      this.editingProject.set(null);
      return;
    }

    this.saving.set(true);
    this.actionError.set('');
    const request = project
      ? this.service.update(project.id, this.editForm)
      : this.service.create(this.editForm);
    request.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: updatedProject => {
        if (project) {
          this.projects.update(items => items.map(item => item.id === project.id ? updatedProject : item));
        } else {
          this.projects.update(items => this.showingSampleData() ? [updatedProject] : [updatedProject, ...items]);
          this.showingSampleData.set(false);
        }
        this.editingProject.set(null);
        this.creatingProject.set(false);
        this.teamDropdownOpen.set(false);
      },
      error: (err: HttpErrorResponse) => this.actionError.set(err.status === 401 ? 'Your session may have expired. Please sign in again.' : err.status === 403 ? `Your account does not have permission to ${isCreating ? 'create' : 'edit'} projects.` : `Could not ${isCreating ? 'create' : 'save'} the project. Check the details and try again.`)
    });
  }

  private emptyProjectForm(): CreateProjectDto {
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
