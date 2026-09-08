# Waybon API

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core)
[![C#](https://img.shields.io/badge/C%23-14.0-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![Render](https://img.shields.io/badge/Deploy-Render-46E3B7?logo=render&logoColor=111111)](https://render.com/)

ASP.NET Core API built with .NET 10, PostgreSQL, and Entity Framework Core. The solution is organized into separate projects following Clean Architecture principles.

> **Project status:** foundation stage. The API includes a database connectivity check and the initial EF Core migration. Business endpoints will be added as the application grows.

## Contents

- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Requirements](#requirements)
- [Local Setup](#local-setup)
- [Run Locally](#run-locally)
- [Migrations](#migrations)
- [Docker](#docker)
- [Deploy to Render](#deploy-to-render)

## Architecture

The solution uses a dependency direction that points inward:

```text
Waybon.Api -> Waybon.Application -> Waybon.Domain

Waybon.Api -> Waybon.Infrastructure -> Waybon.Application/Domain
```

<details>
<summary>Layer responsibilities</summary>

- **Domain:** business entities, rules, and contracts with no project dependencies.
- **Application:** use cases and application services that depend only on the domain.
- **Infrastructure:** persistence and external service implementations.
- **API:** HTTP composition root, configuration, and dependency injection.

</details>

## Project Structure

```text
src/
├── Waybon.Api              # HTTP entry point
├── Waybon.Application      # Use cases and application services
├── Waybon.Domain           # Core business rules
└── Waybon.Infrastructure   # External implementations
```

The infrastructure project contains the EF Core `AppDbContext`, entity configurations, and database migrations.

## Requirements

- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0) or later.
- [Docker](https://docs.docker.com/get-docker/) for container workflows.
- A [Supabase](https://supabase.com/) project with a PostgreSQL database.
- Entity Framework Core CLI:

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
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your Supabase connection string>" --project src/Waybon.Api/Waybon.Api.csproj
   ```

   The connection string must be named `DefaultConnection`. Never commit the real value or database password.

## Run Locally

1. Run the API from the repository root:

   ```powershell
   $env:ASPNETCORE_ENVIRONMENT = "Development"
   dotnet run --project src/Waybon.Api/Waybon.Api.csproj
   ```

2. Check the database connection via the root endpoint:

   ```text
   GET /
   ```

   It returns `Funciona` when the database is reachable and `No funciona` otherwise.

3. (Optional) Compile the complete solution:

   ```powershell
   dotnet build Waybon.slnx
   ```

## Migrations

Migrations are stored in:

```text
src/Waybon.Infrastructure/Persistence/Migrations/
```

1. Apply migrations to the configured database:

   ```powershell
   dotnet ef database update `
       --project src/Waybon.Infrastructure/Waybon.Infrastructure.csproj `
       --startup-project src/Waybon.Api/Waybon.Api.csproj
   ```

2. (Optional) Create a new migration — only needed after changing the entities or EF Core configurations:

   ```powershell
   dotnet ef migrations add MigrationName `
       --project src/Waybon.Infrastructure/Waybon.Infrastructure.csproj `
       --startup-project src/Waybon.Api/Waybon.Api.csproj `
       --output-dir Persistence/Migrations
   ```

## Docker

1. Build the image from the repository root:

   ```powershell
   docker build -t waybon-api .
   ```

2. Run the container:

   ```powershell
   docker run --rm -p 8080:8080 -e ConnectionStrings__DefaultConnection="<your Supabase connection string>" waybon-api
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