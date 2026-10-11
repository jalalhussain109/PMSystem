import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ApiClientService } from '../../shared/services/api-client.service';

export interface ProjectDto {
  id: number;
  name: string;
  projectCode?: string;
  description?: string | null;
  status?: string;
  startDate?: string | null;
  endDate?: string | null;
  priority?: number;
  projectManagerId?: number | null;
  memberIds?: number[];
}

export interface CreateProjectDto {
  projectCode: string;
  projectName: string;
  description: string | null;
  startDate: string | null;
  endDate: string | null;
  priority: number;
  projectManagerId: number | null;
  memberIds: number[];
}

export type UpdateProjectDto = CreateProjectDto;

interface ProjectResponse {
  id: number;
  name?: string;
  projectName?: string;
  projectCode?: string;
  description?: string | null;
  status?: string | number;
  startDate?: string | null;
  endDate?: string | null;
  priority?: number;
  projectManagerId?: number | null;
  memberIds?: number[];
}

interface ProjectListEnvelope {
  succeeded?: boolean;
  success?: boolean;
  data?: ProjectResponse[] | null;
  errors?: string[];
}

const PROJECT_STATUSES = ['Draft', 'Planning', 'In Progress', 'On Hold', 'Completed', 'Cancelled'];

function mapProject(project: ProjectResponse): ProjectDto {
  return {
    ...project,
    name: project.name ?? project.projectName ?? '',
    status: typeof project.status === 'number' ? PROJECT_STATUSES[project.status] : project.status
  };
}

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private readonly api = inject(ApiClientService);

  getAll(): Observable<ProjectDto[]> {
    return this.api.get<ProjectResponse[] | ProjectListEnvelope | null>('Projects').pipe(
      map(result => {
        if (result == null) return [];
        if (Array.isArray(result)) return result.map(mapProject);

        if (result.succeeded === true || result.success === true) {
          if (result.data == null) return [];
          if (Array.isArray(result.data)) return result.data.map(mapProject);
          throw new Error('The projects response data must be an array.');
        }

        if (result.succeeded === false || result.success === false) {
          throw new Error(result.errors?.join(', ') || 'Unable to load projects.');
        }

        throw new Error('The projects response has an unexpected format.');
      })
    );
  }

  create(project: CreateProjectDto): Observable<ProjectDto> {
    return this.api.post<ProjectResponse, CreateProjectDto>('Projects', project).pipe(map(mapProject));
  }

  update(id: number, project: UpdateProjectDto): Observable<ProjectDto> {
    return this.api.put<ProjectResponse, UpdateProjectDto>(`Projects/${id}`, project).pipe(map(mapProject));
  }
}
