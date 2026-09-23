# Waybon API

Waybon API is the backend for the Waybon platform: an ASP.NET Core 10 Web API built with PostgreSQL (hosted on Supabase) and Entity Framework Core. The solution follows Clean Architecture, splitting domain rules, application use cases, infrastructure implementations, and the HTTP layer into independent projects so each one can evolve and be tested on its own.

It currently exposes role and user management as its first modules, with authentication and additional business endpoints planned next. The API is deployed on Render and is meant to power two future clients: an admin web panel for managing the service, and a .NET MAUI mobile app.

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core)
[![C#](https://img.shields.io/badge/C%23-14.0-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![Render](https://img.shields.io/badge/Deploy-Render-46E3B7?logo=render&logoColor=111111)](https://render.com/)

> **Status:** initial stage. Role and user management are done. Authentication and other business endpoints come next.

---

## 🏗️ Architecture

Dependencies point inward:

```text
Waybon.Api  →  Waybon.Application  →  Waybon.Domain
Waybon.Api  →  Waybon.Infrastructure  →  Waybon.Application / Domain
```

<!-- markdownlint-disable MD033 -->
<details>
<summary>Layer responsibilities</summary>

---

| Layer          | Responsibility                                                        |
| -------------- | --------------------------------------------------------------------- |
| Domain         | Business entities, rules, and contracts with no project dependencies. |
| Application    | Use cases and application services that depend only on the domain.    |
| Infrastructure | Persistence and external service implementations.                     |
| API            | HTTP composition root, configuration, and dependency injection.       |

</details>

---

## 📁 Project Structure

```text
waybon-api/
├── src/
│   ├── Waybon.Api              # HTTP entry point
│   ├── Waybon.Application      # Use cases and application services
│   ├── Waybon.Domain           # Core business rules
│   └── Waybon.Infrastructure   # External implementations
└── scripts/
    └── Waybon.Seeder           # Console tool that seeds the base roles and the default role
```

`Waybon.Seeder` is excluded from the Docker image (the `Dockerfile` only builds `Waybon.Api.csproj`) — it's a local/operational tool only. The initial migration creates the `role`, `user`, `user_credential`, and `session` tables with their relationships and unique indexes.

---

## 🔌 API Endpoints

`/api/roles` — name must be unique, 3-25 characters. One role is marked as the default (`isDefault`); it's assigned to every new user and can't be deleted:

| Method   | Endpoint          | Description             |
| -------- | ----------------- | ----------------------- |
| `GET`    | `/api/roles`      | List all roles          |
| `GET`    | `/api/roles/{id}` | Get a role by ID        |
| `POST`   | `/api/roles`      | Create a new role       |
| `PUT`    | `/api/roles/{id}` | Update an existing role |
| `DELETE` | `/api/roles/{id}` | Delete a role           |

```json
{ "name": "admin" }
```

`/api/users` — email must be unique; password must be 8-128 characters. New accounts are always created with the default role:

| Method   | Endpoint          | Description                      |
| -------- | ----------------- | -------------------------------- |
| `GET`    | `/api/users`      | List all users                   |
| `GET`    | `/api/users/{id}` | Get a user by ID                 |
| `POST`   | `/api/users`      | Register a new user              |
| `PATCH`  | `/api/users/{id}` | Update the username and/or email |
| `DELETE` | `/api/users/{id}` | Delete a user                    |

```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "supersecret123"
}
```

Errors follow the [ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457) format: every error response includes `status`, `title`, and `detail`, plus `errors` for validation failures.

You can try the endpoints with Postman or any similar HTTP client.

---

## Requirements

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download/dotnet/10.0)
- A [Supabase](https://supabase.com/) project (PostgreSQL database)
- EF Core CLI: `dotnet tool install --global dotnet-ef --version 10.0.4`

---

<a id="local-setup"></a>

## ⚙️ Local Setup

1. **Clone and restore**

   ```powershell
   git clone https://github.com/Meyzov/waybon-api.git
   cd waybon-api
   dotnet restore Waybon.slnx
   ```

2. **Set your Supabase connection string.** Use the **session pooler** string from Supabase (`*.pooler.supabase.com`, port `5432`) and add `Maximum Pool Size` at the end. The API and the seed script each keep their own secrets, so set it for both:

   ```powershell
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_SUPABASE_CONNECTION_STRING;Maximum Pool Size=20" --project src/Waybon.Api/Waybon.Api.csproj

   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_SUPABASE_CONNECTION_STRING;Maximum Pool Size=5" --project scripts/Waybon.Seeder/Waybon.Seeder.csproj
   ```

3. **Apply the database migrations:**

   ```powershell
   dotnet ef database update --project src/Waybon.Infrastructure/Waybon.Infrastructure.csproj --startup-project src/Waybon.Api/Waybon.Api.csproj
   ```

4. **Seed the base roles** (`admin`, `user`) and mark `user` as the default role — safe to run more than once:

   ```powershell
   dotnet run --project scripts/Waybon.Seeder
   ```

5. **Run the API:**

   ```powershell
   dotnet run --project src/Waybon.Api/Waybon.Api.csproj
   ```

6. **Confirm it works** — `GET /api/roles` should return the `admin` and `user` roles, with `user` marked as `"isDefault": true`.

---

## 🚀 Deploy to Render

1. Create a **Web Service**, connect the GitHub repo, and set:

   | Setting              | Value        |
   | -------------------- | ------------ |
   | Runtime              | Docker       |
   | Dockerfile Path      | `Dockerfile` |
   | Docker Build Context | `.`          |
   | Start Command        | Leave empty  |

2. Add the environment variable `ConnectionStrings__DefaultConnection` with the Supabase session pooler connection string, ending in `;Maximum Pool Size=20`.
3. Deploy.
4. The image doesn't run migrations or the seed script automatically — from your machine, pointed at the same Supabase database, run the migration and seed commands from [Local Setup](#local-setup) (steps 3 and 4) once.

---

<details>
<summary><strong>Creating a New Migration</strong></summary>

Only after changing entities or EF Core configurations:

```powershell
dotnet ef migrations add MigrationName `
    --project src/Waybon.Infrastructure/Waybon.Infrastructure.csproj `
    --startup-project src/Waybon.Api/Waybon.Api.csproj `
    --output-dir Persistence/Migrations
```

</details>

<details>
<summary><strong>Run with Docker</strong></summary>

```powershell
docker build -t waybon-api .
docker run --rm -p 8080:8080 -e ConnectionStrings__DefaultConnection="YOUR_SUPABASE_CONNECTION_STRING" waybon-api
```

The API is then available at `http://localhost:8080`.

</details>
<!-- markdownlint-enable MD033 -->