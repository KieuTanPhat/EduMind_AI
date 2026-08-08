# Phase 1 — Architecture and Authentication

## Scope

Phase 1 establishes the backend foundation and authentication flow. It does not implement document processing or Gemini calls yet.

## Dependency direction

```text
StudyAI.Domain
    ↑
StudyAI.Application ──> StudyAI.Contracts
    ↑
StudyAI.Infrastructure
    ↑
StudyAI.Api
```

The Domain project has no dependency on Infrastructure or API. Controllers send MediatR commands and queries; business rules stay in Application handlers and Domain entities.

## Authentication flow

```text
Register/Login
    ↓
Validate request
    ↓
Load user and roles
    ↓
Verify BCrypt password (login)
    ↓
Create short-lived JWT access token
    ↓
Create refresh token, store only SHA-256 hash
    ↓
Return token pair
```

Refresh token rotation revokes the old token and persists a replacement hash. Logout is idempotent and revokes the supplied refresh token when it exists.

## Database

- EF Core Code First with SQL Server.
- `InitialCreate` creates the complete Phase 1 model for identity, documents, AI results, learning and audit data.
- `SeedSystemRoles` inserts deterministic `User` and `Admin` roles.
- Foreign keys use explicit delete behavior; user-owned data is not exposed across users by design.
- UTC timestamps are used by domain entities and token records.

## API foundation

- `/api/auth/register`
- `/api/auth/login`
- `/api/auth/refresh`
- `/api/auth/logout`
- `/api/auth/me`
- `/health`

Errors use a ProblemDetails-compatible JSON shape with a trace ID. JWT secret, database connection and future AI credentials are configuration/environment concerns, not source-controlled secrets.

## Next phase

Implement document CRUD, file extension/MIME/size/path validation, `IFileStorageService` local storage, ownership authorization and background processing status. The upload request must remain short-running; extraction and AI work belong in Hangfire jobs.
