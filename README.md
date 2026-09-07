# Waybon API

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core)
[![C%23](https://img.shields.io/badge/C%23-14.0-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![Render](https://img.shields.io/badge/Deploy-Render-46E3B7?logo=render&logoColor=111111)](https://render.com/)

API built with ASP.NET Core and .NET 10, organized into separate projects.

> **Project status:** foundation stage. The application is configured for development and deployment, but no HTTP endpoints have been implemented yet.

## Contents

- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Requirements](#requirements)
- [Run Locally](#run-locally)
- [Build](#build)
- [Run with Docker](#run-with-docker)
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
├── Waybon.Api             # HTTP entry point
├── Waybon.Application     # Use cases and application services
├── Waybon.Domain          # Core business rules
└── Waybon.Infrastructure  # External implementations
```

## Requirements

- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0) or later.
- [Docker](https://docs.docker.com/get-docker/) for container workflows.

## Run Locally

From the repository root:

```powershell
dotnet restore Waybon.slnx
dotnet run --project src/Waybon.Api/Waybon.Api.csproj
```

The application starts the HTTP host without registered endpoints at this stage.

## Build

```powershell
dotnet build Waybon.slnx
```

## Run with Docker

Build the image from the repository root:

```powershell
docker build -t waybon-api .
```

Run the container locally:

```powershell
docker run --rm -p 8080:8080 -e PORT=8080 waybon-api
```

The container listens on `http://localhost:8080`.

## Deploy to Render

Create a new **Web Service** in Render and connect the GitHub repository with these settings:

| Setting | Value |
| --- | --- |
| Runtime | Docker |
| Dockerfile path | `./Dockerfile` |
| Docker build context | `.` |
| Start command | Leave empty |

Render provides the `PORT` environment variable automatically. The Docker entrypoint uses it to bind the application to the assigned port.