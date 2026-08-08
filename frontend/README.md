# EduMind AI frontend

React + TypeScript + Vite client for the EduMind AI learning workspace.

## Run locally

```powershell
Copy-Item .env.example .env.local
npm install
npm run dev
```

The default API URL is `http://localhost:5194/api`. Override it with `VITE_API_URL`.

The protected workspace includes the dashboard, document search/upload/delete, processing status polling, download, and document detail tabs for summary, interactive mind map, flashcards, quiz submission and document Q&A.
