import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ProjectDto, ProjectService } from '../project.service';
import { ProjectMemberDto, ProjectMemberService } from '../project-member.service';
import { UserDto, UserService } from '../../users/user.service';

@Component({
  selector: 'app-assign-project-user',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './assign-project-user.component.html',
  styleUrl: './assign-project-user.component.css'
})
export class AssignProjectUserComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly projectService = inject(ProjectService);
  private readonly memberService = inject(ProjectMemberService);
  private readonly userService = inject(UserService);
  private memberRequestId = 0;

  readonly projects = signal<ProjectDto[]>([]);
  readonly users = signal<UserDto[]>([]);
  readonly members = signal<ProjectMemberDto[]>([]);
  readonly selectedProjectId = signal<number | null>(null);
  readonly selectedUserId = signal<number | null>(null);
  readonly loadingProjects = signal(false);
  readonly loadingUsers = signal(false);
  readonly loadingMembers = signal(false);
  readonly saving = signal(false);
  readonly projectsError = signal('');
  readonly usersError = signal('');
  readonly membersError = signal('');
  readonly actionError = signal('');
  readonly actionMessage = signal('');

  constructor() {
    const projectId = Number(this.route.snapshot.queryParamMap.get('projectId'));
    if (Number.isSafeInteger(projectId) && projectId > 0) {
      this.selectedProjectId.set(projectId);
      this.loadMembers(projectId);
    }
    this.loadProjects();
    this.loadUsers();
  }

  loadProjects(): void {
    this.loadingProjects.set(true);
    this.projectsError.set('');
    this.projectService.getAll({ page: 1, pageSize: 100 })
      .pipe(finalize(() => this.loadingProjects.set(false)))
      .subscribe({
        next: result => this.projects.set(result.items),
        error: (error: HttpErrorResponse) => this.projectsError.set(this.errorMessage(error, 'Could not load projects.'))
      });
  }

  loadUsers(): void {
    this.loadingUsers.set(true);
    this.usersError.set('');
    this.userService.getAll().pipe(finalize(() => this.loadingUsers.set(false))).subscribe({
      next: users => this.users.set(users),
      error: (error: HttpErrorResponse) => this.usersError.set(this.errorMessage(error, 'Could not load users.'))
    });
  }

  onProjectChange(value: number | null): void {
    const projectId = value === null ? null : Number(value);
    this.memberRequestId++;
    this.selectedProjectId.set(projectId);
    this.selectedUserId.set(null);
    this.loadingMembers.set(false);
    this.members.set([]);
    this.membersError.set('');
    this.actionError.set('');
    this.actionMessage.set('');
    if (projectId !== null) this.loadMembers(projectId);
  }

  onUserChange(value: number | null): void {
    this.selectedUserId.set(value === null ? null : Number(value));
    this.actionError.set('');
    this.actionMessage.set('');
  }

  loadMembers(projectId = this.selectedProjectId()): void {
    if (projectId === null) return;
    const requestId = ++this.memberRequestId;
    this.loadingMembers.set(true);
    this.membersError.set('');
    this.memberService.getByProject(projectId)
      .pipe(finalize(() => {
        if (requestId === this.memberRequestId) this.loadingMembers.set(false);
      }))
      .subscribe({
        next: members => {
          if (requestId === this.memberRequestId) this.members.set(members);
        },
        error: (error: HttpErrorResponse) => {
          if (requestId === this.memberRequestId) {
            this.membersError.set(this.errorMessage(error, 'Could not load this project’s team.'));
          }
        }
      });
  }

  assignUser(): void {
    const projectId = this.selectedProjectId();
    const userId = this.selectedUserId();
    if (projectId === null || userId === null || this.saving()) return;
    if (this.members().some(member => member.userId === userId && member.isActive)) {
      this.actionError.set('This user is already assigned to the selected project.');
      return;
    }

    this.saving.set(true);
    this.actionError.set('');
    this.actionMessage.set('');
    this.memberService.addToProject(projectId, userId)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.actionMessage.set('User assigned to the project.');
          this.selectedUserId.set(null);
          this.loadMembers(projectId);
        },
        error: (error: HttpErrorResponse) => this.actionError.set(this.errorMessage(error, 'Could not assign this user to the project.'))
      });
  }

  removeUser(userId: number): void {
    const projectId = this.selectedProjectId();
    if (projectId === null || this.saving()) return;

    this.saving.set(true);
    this.actionError.set('');
    this.actionMessage.set('');
    this.memberService.removeFromProject(projectId, userId)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.actionMessage.set('User removed from the project.');
          this.loadMembers(projectId);
        },
        error: (error: HttpErrorResponse) => this.actionError.set(this.errorMessage(error, 'Could not remove this user from the project.'))
      });
  }

  userDisplayName(userId: number, fallback?: string | null): string {
    const user = this.users().find(item => item.id === userId);
    return user?.displayName?.trim()
      || user?.userName?.trim()
      || user?.email?.trim()
      || fallback?.trim()
      || `User #${userId}`;
  }

  private errorMessage(error: HttpErrorResponse, fallback: string): string {
    if (error.status === 401) return 'Your session may have expired. Please sign in again.';
    if (error.status === 403) return 'Your account does not have permission to manage this project team.';
    const serverMessage = typeof error.error === 'string'
      ? error.error
      : Array.isArray(error.error?.errors)
        ? error.error.errors.join(' ')
        : '';
    return serverMessage || fallback;
  }
}
