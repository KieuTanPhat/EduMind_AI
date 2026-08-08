# EduMind AI

StudyAI is a personalized AI learning platform for documents, summaries, mind maps, flashcards, quizzes and document Q&A.

Phase 2 core learning platform đã được triển khai end-to-end trên nền tảng Phase 1:

- .NET 8 / C# 12 Web API
- Clean Architecture modular monolith
- EF Core SQL Server Code First and migrations
- JWT access tokens + hashed rotating refresh tokens
- BCrypt password hashing
- FluentValidation and MediatR pipeline behavior
- Serilog request logging
- Swagger with Bearer authentication
- ProblemDetails-compatible exception handling
- Local file storage với ownership, validation, download, delete, search và pagination
- PDF/DOCX/TXT extraction, text cleaning/chunking và document processing status
- Hangfire scheduler (hoặc inline development scheduler khi Hangfire tắt)
- OpenAI (hoặc Gemini) summary, mind map, flashcards, quiz và document-grounded Q&A
- AI structured JSON validation, result caching và AI usage logging
- React/TypeScript dashboard, document library và document detail workspace

Read [PROJECT_SUMMARY.md](PROJECT_SUMMARY.md) for the product and roadmap overview. Architecture notes are in [docs/architecture/phase-1.md](docs/architecture/phase-1.md) and [docs/architecture/phase-2.md](docs/architecture/phase-2.md).

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

In a second terminal, run the frontend:

```powershell
cd frontend
Copy-Item .env.example .env.local
npm install
npm run dev
```

The API exposes Swagger in Development and a health endpoint at `GET /health`. Open the frontend at `http://127.0.0.1:5173`.

Set `OPENAI_API_KEY` to enable AI generation with the default `gpt-5.4-nano` provider. Set `AI:Provider` to `Gemini` and provide `GEMINI_API_KEY` to use Gemini instead. `Hangfire:Enabled` is false by default for a simple local development setup; set it to true when SQL Server-backed Hangfire processing is configured.

Registration uses an image CAPTCHA with rotated characters followed by a 6-digit email OTP. Configure `EMAIL_SMTP_HOST`, `EMAIL_SMTP_PORT`, `EMAIL_SMTP_USERNAME`, `EMAIL_SMTP_PASSWORD`, `EMAIL_FROM_ADDRESS`, and `EMAIL_SMTP_USE_SSL` for real email delivery. For Gmail, use an App Password instead of the normal account password. When SMTP is empty in Development, the OTP is returned to the UI for local testing only.

Local development seeds an administrator account with username `admin` and password `123`. Change or remove this bootstrap credential before deployment.

### Docker Compose

Copy the example environment file, set strong local secrets, then start the full stack:

```powershell
Copy-Item infrastructure/docker/.env.example infrastructure/docker/.env
docker compose --env-file infrastructure/docker/.env -f infrastructure/docker/docker-compose.yml up --build
```

The compose stack exposes the frontend at `http://localhost:5173` and the API at `http://localhost:5194`. It runs SQL Server, automatically applies migrations and persists database/file-storage volumes.

To use another SQL Server instance, set `DATABASE_CONNECTION_STRING` or update the local connection string in `appsettings.json`. Do not commit production secrets. The API accepts `JWT_SECRET` and requires at least 32 bytes.

## Verification

```powershell
dotnet build StudyAI.sln
dotnet test StudyAI.sln
```

The first migration is in `backend/src/StudyAI.Infrastructure/Persistence/Migrations` and should be applied with `dotnet ef database update`; the application does not use `EnsureCreated`.

## Authentication API surface

```text
GET  /api/auth/captcha
POST /api/auth/register
POST /api/auth/verify-otp
POST /api/auth/resend-otp
POST /api/auth/login
```

## Phase 2 API surface

```text
POST   /api/documents
GET    /api/documents?search=&page=1&pageSize=20
GET    /api/documents/{id}
GET    /api/documents/{id}/status
GET    /api/documents/{id}/download
DELETE /api/documents/{id}

POST/GET /api/documents/{id}/summary
POST/GET /api/documents/{id}/mindmap
POST/GET /api/documents/{id}/flashcards
POST/GET /api/documents/{id}/quiz
POST     /api/documents/{id}/chat/sessions
GET      /api/documents/{id}/chat/sessions
POST/GET /api/chat/sessions/{id}/messages
```

## Phase 3 API surface

```text
GET  /api/dashboard
GET  /api/progress
PUT  /api/documents/{id}/progress
GET  /api/recommendations
POST /api/flashcards/{id}/review
GET/PUT /api/preferences

GET  /api/admin/statistics
GET  /api/admin/users
POST /api/admin/users/{id}/deactivate
POST /api/admin/users/{id}/activate
POST /api/admin/users/{id}/plus
DELETE /api/admin/users/{id}
GET  /api/admin/documents
GET  /api/admin/documents/{id}/download
GET  /api/admin/ai-usage
```

Admin access is role-based. After registering a user, use `database/scripts/promote-user-to-admin.sql` to assign the seeded `Admin` role in a controlled environment.
