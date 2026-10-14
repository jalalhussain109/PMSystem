import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const token = auth.getToken();

  const apiUrl = new URL(environment.apiUrl);
  const requestUrl = new URL(request.url, apiUrl);
  const apiPath = apiUrl.pathname.replace(/\/+$/, '');
  const isApiRequest = requestUrl.origin === apiUrl.origin
    && requestUrl.pathname.startsWith(`${apiPath}/`);
  const isLoginRequest = requestUrl.pathname.replace(/\/+$/, '').toLowerCase().endsWith('/auth/login');
  const nextWithAuthHandling = (req: typeof request) => next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse
        && error.status === 401
        && isApiRequest
        && !isLoginRequest) {
        auth.logout();
      }
      return throwError(() => error);
    })
  );

  if (!isApiRequest || isLoginRequest) return nextWithAuthHandling(request);
  if (!token || request.headers.has('Authorization')) return nextWithAuthHandling(request);

  return nextWithAuthHandling(request.clone({
    setHeaders: { Authorization: `Bearer ${token}` }
  }));
};
