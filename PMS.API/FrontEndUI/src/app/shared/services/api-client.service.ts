import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiClientService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  get<T>(path: string, params?: Record<string, string | number | boolean>): Observable<T> {
    let httpParams = new HttpParams();
    Object.entries(params ?? {}).forEach(([key, value]) => {
      httpParams = httpParams.set(key, String(value));
    });
    return this.http.get<T>(this.url(path), { params: httpParams });
  }

  post<TResponse, TBody>(path: string, body: TBody): Observable<TResponse> {
    return this.http.post<TResponse>(this.url(path), body);
  }

  put<TResponse, TBody>(path: string, body: TBody): Observable<TResponse> {
    return this.http.put<TResponse>(this.url(path), body);
  }

  delete<TResponse>(path: string): Observable<TResponse> {
    return this.http.delete<TResponse>(this.url(path));
  }

  deleteWithBody<TResponse, TBody>(path: string, body: TBody): Observable<TResponse> {
    return this.http.request<TResponse>('DELETE', this.url(path), { body });
  }

  private url(path: string): string {
    return `${this.baseUrl}/${path.replace(/^\/+/, '')}`;
  }
}
