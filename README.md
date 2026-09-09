# Waybon API

ASP.NET Core API built with .NET 10, PostgreSQL, and Entity Framework Core. The solution is organized into separate projects following Clean Architecture principles.

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core)
[![C#](https://img.shields.io/badge/C%23-14.0-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![Render](https://img.shields.io/badge/Deploy-Render-46E3B7?logo=render&logoColor=111111)](https://render.com/)

> **Project status:** initial API stage. The API includes role management endpoints and the initial EF Core migration. User authentication and additional business endpoints will be added as the application grows.

## Contents

- [Waybon API](#waybon-api)
  - [Contents](#contents)
  - [Architecture](#architecture)
  - [Project Structure](#project-structure)
  - [Requirements](#requirements)
  - [Local Setup](#local-setup)
  - [Run Locally](#run-locally)
  - [API Endpoints](#api-endpoints)
  - [Migrations](#migrations)
  - [Docker](#docker)
  - [Deploy to Render](#deploy-to-render)

## Architecture

The solution uses a dependency direction that points inward:

```text
Waybon.Api  →  Waybon.Application  →  Waybon.Domain

Waybon.Api  →  Waybon.Infrastructure  →  Waybon.Application / Domain
```

<!-- markdownlint-disable MD033 -->
<details>
<summary>Layer Responsibilities</summary>

| Layer          | Responsibility                                                        |
| -------------- | --------------------------------------------------------------------- |
| Domain         | Business entities, rules, and contracts with no project dependencies. |
| Application    | Use cases and application services that depend only on the domain.    |
| Infrastructure | Persistence and external service implementations.                     |
| API            | HTTP composition root, configuration, and dependency injection.       |

</details>
<!-- markdownlint-enable MD033 -->

## Project Structure

```text
src/
├── Waybon.Api              # HTTP entry point
├── Waybon.Application      # Use cases and application services
├── Waybon.Domain           # Core business rules
└── Waybon.Infrastructure   # External implementations
```

The infrastructure project contains the EF Core `AppDbContext`, entity configurations, and database migrations. The initial migration creates the `role`, `user`, `user_credential`, and `session` tables with their relationships and unique indexes.

## Requirements

| Requirement                                                         | Notes                                   |
| ------------------------------------------------------------------- | --------------------------------------- |
| [.NET SDK 10.0+](https://dotnet.microsoft.com/download/dotnet/10.0) | Required to build and run the solution. |
| [Docker](https://docs.docker.com/get-docker/)                       | Required for container workflows.       |
| [Supabase](https://supabase.com/) project                           | Provides the PostgreSQL database.       |
| Entity Framework Core CLI                                           | Install with the command below.         |

```powershell
dotnet tool install --global dotnet-ef --version 10.0.4
```

## Local Setup

1. Clone the repository and open its root directory:

   ```powershell
   git clone https://github.com/Meyzov/waybon-api.git
   cd waybon-api
   ```

2. Restore the solution dependencies:

   ```powershell
   dotnet restore Waybon.slnx
   ```

3. Configure the Supabase connection with .NET User Secrets:

   ```powershell
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_SUPABASE_CONNECTION_STRING" --project src/Waybon.Api/Waybon.Api.csproj
   ```

   The connection string must be named `DefaultConnection`. Never commit the real value or database password.

## Run Locally

1. Run the API from the repository root:

   ```powershell
   $env:ASPNETCORE_ENVIRONMENT = "Development"
   dotnet run --project src/Waybon.Api/Waybon.Api.csproj
   ```

2. Check the role API:

   ```text
   GET /api/roles
   ```

   An empty JSON array means the API connected successfully and there are no roles yet.

3. (Optional) Compile the complete solution:

   ```powershell
   dotnet build Waybon.slnx
   ```

## API Endpoints

Role management is available under `/api/roles`:

| Method   | Endpoint          | Description             |
| -------- | ----------------- | ----------------------- |
| `GET`    | `/api/roles`      | List all roles          |
| `GET`    | `/api/roles/{id}` | Get a role by ID        |
| `POST`   | `/api/roles`      | Create a new role       |
| `PUT`    | `/api/roles/{id}` | Update an existing role |
| `DELETE` | `/api/roles/{id}` | Delete a role           |

Create or update a role with a JSON body containing a name between 3 and 25 characters:

```json
{
  "name": "admin"
}
```

The role name must be unique. Validation and conflict errors are returned as JSON responses by the global exception handler.

<!-- markdownlint-disable MD033 -->
<details>
<summary>Example Create Request (PowerShell)</summary>

```powershell
$body = @{ name = "admin" } | ConvertTo-Json

Invoke-RestMethod `
   -Uri http://localhost:5000/api/roles `
   -Method Post `
   -ContentType "application/json" `
   -Body $body
```

Use the returned role ID with `GET`, `PUT`, or `DELETE` requests.

</details>
<!-- markdownlint-enable MD033 -->

## Migrations

Migrations are stored in:

```text
src/Waybon.Infrastructure/Persistence/Migrations/
```

Apply migrations to the configured database:

```powershell
dotnet ef database update `
    --project src/Waybon.Infrastructure/Waybon.Infrastructure.csproj `
    --startup-project src/Waybon.Api/Waybon.Api.csproj
```

<!-- markdownlint-disable MD033 -->
<details>
<summary>(Optional) Create a New Migration</summary>

Only create a new migration after changing entities or EF Core configurations:

```powershell
dotnet ef migrations add MigrationName `
    --project src/Waybon.Infrastructure/Waybon.Infrastructure.csproj `
    --startup-project src/Waybon.Api/Waybon.Api.csproj `
    --output-dir Persistence/Migrations
```

</details>
<!-- markdownlint-enable MD033 -->

## Docker

1. Build the image from the repository root:

   ```powershell
   docker build -t waybon-api .
   ```

2. Run the container:

   ```powershell
   docker run --rm -p 8080:8080 -e ConnectionStrings__DefaultConnection="YOUR_SUPABASE_CONNECTION_STRING" waybon-api
   ```

3. The API will be available at:

   ```text
   http://localhost:8080
   ```

## Deploy to Render

1. Create a new **Web Service** in Render.
2. Connect the GitHub repository.
3. Configure the service:

   | Setting              | Value        |
   | -------------------- | ------------ |
   | Runtime              | Docker       |
   | Dockerfile Path      | `Dockerfile` |
   | Docker Build Context | `.`          |
   | Start Command        | Leave empty  |

4. Add the database connection as an environment variable:

   ```text
   ConnectionStrings__DefaultConnection
   ```

   Use the Supabase PostgreSQL connection string as its value.

5. Deploy the service.
