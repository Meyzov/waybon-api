# Waybon API

Waybon API is the backend for the Waybon platform: an ASP.NET Core 10 Web API built with PostgreSQL (hosted on Supabase) and Entity Framework Core. The solution follows Clean Architecture, splitting domain rules, application use cases, infrastructure implementations, and the HTTP layer into independent projects so each one can evolve and be tested on its own.

It currently covers account registration, login with opaque session tokens, and email verification, along with role and user management. The API is deployed on Render and is meant to power two future clients: an admin web panel for managing the service, and a .NET MAUI mobile app.

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core)
[![C#](https://img.shields.io/badge/C%23-14.0-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![Render](https://img.shields.io/badge/Deploy-Render-46E3B7?logo=render&logoColor=111111)](https://render.com/)

> **Status:** initial stage. Registration, login and email verification are done. Endpoint authorization and the business features come next.

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

`Waybon.Seeder` is excluded from the Docker image (the `Dockerfile` only builds `Waybon.Api.csproj`) — it's a local/operational tool only. The migrations create the `role`, `user`, `user_credential`, `session`, and `verification_code` tables with their relationships and unique indexes.

---

## 🔌 API Endpoints

### Auth

`/api/v1/auth` — public endpoints for creating an account and signing in:

| Method | Endpoint                         | Description                                             |
| ------ | -------------------------------- | ------------------------------------------------------- |
| `POST` | `/api/v1/auth/register`          | Create an account (no email is sent)                    |
| `POST` | `/api/v1/auth/login`             | Sign in and get a session token                         |
| `POST` | `/api/v1/auth/verification-code` | Send a 6-digit code to the account's email (resend too) |
| `POST` | `/api/v1/auth/verify-email`      | Verify the email with the code and start a session      |

Username must be 3-20 characters, using only letters, numbers, dots (`.`), hyphens (`-`) and underscores (`_`), and doesn't need to be unique; email must be unique (up to 255 characters); password must be 8-128 characters. New accounts are always created with the default role. Registering again with an email that hasn't been verified replaces that account.

```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "supersecret123"
}
```

<details>
<summary>Verification flow</summary>

---

1. `register` creates the account without verifying it.
2. `login` with the correct password answers `403` with `"code": "email_not_verified"` and a `verificationToken` (valid for 1 hour).
3. `verification-code` with that token sends the code by email. The code expires in 15 minutes and allows 5 attempts. Each address can receive up to 5 codes per day, with 60 seconds between them (`429` with a `Retry-After` header otherwise).
4. `verify-email` with the token and the code verifies the account and returns a session token, the same as a successful login.

Each user has a single session: a new login replaces the previous one. Session tokens are random, returned only once, and stored as SHA-256 hashes.

</details>

### Roles

`/api/v1/roles` — name must be unique, 3-15 characters (stored in lowercase). One role is marked as the default (`isDefault`); it's assigned to every new user and can't be deleted:

| Method   | Endpoint             | Description             |
| -------- | -------------------- | ----------------------- |
| `GET`    | `/api/v1/roles`      | List all roles          |
| `GET`    | `/api/v1/roles/{id}` | Get a role by ID        |
| `POST`   | `/api/v1/roles`      | Create a new role       |
| `PUT`    | `/api/v1/roles/{id}` | Update an existing role |
| `DELETE` | `/api/v1/roles/{id}` | Delete a role           |

```json
{ "name": "admin" }
```

### Users

`/api/v1/users` — accounts are created through `/api/v1/auth/register`:

| Method   | Endpoint             | Description                      |
| -------- | -------------------- | -------------------------------- |
| `GET`    | `/api/v1/users`      | List all users                   |
| `GET`    | `/api/v1/users/{id}` | Get a user by ID                 |
| `PATCH`  | `/api/v1/users/{id}` | Update the username and/or email |
| `DELETE` | `/api/v1/users/{id}` | Delete a user                    |

> The roles and users endpoints aren't protected yet; they will require an admin session once endpoint authorization is added.

### General

All endpoints are versioned in the URL (`/api/v1/...`); every response includes an `api-supported-versions` header.

`GET /health` (and the root `/`) returns `Healthy` while the API is running. It isn't versioned and doesn't check the database; Render uses it to know the service is up.

Errors follow the [ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457) format: every error response includes `status`, `title`, and `detail`, plus `errors` for validation failures and `code` for `403` responses.

Logging uses [Serilog](https://serilog.net/): one line per request, with the level based on the status code (`INFO` for 2xx, `WARN` for 4xx, `EROR` for 5xx); successful health checks (`/` and `/health`) are not logged. Log levels are configured in the `Serilog` section of `appsettings.json`.

You can try the endpoints with Postman or any similar HTTP client.

---

## Requirements

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download/dotnet/10.0)
- A [Supabase](https://supabase.com/) project (PostgreSQL database)
- A [Brevo](https://www.brevo.com/) account with a verified sender and an API key (transactional emails)
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

3. **Set your Brevo settings** (API only — the API won't start without them):

   ```powershell
   dotnet user-secrets set "Brevo:ApiKey" "YOUR_BREVO_API_KEY" --project src/Waybon.Api/Waybon.Api.csproj
   dotnet user-secrets set "Brevo:SenderEmail" "YOUR_VERIFIED_SENDER_EMAIL" --project src/Waybon.Api/Waybon.Api.csproj
   dotnet user-secrets set "Brevo:SenderName" "Waybon" --project src/Waybon.Api/Waybon.Api.csproj
   ```

4. **Apply the database migrations:**

   ```powershell
   dotnet ef database update --project src/Waybon.Infrastructure/Waybon.Infrastructure.csproj --startup-project src/Waybon.Api/Waybon.Api.csproj
   ```

5. **Seed the base roles** (`admin`, `user`) and mark `user` as the default role — safe to run more than once:

   ```powershell
   dotnet run --project scripts/Waybon.Seeder
   ```

6. **Run the API:**

   ```powershell
   dotnet run --project src/Waybon.Api/Waybon.Api.csproj
   ```

7. **Confirm it works** — `GET /api/v1/roles` should return the `admin` and `user` roles, with `user` marked as `"isDefault": true`.

---

## 🚀 Deploy to Render

1. Create a **Web Service**, connect the GitHub repo, and set:

   | Setting              | Value        |
   | -------------------- | ------------ |
   | Runtime              | Docker       |
   | Dockerfile Path      | `Dockerfile` |
   | Docker Build Context | `.`          |
   | Start Command        | Leave empty  |
   | Health Check Path    | `/health`    |

2. Add the environment variables:

   | Variable                               | Value                                                             |
   | -------------------------------------- | ----------------------------------------------------------------- |
   | `ConnectionStrings__DefaultConnection` | Supabase session pooler string ending in `;Maximum Pool Size=20`  |
   | `Brevo__ApiKey`                        | Brevo API key (use a different key than the local one)            |
   | `Brevo__SenderEmail`                   | Verified sender email in Brevo                                    |
   | `Brevo__SenderName`                    | `Waybon`                                                          |
   | `Console__ForceColors`                 | `true` — enables colored logs in the Render log viewer (optional) |

3. Deploy.
4. The image doesn't run migrations or the seed script automatically — from your machine, pointed at the same Supabase database, run the migration and seed commands from [Local Setup](#local-setup) (steps 4 and 5) once.

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
docker run --rm -p 8080:8080 `
    -e ConnectionStrings__DefaultConnection="YOUR_SUPABASE_CONNECTION_STRING" `
    -e Brevo__ApiKey="YOUR_BREVO_API_KEY" `
    -e Brevo__SenderEmail="YOUR_VERIFIED_SENDER_EMAIL" `
    -e Brevo__SenderName="Waybon" `
    waybon-api
```

The API is then available at `http://localhost:8080`.

</details>
<!-- markdownlint-enable MD033 -->
