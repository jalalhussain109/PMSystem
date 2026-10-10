import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ApiClientService } from '../../shared/services/api-client.service';

export interface ProjectDto {
  id: number;
  name: string;
  description?: string | null;
  status?: string;
  startDate?: string | null;
  endDate?: string | null;
}

interface ProjectListEnvelope {
  succeeded?: boolean;
  success?: boolean;
  data?: ProjectDto[] | null;
  errors?: string[];
}

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private readonly api = inject(ApiClientService);
  getAll(): Observable<ProjectDto[]> {
    return this.api.get<ProjectDto[] | ProjectListEnvelope | null>('Projects').pipe(
      map(result => {
        if (result == null) return [];
        if (Array.isArray(result)) return result;

        if (result.succeeded === true || result.success === true) {
          if (result.data == null) return [];
          if (Array.isArray(result.data)) return result.data;
          throw new Error('The projects response data must be an array.');
        }

        if (result.succeeded === false || result.success === false) {
          throw new Error(result.errors?.join(', ') || 'Unable to load projects.');
        }

        throw new Error('The projects response has an unexpected format.');
      })
    );
  }
}
