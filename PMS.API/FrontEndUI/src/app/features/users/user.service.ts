import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ApiClientService } from '../../shared/services/api-client.service';

export interface UserDto {
  id: number;
  userName: string | null;
  displayName: string | null;
  email: string | null;
  phoneNumber: string | null;
  isActive: boolean;
  createdDate: string;
  createdBy: number | null;
  modifiedDate: string | null;
  modifiedBy: number | null;
}

export interface UpdateUserDto {
  userName: string;
  displayName: string | null;
  email: string | null;
  phoneNumber: string | null;
  isActive: boolean;
}

export interface CreateUserDto {
  userName: string;
  displayName: string | null;
  email: string | null;
  phoneNumber: string | null;
  password: string;
}

interface UserListEnvelope {
  succeeded?: boolean;
  success?: boolean;
  data?: UserDto[] | PagedUserResponse | null;
  errors?: string[];
}

export interface PagedUserResponse {
  items: UserDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export type PagedUserResult = PagedUserResponse;

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly api = inject(ApiClientService);

  getAll(): Observable<UserDto[]> {
    return this.api.get<UserDto[]>('Users');
  }

  getPaged(options: { page: number; pageSize: number }): Observable<PagedUserResult> {
    return this.api.get<UserListEnvelope | null>('Users', {
      page: options.page,
      pageSize: options.pageSize
    }).pipe(
      map(result => {
        if (result == null) {
          throw new Error('The users response is empty.');
        }
        if (result.succeeded === false || result.success === false) {
          throw new Error(result.errors?.join(', ') || 'Unable to load users.');
        }

        const data = result.data;
        if (data == null || Array.isArray(data)) {
          throw new Error('The users response does not contain paged data.');
        }
        if (!Array.isArray(data.items)
          || typeof data.totalCount !== 'number'
          || typeof data.page !== 'number'
          || typeof data.pageSize !== 'number'
          || typeof data.totalPages !== 'number') {
          throw new Error('The users response has an unexpected paged data format.');
        }
        return data;
      })
    );
  }

  create(user: CreateUserDto): Observable<UserDto> {
    return this.api.post<UserDto, CreateUserDto>('Users', user);
  }

  update(id: number, user: UpdateUserDto): Observable<UserDto> {
    return this.api.put<UserDto, UpdateUserDto>(`Users/${id}`, user);
  }
}
