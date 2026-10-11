import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from '../../shared/services/api-client.service';

export interface ProjectMemberDto {
  id: number;
  projectId: number;
  userId: number;
  role: string;
  isActive: boolean;
  userName?: string | null;
}

@Injectable({ providedIn: 'root' })
export class ProjectMemberService {
  private readonly api = inject(ApiClientService);

  getByProject(projectId: number): Observable<ProjectMemberDto[]> {
    return this.api.get<ProjectMemberDto[]>(`ProjectMembers/project/${projectId}`);
  }
}
