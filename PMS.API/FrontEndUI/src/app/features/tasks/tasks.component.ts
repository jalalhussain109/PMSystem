import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { finalize } from 'rxjs';
import { TaskDto, TaskService } from './task.service';

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
  imports: [DatePipe],
  templateUrl: './tasks.component.html',
  styleUrl: './tasks.component.css'
})
export class TasksComponent {
  private readonly service = inject(TaskService);
  readonly tasks = signal<TaskDto[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly showingSampleData = signal(false);
  constructor() { this.reload(); }
  reload(): void {
    this.loading.set(true); this.error.set(''); this.showingSampleData.set(false);
    this.service.getAll().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: items => {
        const tasks = items ?? [];
        this.showingSampleData.set(tasks.length === 0);
        this.tasks.set(tasks.length === 0 ? SAMPLE_TASKS : tasks);
      },
      error: err => this.error.set(err.status === 401 ? 'Your session may have expired. Please sign in again.' : err.status === 403 ? 'Your account does not have permission to view tasks.' : 'Check the API URL, endpoint, and network connection.')
    });
  }
}
