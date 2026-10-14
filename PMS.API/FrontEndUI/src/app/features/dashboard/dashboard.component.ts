import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiClientService } from '../../shared/services/api-client.service';

interface DashboardData {
  activeProjects: number;
  openTasks: number;
  dueThisWeek: number;
  teamMembers: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent {
  private readonly api = inject(ApiClientService);

  readonly loadingData = signal(false);
  readonly dashboardData = signal<DashboardData | null>(null);
  readonly errorMessage = signal('');

  constructor() {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loadingData.set(true);
    this.errorMessage.set('');

    this.api.get<DashboardData>('Dashboard').pipe(
      finalize(() => this.loadingData.set(false))
    ).subscribe({
      next: data => this.dashboardData.set(data),
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(error.status === 401
          ? 'Your session may have expired. Please sign in again.'
          : 'Could not load dashboard data. Please try again.');
      }
    });
  }
}
