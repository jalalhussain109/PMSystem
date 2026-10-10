import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ServiceResult } from '../../shared/models/api-response';
import { ApiClientService } from '../../shared/services/api-client.service';

export interface TaskDto {
  id: number;
  title: string;
  status?: string;
  dueDate?: string | null;
  projectName?: string | null;
}

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly api = inject(ApiClientService);
  getAll(): Observable<TaskDto[]> {
    return this.api.get<ServiceResult<TaskDto[]>>('Tasks').pipe(
      map(result => {
        if (!result.succeeded) throw new Error(result.errors?.join(', ') || 'Unable to load tasks.');
        return result.data ?? [];
      })
    );
  }
}
