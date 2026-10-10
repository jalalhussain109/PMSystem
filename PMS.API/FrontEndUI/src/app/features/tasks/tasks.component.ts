import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { finalize } from 'rxjs';
import { TaskDto, TaskService } from './task.service';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './tasks.component.html',
  styleUrl: './tasks.component.css'
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
