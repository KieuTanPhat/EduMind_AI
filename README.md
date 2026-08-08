# EduMind AI

StudyAI is a personalized AI learning platform for documents, summaries, mind maps, flashcards, quizzes and document Q&A.

The project currently contains the Phase 1 backend foundation:

- .NET 8 / C# 12 Web API
- Clean Architecture modular monolith
- EF Core SQL Server Code First and migrations
- JWT access tokens + hashed rotating refresh tokens
- BCrypt password hashing
- FluentValidation and MediatR pipeline behavior
- Serilog request logging
- Swagger with Bearer authentication
- ProblemDetails-compatible exception handling
- Hangfire and local file-storage abstractions ready for later phases

Read [PROJECT_SUMMARY.md](PROJECT_SUMMARY.md) for the product and roadmap overview. The Phase 1 architecture notes are in [docs/architecture/phase-1.md](docs/architecture/phase-1.md).

## Repository layout

```text
StudyAI.sln
backend/
  src/
    StudyAI.Domain/
    StudyAI.Application/
    StudyAI.Infrastructure/
    StudyAI.Contracts/
    StudyAI.Api/
  tests/
    StudyAI.Domain.Tests/
    StudyAI.Application.Tests/
    StudyAI.Infrastructure.Tests/
    StudyAI.Api.Tests/
frontend/
database/
docs/
infrastructure/
storage/
```

## Local setup

From `D:\EduMind_AI`:

```powershell
dotnet restore StudyAI.sln
$env:JWT_SECRET = "replace-with-a-local-secret-of-at-least-32-bytes"
dotnet ef database update --project backend/src/StudyAI.Infrastructure/StudyAI.Infrastructure.csproj --startup-project backend/src/StudyAI.Api/StudyAI.Api.csproj
dotnet run --project backend/src/StudyAI.Api/StudyAI.Api.csproj
```

The API exposes Swagger in Development and a health endpoint at `GET /health`.

To use another SQL Server instance, set `DATABASE_CONNECTION_STRING` or update the local connection string in `appsettings.json`. Do not commit production secrets. The API accepts `JWT_SECRET` and requires at least 32 bytes.

## Verification

```powershell
dotnet build StudyAI.sln
dotnet test StudyAI.sln
```

The first migration is in `backend/src/StudyAI.Infrastructure/Persistence/Migrations` and should be applied with `dotnet ef database update`; the application does not use `EnsureCreated`.

