import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';
import { ProjectDto, ProjectService } from '../../projects/project.service';
import { ProjectMemberDto, ProjectMemberService } from '../../projects/project-member.service';
import { UserDto, UserService } from '../../users/user.service';
import { CreateTaskDto, TaskDto, TaskService } from '../task.service';

interface AssigneeOption {
  id: number;
  displayName: string | null;
  userName: string | null;
  email: string | null;
  isActive: boolean;
}

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.css'
})
export class TaskFormComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly taskService = inject(TaskService);
  private readonly projectService = inject(ProjectService);
  private readonly projectMemberService = inject(ProjectMemberService);
  private readonly userService = inject(UserService);

  readonly task = signal<TaskDto | null>(null);
  readonly parentTask = signal<TaskDto | null>(null);
  readonly creating = signal(true);
  readonly creatingSubtask = signal(false);
  readonly loading = signal(false);
  readonly projects = signal<ProjectDto[]>([]);
  readonly projectMembers = signal<ProjectMemberDto[]>([]);
  readonly assignees = computed<AssigneeOption[]>(() => this.projectMembers()
    .filter(member => member.isActive || member.userId === this.form.assignedTo)
    .map(member => {
      const user = this.users().find(item => item.id === member.userId);
      return {
        id: member.userId,
        displayName: user?.displayName ?? member.userName ?? null,
        userName: user?.userName ?? member.userName ?? null,
        email: user?.email ?? null,
        isActive: member.isActive
      };
    }));
  readonly users = signal<UserDto[]>([]);
  readonly lookupsLoading = signal(false);
  readonly lookupError = signal('');
  readonly assigneesLoading = signal(false);
  readonly assigneesError = signal('');
  readonly saving = signal(false);
  readonly pageError = signal('');
  readonly actionError = signal('');
  private lookupsReady = false;
  private assigneeRequestId = 0;

  form: CreateTaskDto & { projectId: number | null } = this.emptyForm();

  constructor() {
    const parentIdValue = this.route.snapshot.paramMap.get('parentId');
    const idValue = this.route.snapshot.paramMap.get('id');
    this.creatingSubtask.set(parentIdValue !== null);
    this.creating.set(idValue === null);
    if (parentIdValue !== null) {
      const parentId = Number(parentIdValue);
      if (!Number.isSafeInteger(parentId) || parentId <= 0) {
        this.pageError.set('Invalid parent task ID.');
      } else {
        this.loadParentTask(parentId);
      }
    } else if (idValue !== null) {
      const id = Number(idValue);
      if (!Number.isSafeInteger(id) || id <= 0) {
        this.pageError.set('Invalid task ID.');
      } else {
        this.loadTask(id);
      }
    }
    this.loadLookups();
  }

  loadParentTask(id = Number(this.route.snapshot.paramMap.get('parentId'))): void {
    if (!Number.isSafeInteger(id) || id <= 0) {
      this.pageError.set('Invalid parent task ID.');
      return;
    }

    this.loading.set(true);
    this.pageError.set('');
    this.taskService.getById(id).pipe(finalize(() => this.loading.set(false))).subscribe({
      next: parentTask => {
        this.parentTask.set(parentTask);
        this.form = { ...this.form, projectId: parentTask.projectId ?? null };
        this.loadAssigneesWhenReady();
      },
      error: (err: HttpErrorResponse) => this.pageError.set(err.status === 404
        ? 'Parent task not found.'
        : err.status === 401
          ? 'Your session may have expired. Please sign in again.'
          : err.status === 403
            ? 'Your account does not have permission to view the parent task.'
            : 'Could not load the parent task. Check the API URL and try again.')
    });
  }

  retryLoad(): void {
    if (this.creatingSubtask()) this.loadParentTask();
    else this.loadTask();
  }

  loadTask(id = Number(this.route.snapshot.paramMap.get('id'))): void {
    if (!Number.isSafeInteger(id) || id <= 0) {
      this.pageError.set('Invalid task ID.');
      return;
    }

    this.loading.set(true);
    this.pageError.set('');
    this.taskService.getById(id).pipe(finalize(() => this.loading.set(false))).subscribe({
      next: task => {
        this.task.set(task);
        this.form = {
          projectId: task.projectId ?? null,
          title: task.title,
          description: task.description ?? '',
          assignedTo: task.assignedTo ?? null,
          priority: task.priority ?? 1,
          startDate: this.toDateInputValue(task.startDate),
          dueDate: this.toDateInputValue(task.dueDate)
        };
        this.loadAssigneesWhenReady();
      },
      error: (err: HttpErrorResponse) => this.pageError.set(err.status === 404
        ? 'Task not found.'
        : err.status === 401
          ? 'Your session may have expired. Please sign in again.'
          : err.status === 403
            ? 'Your account does not have permission to view this task.'
            : 'Could not load the task. Check the API URL and try again.')
    });
  }

  loadLookups(): void {
    this.lookupsLoading.set(true);
    this.lookupError.set('');
    forkJoin({
      projects: this.projectService.getLookup(),
      users: this.userService.getAll()
    }).pipe(finalize(() => this.lookupsLoading.set(false))).subscribe({
      next: ({ projects, users }) => {
        this.projects.set(projects);
        this.users.set(users);
        this.lookupsReady = true;
        this.loadAssigneesWhenReady();
        if (this.creating() && !this.creatingSubtask() && projects.length === 0) {
          this.lookupError.set('Create a project before adding a task.');
        }
      },
      error: (err: HttpErrorResponse) => this.lookupError.set(err.status === 401
        ? 'Your session may have expired. Please sign in again.'
        : err.status === 403
          ? 'Your account does not have permission to view projects or users.'
          : 'Could not load projects and users. Check the API and try again.')
    });
  }

  onProjectChange(projectId: number | null): void {
    this.form = { ...this.form, projectId, assignedTo: null };
    this.assigneeRequestId++;
    this.assigneesLoading.set(false);
    this.projectMembers.set([]);
    this.assigneesError.set('');
    if (projectId !== null) this.fetchProjectAssignees(projectId);
  }

  loadProjectAssignees(): void {
    const projectId = this.form.projectId;
    if (projectId !== null) this.fetchProjectAssignees(projectId);
  }

  private loadAssigneesWhenReady(): void {
    if (this.lookupsReady && this.form.projectId !== null) {
      this.fetchProjectAssignees(this.form.projectId);
    }
  }

  private fetchProjectAssignees(projectId: number): void {
    const requestId = ++this.assigneeRequestId;
    this.assigneesLoading.set(true);
    this.assigneesError.set('');
    this.projectMemberService.getByProject(projectId)
      .pipe(finalize(() => {
        if (requestId === this.assigneeRequestId) this.assigneesLoading.set(false);
      }))
      .subscribe({
        next: members => {
          if (requestId !== this.assigneeRequestId) return;
          this.projectMembers.set(members);
          if (this.form.assignedTo !== null
            && !members.some(member => member.userId === this.form.assignedTo)) {
            this.form = { ...this.form, assignedTo: null };
          }
        },
        error: (err: HttpErrorResponse) => {
          if (requestId !== this.assigneeRequestId) return;
          this.assigneesError.set(err.status === 401
            ? 'Your session may have expired. Please sign in again.'
            : err.status === 403
              ? 'Your account does not have permission to view project members.'
              : 'Could not load this project’s members. Retry to load assignees.');
        }
      });
  }

  projectDisplayName(project: ProjectDto): string {
    return project.projectCode ? `${project.projectCode} — ${project.name}` : project.name;
  }

  userDisplayName(user: AssigneeOption): string {
    return user.displayName?.trim() || user.userName?.trim() || user.email?.trim() || `User #${user.id}`;
  }

  selectedProjectName(): string {
    const id = this.form.projectId;
    const project = this.projects().find(item => item.id === id);
    return project ? this.projectDisplayName(project) : id == null ? '—' : `Project #${id}`;
  }

  parentProjectName(): string {
    const parentTask = this.parentTask();
    return parentTask ? this.selectedProjectName() : '—';
  }

  submit(): void {
    if (this.saving() || this.loading() || this.lookupsLoading() || this.lookupError()
      || (this.creating() && (this.assigneesLoading() || this.assigneesError()))) return;
    if (this.creating() && !this.creatingSubtask() && this.form.projectId == null) return;
    if (this.creatingSubtask() && (!this.parentTask() || this.form.projectId == null)) return;

    const { projectId, ...taskData } = this.form;
    const currentTask = this.task();
    const parentTask = this.parentTask();
    const request = currentTask
      ? this.taskService.update(currentTask.id, taskData)
      : parentTask
        ? this.taskService.createSubtask(parentTask.id, taskData)
        : projectId === null
          ? null
          : this.taskService.create(projectId, taskData);
    if (!request) return;

    this.saving.set(true);
    this.actionError.set('');
    request.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => void this.router.navigate(parentTask
        ? ['/tasks', parentTask.id, 'view']
        : ['/tasks']),
      error: (err: HttpErrorResponse) => this.actionError.set(err.status === 401
        ? 'Your session may have expired. Please sign in again.'
        : err.status === 403
          ? `Your account does not have permission to ${this.creating() ? 'create' : 'edit'} ${this.creatingSubtask() ? 'subtasks' : 'tasks'}.`
          : `Could not ${this.creating() ? 'create' : 'save'} the ${this.creatingSubtask() ? 'subtask' : 'task'}. Check the details and try again.`)
    });
  }

  private emptyForm(): CreateTaskDto & { projectId: number | null } {
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

  private toDateInputValue(value?: string | null): string | null {
    return value ? value.slice(0, 10) : null;
  }
}
