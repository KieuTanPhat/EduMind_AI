# Phase 3 — Personalization and Production

## Personalization

- `LearningProgress` stores per-user document completion and study minutes.
- Quiz submission updates document progress from the score when it improves the current value.
- `FlashcardReview` records known, review and unknown outcomes.
- `PersonalizationService` applies deterministic rules to generate recommendations:
  - no documents → start the learning library;
  - processed documents without summaries → create a summary;
  - quiz average below 50% → revisit weak material;
  - generated flashcards not yet reviewed → schedule a flashcard session.
- `UserPreference` stores learning level, goal and language for future prompt difficulty adjustment.

## Admin

Admin APIs are protected by `[Authorize(Roles = "Admin")]` and expose users, documents, AI usage summaries, storage usage and system statistics. The Admin role is seeded; assignment is intentionally an explicit SQL operation in `database/scripts/promote-user-to-admin.sql`.

## Production hardening

- AI endpoints use a configurable fixed-window rate limit (`RateLimiting:AiPermitLimit`).
- Local storage remains the default for development.
- `R2FileStorageService` uses the S3-compatible Cloudflare R2 API when `Storage:Provider` is `R2`; credentials are read from environment variables (`R2_ENDPOINT`, `R2_BUCKET`, `R2_ACCESS_KEY_ID`, `R2_SECRET_ACCESS_KEY`).
- Docker Compose runs SQL Server, API and the React/Nginx frontend with persistent volumes.
- GitHub Actions runs backend build/tests and frontend lint/build for pushes and pull requests.

## Operational requirements

Before production deployment, set a secret manager-backed JWT key, Gemini key, SQL Server connection string, R2 credentials, allowed CORS origins and a non-development ASP.NET environment. Do not use the compose example credentials in production.
