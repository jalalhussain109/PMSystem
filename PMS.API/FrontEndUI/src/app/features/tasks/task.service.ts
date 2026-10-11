import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ApiClientService } from '../../shared/services/api-client.service';

export interface CreateTaskDto {
  title: string;
  description: string | null;
  assignedTo: number | null;
  priority: number;
  startDate: string | null;
  dueDate: string | null;
}
interface TaskResponse {
  id: number;
  title: string;
  status?: string | number;
  dueDate?: string | null;
  projectName?: string | null;
  projectId?: number;
  parentTaskId?: number | null;
  description?: string | null;
  assignedTo?: number | null;
  priority?: number;
  startDate?: string | null;
  completedDate?: string | null;
}

export interface PagedTaskResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

interface TaskListEnvelope {
  succeeded?: boolean;
  success?: boolean;
  data?: PagedTaskResult<TaskResponse> | TaskResponse[] | null;
  errors?: string[];
}

export interface TaskDto {
  id: number;
  title: string;
  status?: string;
  dueDate?: string | null;
  projectName?: string | null;
  projectId?: number;
  parentTaskId?: number | null;
  description?: string | null;
  assignedTo?: number | null;
  priority?: number;
  startDate?: string | null;
  completedDate?: string | null;
}

const Task_STATUSES = ['Draft', 'Planning', 'In Progress', 'On Hold', 'Completed', 'Cancelled'];

function mapTask(task: TaskResponse): TaskDto {
  return {
    ...task,
    //title: task.title ?? task.projectName ?? '',
    status: typeof task.status === 'number' ? Task_STATUSES[task.status] : task.status
  };
}
@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly api = inject(ApiClientService);
  getAll(options: {
    page: number;
    pageSize: number;
    search?: string;
    projectId?: number;
    startFrom?: string;
    dueTo?: string;
  }): Observable<PagedTaskResult<TaskDto>> {
    const params: Record<string, string | number | boolean> = {
      page: options.page,
      pageSize: options.pageSize
    };
    if (options.search) params['search'] = options.search;
    if (options.projectId !== undefined) params['projectId'] = options.projectId;
    if (options.startFrom) params['startFrom'] = options.startFrom;
    if (options.dueTo) params['dueTo'] = options.dueTo;

    return this.api.get<TaskListEnvelope | null>('Tasks', params).pipe(
      map(result => {
        if (result == null) {
          throw new Error('The tasks response is empty.');
        }
        const envelope = result as TaskListEnvelope;
        if (envelope.succeeded === false || envelope.success === false) {
          throw new Error(envelope.errors?.join(', ') || 'Unable to load tasks.');
        }

        const data = envelope.data;
        if (data == null) {
          throw new Error('The tasks response does not contain paged data.');
        }

        if (Array.isArray(data)) {
          const items = data.map(mapTask);
          return {
            items,
            totalCount: items.length,
            page: options.page,
            pageSize: options.pageSize,
            totalPages: items.length ? 1 : 0
          };
        }

        if (!Array.isArray(data.items)
          || typeof data.totalCount !== 'number'
          || typeof data.page !== 'number'
          || typeof data.pageSize !== 'number'
          || typeof data.totalPages !== 'number') {
          throw new Error('The tasks response has an unexpected paged data format.');
        }

        return {
          ...data,
          items: data.items.map(mapTask)
        };
      })
    );
  }

  getById(id: number){
    return this.api.get<TaskDto>(`tasks/${id}`);
  }

  create(projectId: number, task: CreateTaskDto){
    return this.api.post<TaskDto, CreateTaskDto>(`projects/${projectId}/tasks`, task);
  }

  createSubtask(parentTaskId: number, task: CreateTaskDto) {
    return this.api.post<TaskDto, CreateTaskDto>(`tasks/${parentTaskId}/subtasks`, task);
  }

  update(id: number, task: CreateTaskDto) {
    return this.api.put<TaskDto, CreateTaskDto>(`tasks/${id}`, task);
  }
}
