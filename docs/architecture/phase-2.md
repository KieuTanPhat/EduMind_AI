# Phase 2 — Core Learning Platform

## Scope

Phase 2 delivers the usable document-to-learning-content flow:

```text
Upload → validate → local storage → database metadata → processing job
      → extract PDF/DOCX/TXT → clean/chunk text → processed document
      → Gemini summary/mind map/flashcards/quiz/Q&A → cache + usage log
```

## Backend design

- `DocumentsController` exposes authenticated CRUD, search, pagination, download and status endpoints.
- `UploadDocumentCommand` validates extension, MIME type, size and ownership context before saving the file.
- `LocalFileStorageService` stores files below `storage/users/{userId}/documents` and rejects path traversal.
- `PdfTextExtractor`, `DocxTextExtractor` and `TxtTextExtractor` implement `IDocumentTextExtractor`.
- `TextProcessingService` normalizes whitespace and produces bounded overlapping chunks.
- `DocumentProcessingService` updates `Pending → Processing → Processed/Failed` and runs outside the upload request.
- `HangfireDocumentProcessingScheduler` queues durable jobs when `Hangfire:Enabled=true`; the inline scheduler is a development fallback.
- `GeminiService` is the only Gemini integration. Application handlers depend on `IAiService` and validate/sanitize structured output before persistence.
- Existing summary, mind map, flashcard and quiz results are returned unless `forceRegenerate=true`.
- Every AI generation writes an `AIUsageLog` with operation and token usage; document context and secrets are not logged.

## Frontend design

The React app uses feature-oriented screens within a protected application shell:

- Dashboard: document counts, processing state and quick actions.
- Documents: search, pagination, upload, status and delete.
- Document detail: Summary, Mind Map (`@xyflow/react`), Flashcards, Quiz and Q&A Chat tabs.
- Axios carries the access token; TanStack Query handles server state and polling while a document is processing.

## Local configuration

Required for the full flow:

```powershell
$env:JWT_SECRET = "at-least-32-random-bytes-for-development"
$env:GEMINI_API_KEY = "your-gemini-key"
```

Apply the EF Core migration before using authenticated document endpoints. The frontend API URL is configured with `VITE_API_URL` and defaults to `http://127.0.0.1:5194/api`.

Phase 3 implementation is documented in [phase-3.md](phase-3.md), including personalization, progress, recommendations, admin APIs, rate limiting, Docker/CI and Cloudflare R2-ready storage.
