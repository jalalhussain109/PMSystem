import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
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

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly api = inject(ApiClientService);

  getAll(): Observable<UserDto[]> {
    return this.api.get<UserDto[]>('Users');
  }

  create(user: CreateUserDto): Observable<UserDto> {
    return this.api.post<UserDto, CreateUserDto>('Users', user);
  }

  update(id: number, user: UpdateUserDto): Observable<UserDto> {
    return this.api.put<UserDto, UpdateUserDto>(`Users/${id}`, user);
  }
}
