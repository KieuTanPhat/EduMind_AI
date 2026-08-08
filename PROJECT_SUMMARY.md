# EduMind AI — Project Summary

## 1. Tổng quan

EduMind AI là website hỗ trợ học tập cá nhân hóa bằng AI. Người dùng có thể tải tài liệu học tập lên hệ thống, sau đó sử dụng AI để tạo tóm tắt, mind map, flashcard, quiz và hỏi đáp theo nội dung tài liệu.

Mục tiêu của project là xây dựng một sản phẩm có kiến trúc rõ ràng, bảo mật, dễ mở rộng và phù hợp để demo đồ án hoặc phát triển thành portfolio backend/full-stack.

## 2. Đối tượng sử dụng

- **User:** quản lý tài khoản, tải và xem tài liệu, tạo nội dung học tập bằng AI, làm quiz, theo dõi tiến độ và nhận đề xuất học tập.
- **Admin:** quản lý người dùng/tài liệu, theo dõi AI usage, storage usage, thống kê hệ thống và giới hạn sử dụng.

## 3. MVP ưu tiên

Phiên bản đầu tiên tập trung vào luồng hoàn chỉnh sau:

```text
Đăng nhập → Upload PDF → Trích xuất nội dung → Tóm tắt → Tạo Mind Map → Lưu kết quả → Dashboard
```

Sau khi MVP ổn định mới mở rộng lần lượt:

1. Flashcard
2. Quiz
3. Q&A với tài liệu
4. Theo dõi tiến độ học tập
5. Cá nhân hóa và đề xuất học tập
6. Admin dashboard
7. Testing, bảo mật nâng cao và deploy

## 4. Tech stack dự kiến

### Backend

- ASP.NET Core 8 Web API
- C# 12
- Clean Architecture
- Entity Framework Core
- SQL Server Express hoặc LocalDB
- JWT Access Token + Refresh Token
- BCrypt để hash password
- FluentValidation
- Serilog
- Hangfire cho xử lý tài liệu/AI chạy nền
- Swagger/OpenAPI

### Frontend

- React
- TypeScript
- Vite
- Ant Design hoặc Tailwind CSS
- TanStack Query
- React Router
- Axios
- React Flow cho Mind Map

### AI và tài liệu

- Google Gemini API, chỉ gọi từ backend
- PDF: thư viện PDF .NET phù hợp, ví dụ PdfPig
- DOCX: Open XML SDK
- TXT: thư viện .NET có sẵn
- AI output dạng JSON có cấu trúc cho Mind Map, Flashcard và Quiz

### Hạ tầng

- Local file storage trong giai đoạn phát triển
- Cloudflare R2 khi deploy
- Docker/Docker Compose nếu cần chuẩn hóa môi trường
- Git/GitHub và có thể thêm GitHub Actions

## 5. Kiến trúc thư mục

```text
EduMind_AI/
├── PROJECT_SUMMARY.md
├── README.md
├── .gitignore
├── backend/
│   ├── src/
│   │   ├── StudyAI.Domain/
│   │   ├── StudyAI.Application/
│   │   ├── StudyAI.Infrastructure/
│   │   ├── StudyAI.Contracts/
│   │   └── StudyAI.Api/
│   └── tests/
│       ├── StudyAI.UnitTests/
│       └── StudyAI.IntegrationTests/
├── frontend/
├── database/
│   ├── migrations/
│   └── scripts/
├── docs/
│   ├── requirements/
│   ├── architecture/
│   └── diagrams/
├── infrastructure/
│   └── docker/
└── storage/
    └── users/
```

### Trách nhiệm các backend project

- `StudyAI.Domain`: entity, value object, enum và business rule cốt lõi; không phụ thuộc framework bên ngoài.
- `StudyAI.Application`: use case, service interface, DTO mapping, validation và application business logic.
- `StudyAI.Infrastructure`: EF Core, SQL Server, repository, file storage, Gemini service, Hangfire và logging.
- `StudyAI.Contracts`: request/response contract dùng chung giữa API và client.
- `StudyAI.Api`: controller, authentication setup, middleware, DI và cấu hình HTTP API.

## 6. Module chức năng chính

1. Authentication và authorization
2. User profile và preferences
3. Document upload, validation, storage và text extraction
4. AI summary
5. AI mind map
6. Flashcard
7. Quiz và chấm điểm
8. Q&A theo tài liệu
9. Learning progress và study session
10. Weak topic detection và recommendation
11. User dashboard
12. Admin dashboard

## 7. Database dự kiến

Các nhóm bảng chính:

- **Identity:** `Users`, `Roles`, `UserRoles`, `UserPreferences`
- **Document:** `Documents`, `DocumentCategories`
- **AI result:** `Summaries`, `MindMaps`, `MindMapNodes`, `Flashcards`
- **Quiz:** `Quizzes`, `Questions`, `Options`, `QuizResults`, `UserAnswers`
- **Learning:** `LearningProgress`, `StudySessions`, `UserTopics`, `Recommendations`
- **Q&A:** `ChatSessions`, `ChatMessages`

## 8. Nguyên tắc kỹ thuật quan trọng

- Không gọi Gemini API trực tiếp từ React; API key chỉ nằm ở backend.
- Kiểm tra extension, MIME type, kích thước và tên file upload.
- Kiểm tra ownership để user chỉ truy cập tài liệu của chính mình.
- Dùng structured JSON output và validate trước khi lưu kết quả AI.
- Tác vụ trích xuất tài liệu và gọi AI nên chạy nền khi thời gian xử lý dài.
- Có rate limit cho API AI và giới hạn file/storage theo user.
- Xây dựng từ MVP nhỏ, kiểm thử từng module trước khi mở rộng.

## 9. Lộ trình triển khai

```text
Phân tích yêu cầu
    ↓
ERD + Database
    ↓
Backend Clean Architecture
    ↓
Authentication
    ↓
Document CRUD
    ↓
Upload + Extract Text
    ↓
Frontend cơ bản
    ↓
AI Summary
    ↓
AI Mind Map
    ↓
Flashcard + Quiz
    ↓
Q&A + Learning Progress
    ↓
Cá nhân hóa
    ↓
Admin + Testing + Deploy
```

## 10. Trạng thái hiện tại

- Đã tạo cấu trúc thư mục và solution `StudyAI.sln`.
- Đã dựng backend Clean Architecture với Domain, Application, Infrastructure, Contracts và API.
- Đã có EF Core SQL Server model, migration `InitialCreate`, migration seed role và database seeder.
- Đã triển khai authentication nền tảng: register, login, refresh token, logout, `/api/auth/me` và role claims.
- Đã thêm validation pipeline, ProblemDetails exception handling, Serilog, Swagger, CORS, Hangfire configuration và local storage abstraction.
- Đã thêm test cho domain behavior, validation, BCrypt/JWT và health controller.
- Bước tiếp theo: Document CRUD + local storage + file validation, sau đó text extraction.

## 11. Quy ước tên

- Tên backend project: `StudyAI.*` theo tài liệu gốc.
- Tên sản phẩm/repository: `EduMind_AI`.
- API route dùng tiền tố `/api`.
- Entity và table dùng PascalCase; route dùng kebab-case hoặc plural nhất quán.
