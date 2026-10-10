import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { finalize } from 'rxjs';
import { TaskDto, TaskService } from './task.service';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [DatePipe],
  template: `
    <div class="page-heading"><div><span class="eyebrow">EXECUTION</span><h2>My tasks</h2><p class="muted">Keep track of the next actions and deadlines.</p></div><button class="primary-button compact" (click)="reload()">↻ Refresh</button></div>
    @if (loading()) { <div class="empty-state"><span class="loader"></span><p>Loading tasks…</p></div> }
    @else if (error()) { <div class="notice error-notice"><strong>Could not load tasks</strong><p>{{ error() }}</p><small>Confirm that GET /api/Tasks is available and the account has Tasks.View permission.</small><button class="secondary-button" (click)="reload()">Try again</button></div> }
    @else if (tasks().length === 0) { <div class="empty-state"><span class="empty-icon">✓</span><h3>You're all caught up</h3><p>No tasks were returned by the API.</p></div> }
    @else { <div class="table-wrap"><table><thead><tr><th>TASK</th><th>PROJECT</th><th>STATUS</th><th>DUE DATE</th></tr></thead><tbody>@for (task of tasks(); track task.id) { <tr><td><strong>{{ task.title }}</strong><small class="table-id">Task #{{ task.id }}</small></td><td>{{ task.projectName || '—' }}</td><td><span class="status-pill">{{ task.status || 'Open' }}</span></td><td>{{ task.dueDate ? (task.dueDate | date:'mediumDate') : '—' }}</td></tr> }</tbody></table></div> }
  `
})
export class TasksComponent {
  private readonly service = inject(TaskService);
  readonly tasks = signal<TaskDto[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  constructor() { this.reload(); }
  reload(): void {
    this.loading.set(true); this.error.set('');
    this.service.getAll().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: items => this.tasks.set(items ?? []),
      error: err => this.error.set(err.status === 401 ? 'Your session may have expired. Please sign in again.' : err.status === 403 ? 'Your account does not have permission to view tasks.' : 'Check the API URL, endpoint, and network connection.')
    });
  }
}
