export type AuthResponse = {
  userId: string
  email: string
  firstName: string
  lastName: string
  roles: string[]
  accessToken: string
  accessTokenExpiresAtUtc: string
  refreshToken: string
  refreshTokenExpiresAtUtc: string
}

export type RegisterResponse = { userId: string; email: string; requiresEmailVerification: boolean; developmentVerificationUrl?: string }

export type CurrentUser = Pick<AuthResponse, 'userId' | 'email' | 'firstName' | 'lastName' | 'roles'> & { isEmailVerified?: boolean; isPlus?: boolean; plusExpiresAtUtc?: string }

export type DocumentItem = {
  id: string
  originalFileName: string
  fileType: string
  fileSizeBytes: number
  status: string
  createdAtUtc: string
  updatedAtUtc?: string
}

export type DocumentDetail = DocumentItem & {
  processingError?: string
  hasExtractedText: boolean
  hasSummary: boolean
  hasMindMap: boolean
  flashcardCount: number
  quizCount: number
}

export type PagedResponse<T> = {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export type Summary = {
  id: string
  documentId: string
  content: string
  model: string
  createdAtUtc: string
  updatedAtUtc?: string
}

export type MindMap = {
  id: string
  documentId: string
  title: string
  model: string
  nodes: { id: string; parentNodeId?: string; label: string; description?: string; depth: number; positionX: number; positionY: number }[]
}

export type Flashcards = {
  documentId: string
  items: { id: string; question: string; answer: string; explanation?: string }[]
}

export type Quiz = {
  id: string
  documentId: string
  title: string
  questions: { id: string; content: string; explanation: string; options: { id: string; text: string; isCorrect: boolean }[] }[]
}

export type QuizResult = {
  attemptId: string
  quizId: string
  score: number
  totalQuestions: number
  percentage: number
  completedAtUtc: string
}

export type Recommendation = { id: string; title: string; description: string; isCompleted: boolean; expiresAtUtc?: string; createdAtUtc: string }
export type Dashboard = { totalDocuments: number; processedDocuments: number; inProgressDocuments: number; completedQuizzes: number; averageQuizPercentage: number; flashcardsReviewed: number; recentDocuments: { id: string; originalFileName: string; fileType: string; status: string; fileSizeBytes: number; createdAtUtc: string }[]; recommendations: Recommendation[] }
export type Progress = { totalDocuments: number; completedDocuments: number; totalStudyMinutes: number; averageCompletionPercentage: number; quizAttempts: number; quizAveragePercentage: number; flashcardsReviewed: number; documents: { documentId: string; originalFileName: string; completionPercentage: number; studyMinutes: number; updatedAtUtc: string }[] }
export type UserPreference = { learningLevel: string; learningGoal: string; preferredLanguage: string }
export type AdminStats = { totalUsers: number; activeUsers: number; totalDocuments: number; processedDocuments: number; storageBytes: number; aiRequestCount: number; aiInputTokens: number; aiOutputTokens: number }
export type AdminUser = { id: string; email: string; firstName: string; lastName: string; isActive: boolean; roles: string[]; createdAtUtc: string }
export type AdminDocument = { id: string; originalFileName: string; ownerEmail: string; fileType: string; status: string; fileSizeBytes: number; createdAtUtc: string }
export type AiUsageSummary = { operation: string; requestCount: number; inputTokens: number; outputTokens: number }
export type PlusRequestAdmin = { id: string; userId: string; email: string; fullName: string; amountVnd: number; transferContent: string; status: string; note?: string; createdAtUtc: string }
export type SupportTicket = { id: string; subject: string; message: string; status: string; adminReply?: string; createdAtUtc: string; resolvedAtUtc?: string }

export type ChatSession = { id: string; documentId: string; title: string; createdAtUtc: string }
export type ChatMessage = { id: string; sessionId: string; role: string; content: string; createdAtUtc: string }
