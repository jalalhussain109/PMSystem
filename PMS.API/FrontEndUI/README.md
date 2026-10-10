# PMS Angular 22 Starter

A feature-based Angular standalone starter for a Project Management System. It includes lazy-loaded routes, a shared API client, JWT interceptor, authentication guard, permission guard, login page, dashboard, and Projects/Tasks/Users starter screens.

## Requirements
- Node.js version supported by your installed Angular CLI 22
- .NET PMS API running and reachable from the browser

## Run

```bash
npm install
npm start
```

Open the local URL printed by Angular CLI.

## Configure the API

Edit `src/environments/environment.ts`:

```ts
apiUrl: 'https://localhost:7001/api'
```

Set it to your ASP.NET Core API base URL. The sample assumes these endpoints:
- `POST /api/Auth/login` returns `{ "token": "..." }`
- `GET /api/Projects` returns your `ServiceResult<ProjectDto[]>` response
- `GET /api/Tasks` returns your `ServiceResult<TaskDto[]>` response

The Projects and Tasks services expect the standard wrapper `{ succeeded, notFound, data, errors }`. Adjust endpoint names and DTO property names to match your API.

## JWT claims

The permission guard expects `permission` claims such as `Projects.View` and `Tasks.View`, and the admin navigation expects a `role` claim with `Admin`. These must match the claim names and values emitted by the backend. Angular guards only control navigation/UI; always enforce authorization on the API as well.

## Loose-coupling conventions
- Components depend on feature services, not `HttpClient`.
- Feature services use the shared `ApiClientService`.
- API base URL is configured in one environment file.
- Authentication is isolated in `AuthService` and `authInterceptor`.
- Feature pages are lazy-loaded and do not import one another.
- Shared DTOs/models are kept outside feature components.

## Security note
This starter stores the access token in `sessionStorage` for simplicity. JavaScript-accessible storage is vulnerable if an XSS issue exists. For production, consider a backend-for-frontend with Secure, HttpOnly cookies, or a carefully designed access-token/refresh-token strategy. Do not treat client-side role/permission checks as a security boundary.
