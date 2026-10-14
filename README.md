# Project Management System

This repository contains the ASP.NET Core API and the Angular web application for the Project Management System (PMS).

## Repository layout

- `PMS.API/` - ASP.NET Core API, authentication, Swagger, and startup configuration.
- `PMS.Application/` - application services and interfaces.
- `PMS.Domain/` - domain entities and rules.
- `PMS.Infrastructure/` - SQL Server persistence, Identity, repositories, and EF Core migrations.
- `PMS.API/FrontEndUI/` - Angular 22 client.

## Prerequisites

Install the following before starting the application:

- .NET 10 SDK.
- Node.js and npm versions supported by Angular CLI 22.
- SQL Server (for example, SQL Server Express) and a database login with permission to create/use the PMS database.
- Git.

Check the installed tools from PowerShell:

```powershell
dotnet --version
node --version
npm --version
```

## Configure the API

The API reads a SQL Server connection string named `DefaultConnection` and JWT settings from ASP.NET Core configuration. The connection string in `PMS.API/appsettings.json` is specific to one developer's computer; replace it for your own environment. Do not commit machine-specific credentials or production secrets.

From the repository root, initialize .NET user secrets and set a connection string for your SQL Server instance:

```powershell
dotnet user-secrets init --project PMS.API\PMS.API.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\SQLEXPRESS;Database=PMSDatabase;Integrated Security=True;TrustServerCertificate=True;" --project PMS.API\PMS.API.csproj
dotnet user-secrets set "Jwt:Key" "<replace-with-a-unique-secret-at-least-32-bytes-long>" --project PMS.API\PMS.API.csproj
```

Adjust the server/connection options for your SQL Server installation. Keep the JWT issuer and audience consistent with the API configuration; the development defaults are `PMS.API` and `PMS.Angular`.

Create or update the database from the repository root. If the EF Core command-line tool is not installed, install the version matching the repository's EF Core packages:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
dotnet ef database update --project PMS.Infrastructure\PMS.Infrastructure.csproj --startup-project PMS.API\PMS.API.csproj
```

Install and trust the ASP.NET Core HTTPS development certificate when prompted:

```powershell
dotnet dev-certs https --trust
```

## Start the application

Open two PowerShell terminals at the repository root.

### Terminal 1: API

```powershell
dotnet run --project PMS.API\PMS.API.csproj --launch-profile https
```

The HTTPS launch profile listens at `https://localhost:7108` (and HTTP at `http://localhost:5094`). In Development, Swagger UI is available at:

```text
https://localhost:7108/
```

### Terminal 2: Angular frontend

Set the Angular API base URL in `PMS.API/FrontEndUI/src/environments/environment.ts` to match the HTTPS API launch profile:

```typescript
apiUrl: 'https://localhost:7108/api'
```

Then start the frontend:

```powershell
Set-Location PMS.API\FrontEndUI
npm ci
npm start
```

Open `http://localhost:4200`. The API CORS policy currently allows this origin. Leave the API running while using the frontend.

## Sign in and check the API

The database migration `SeedPMSRolesAndAdmin` creates an initial development administrator account:

- Username: `admin`
- Password: `Admin@123`

This is a shared, migration-defined development credential. Change it immediately after local setup and do not use it in a deployed environment. Sign in from the Angular login page. Once authenticated, the dashboard requests `GET /api/Dashboard`; the controller requires authorization.

## Build and test

From the repository root, build the .NET solution:

```powershell
dotnet build ProjectManagementSystem.slnx
```

From `PMS.API\FrontEndUI`, build or test the Angular application:

```powershell
npm run build
npm test
```

## Troubleshooting

- **Database connection errors:** Check that SQL Server is running and that `DefaultConnection` points to the correct server/database. Re-run `dotnet ef database update` after correcting the connection.
- **HTTPS certificate errors:** Run `dotnet dev-certs https --trust`, restart the browser, and use the API URL that matches the selected launch profile.
- **Frontend cannot reach the API:** Confirm the API is running, `environment.ts` points to its base URL with `/api`, and the browser origin is `http://localhost:4200` (the currently allowed CORS origin).
- **401 responses or login failures:** Confirm the database migration has run, use a valid account, and ensure the API's JWT issuer, audience, and key are configured consistently. Restart the API after changing its configuration.
- **Wrong API port:** The standard `https` launch profile uses port `7108`; `44309` is the IIS Express SSL port. Update the frontend `apiUrl` if you start the API with a different profile.
