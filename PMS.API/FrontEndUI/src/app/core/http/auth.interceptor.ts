import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const token = inject(AuthService).getToken();
  debugger;
  if (!token || request.headers.has('Authorization')) return next(request);

  const apiUrl = new URL(environment.apiUrl);
  const requestUrl = new URL(request.url, apiUrl);
  const apiPath = apiUrl.pathname.replace(/\/+$/, '');
  const isApiRequest = requestUrl.origin === apiUrl.origin
    && requestUrl.pathname.startsWith(`${apiPath}/`);
  const isLoginRequest = requestUrl.pathname.replace(/\/+$/, '').toLowerCase().endsWith('/auth/login');

  if (!isApiRequest || isLoginRequest) return next(request);

  return next(request.clone({
    setHeaders: { Authorization: `Bearer ${token}` }
  }));
};
