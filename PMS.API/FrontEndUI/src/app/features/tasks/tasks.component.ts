import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize, forkJoin } from 'rxjs';
import { CreateTaskDto, TaskDto, TaskService } from './task.service';
import { HttpErrorResponse } from '@angular/common/http';
import { ProjectDto, ProjectService } from '../projects/project.service';
import { UserDto, UserService } from '../users/user.service';

const SAMPLE_TASKS: TaskDto[] = [
  { id: 201, title: 'Review homepage wireframes', projectName: 'Website Redesign', status: 'In Progress', dueDate: '2026-10-14' },
  { id: 202, title: 'Finalize app store screenshots', projectName: 'Mobile App Launch', status: 'Open', dueDate: '2026-10-18' },
  { id: 203, title: 'Implement account settings page', projectName: 'Customer Portal', status: 'In Progress', dueDate: '2026-10-22' },
  { id: 204, title: 'Validate analytics event tracking', projectName: 'Analytics Dashboard', status: 'Completed', dueDate: '2026-10-08' },
  { id: 205, title: 'Prepare stakeholder progress update', projectName: 'Website Redesign', status: 'Open', dueDate: '2026-10-28' }
];

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [DatePipe, FormsModule],
  templateUrl: './tasks.component.html',
  styleUrl: './tasks.component.css'
})
export class TasksComponent {
  private readonly service = inject(TaskService);
  private readonly projectService = inject(ProjectService);
  private readonly userService = inject(UserService);
  readonly tasks = signal<TaskDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly actionError = signal('');
  readonly creatingTask = signal(false);
  readonly lookupsLoading = signal(false);
  readonly lookupError = signal('');
  readonly projects = signal<ProjectDto[]>([]);
  readonly assignees = signal<UserDto[]>([]);
  readonly showingSampleData = signal(false);
  taskForm: CreateTaskDto & { projectId: number | null } = this.emptyTaskForm();

  constructor() { this.reload(); }

  reload(): void {
    this.loading.set(true);
    this.error.set('');
    this.showingSampleData.set(false);
    this.service.getAll().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: items => {
        const tasks = items ?? [];
        this.showingSampleData.set(tasks.length === 0);
        this.tasks.set(tasks.length ? tasks : SAMPLE_TASKS);
      },
      error: (err: HttpErrorResponse) => this.error.set(err.status === 401 ? 'Your session may have expired. Please sign in again.' : err.status === 403 ? 'Your account does not have permission to view tasks.' : 'Check the API URL, endpoint, and network connection.')
    });
  }

  addTask(): void {
    this.actionError.set('');
    this.lookupError.set('');
    this.taskForm = this.emptyTaskForm();
    this.creatingTask.set(true);
    this.loadTaskLookups();
  }

  loadTaskLookups(): void {
    this.lookupsLoading.set(true);
    this.lookupError.set('');
    forkJoin({
      projects: this.projectService.getAll(),
      users: this.userService.getAll()
    }).pipe(finalize(() => this.lookupsLoading.set(false))).subscribe({
      next: ({ projects, users }) => {
        this.projects.set(projects);
        this.assignees.set(users.filter(user => user.isActive));
        if (projects.length === 0) this.lookupError.set('Create a project before adding a task.');
      },
      error: (err: HttpErrorResponse) => this.lookupError.set(err.status === 401
        ? 'Your session may have expired. Please sign in again.'
        : err.status === 403
          ? 'Your account does not have permission to view projects or users.'
          : 'Could not load projects and users. Check the API and try again.')
    });
  }

  closeCreateTask(): void {
    if (!this.saving()) this.creatingTask.set(false);
  }

  saveTask(): void {
    const projectId = this.taskForm.projectId;
    if (projectId == null || this.saving() || this.lookupsLoading() || this.lookupError()) return;

    this.saving.set(true);
    this.actionError.set('');
    const { projectId: _projectId, ...task } = this.taskForm;
    this.service.create(projectId, task).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: createdTask => {
        const projectName = this.projects().find(project => project.id === projectId)?.name ?? null;
        const taskWithProject = { ...createdTask, projectName };
        this.tasks.update(tasks => this.showingSampleData() ? [taskWithProject] : [taskWithProject, ...tasks]);
        this.showingSampleData.set(false);
        this.creatingTask.set(false);
      },
      error: (err: HttpErrorResponse) => this.actionError.set(err.status === 401
        ? 'Your session may have expired. Please sign in again.'
        : err.status === 403
          ? 'Your account does not have permission to create tasks.'
          : 'Could not create the task. Check the details and try again.')
    });
  }

  projectDisplayName(project: ProjectDto): string {
    return project.projectCode ? `${project.projectCode} — ${project.name}` : project.name;
  }

  userDisplayName(user: UserDto): string {
    return user.displayName?.trim() || user.userName?.trim() || user.email?.trim() || `User #${user.id}`;
  }

  private emptyTaskForm(): CreateTaskDto & { projectId: number | null } {
    return {
      projectId: null,
      title: '',
      description: '',
      assignedTo: null,
      priority: 1,
      startDate: null,
      dueDate: null
    };
  }
}
