import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ServiceResult } from '../../shared/models/api-response';
import { ApiClientService } from '../../shared/services/api-client.service';

export interface ProjectDto {
  id: number;
  name: string;
  description?: string | null;
  status?: string;
  startDate?: string | null;
  endDate?: string | null;
}

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private readonly api = inject(ApiClientService);
  getAll(): Observable<ProjectDto[]> {
    return this.api.get<ServiceResult<ProjectDto[]>>('Projects').pipe(
      map(result => {
        if (!result.succeeded) throw new Error(result.errors?.join(', ') || 'Unable to load projects.');
        return result.data ?? [];
      })
    );
  }
}
