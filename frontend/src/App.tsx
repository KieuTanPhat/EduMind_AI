import { useEffect, useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Alert, Avatar, Badge, Button, Card, Col, Empty, Flex, Input, Layout, List, Menu, Modal, Progress, Radio, Row, Select, Skeleton, Space, Statistic, Tag, Tabs, Typography, Upload, message } from 'antd'
import { AppstoreOutlined, BookOutlined, CloudUploadOutlined, FilePdfOutlined, FileTextOutlined, LogoutOutlined, RobotOutlined, SearchOutlined, SettingOutlined, ThunderboltFilled } from '@ant-design/icons'
import { Position, ReactFlow, type Edge, type Node } from '@xyflow/react'
import '@xyflow/react/dist/style.css'
import { Link, Navigate, Route, Routes, useLocation, useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { api } from './lib/api'
import { useAuth } from './auth/AuthContext'
import type { ChatMessage, ChatSession, DocumentDetail, DocumentItem, Flashcards, MindMap, PagedResponse, Quiz, QuizResult, Summary } from './types'
import './App.css'

const { Header, Sider, Content } = Layout

function formatBytes(bytes: number) { return `${(bytes / 1024 / 1024).toFixed(1)} MB` }
function formatDate(value: string) { return new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(value)) }
function statusColor(status: string) { return status === 'Processed' ? 'success' : status === 'Failed' ? 'error' : status === 'Processing' ? 'processing' : 'default' }
function errorMessage(error: any) { const data = error?.response?.data; const validationErrors = data?.errors ? Object.values(data.errors).flat().join(' ') : ''; return validationErrors || data?.detail || 'Không thể kết nối máy chủ. Hãy kiểm tra API.' }

function Protected({ children }: { children: ReactNode }) {
  return useAuth().isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />
}

function AuthPage({ mode }: { mode: 'login' | 'register' }) {
  const { isAuthenticated, login, register } = useAuth()
  const navigate = useNavigate()
  const isRegister = mode === 'register'
  const [error, setError] = useState('')
  const schema = isRegister
    ? z.object({ email: z.string().email('Email không hợp lệ'), password: z.string().min(8, 'Mật khẩu tối thiểu 8 ký tự').regex(/[A-Z]/, 'Mật khẩu phải có chữ hoa').regex(/[a-z]/, 'Mật khẩu phải có chữ thường').regex(/[0-9]/, 'Mật khẩu phải có chữ số'), firstName: z.string().min(1, 'Bắt buộc'), lastName: z.string().min(1, 'Bắt buộc') })
    : z.object({ email: z.string().email('Email không hợp lệ'), password: z.string().min(1, 'Bắt buộc'), firstName: z.string().optional(), lastName: z.string().optional() })
  const form = useForm<any>({ resolver: zodResolver(schema), defaultValues: { email: '', password: '', firstName: '', lastName: '' } })

  if (isAuthenticated) return <Navigate to="/" replace />

  async function submit(values: any) {
    try {
      setError('')
      if (isRegister) await register(values.email, values.password, values.firstName, values.lastName)
      else await login(values.email, values.password)
      navigate('/')
    } catch (requestError) { setError(errorMessage(requestError)) }
  }

  return <div className="auth-page">
    <div className="auth-art"><div className="orb orb-a" /><div className="orb orb-b" /><div className="auth-brand brand-font"><span className="brand-mark">✦</span> EduMind AI</div><div className="auth-copy"><Typography.Title>Học sâu hơn.<br /><span>Nhớ lâu hơn.</span></Typography.Title><Typography.Paragraph>Biến mọi tài liệu thành lộ trình học riêng dành cho bạn với sức mạnh của AI.</Typography.Paragraph><div className="auth-pills"><span>✦ Tóm tắt thông minh</span><span>⌁ Mind map trực quan</span><span>◈ Quiz cá nhân hóa</span></div></div></div>
    <div className="auth-form-wrap"><div className="auth-form"><Typography.Text className="eyebrow">WELCOME BACK</Typography.Text><Typography.Title level={2}>{isRegister ? 'Tạo tài khoản mới' : 'Chào mừng trở lại'}</Typography.Title><Typography.Paragraph type="secondary">{isRegister ? 'Bắt đầu hành trình học tập thông minh.' : 'Tiếp tục hành trình học tập của bạn.'}</Typography.Paragraph>{error && <Alert showIcon type="error" message={error} className="form-alert" />}<form onSubmit={form.handleSubmit(submit)}>{isRegister && <Row gutter={12}><Col span={12}><Field label="Họ" name="firstName" register={form.register} error={form.formState.errors.firstName?.message} placeholder="Nguyễn" /></Col><Col span={12}><Field label="Tên" name="lastName" register={form.register} error={form.formState.errors.lastName?.message} placeholder="An" /></Col></Row>}<Field label="Email" name="email" register={form.register} error={form.formState.errors.email?.message} placeholder="you@example.com" type="email" /><Field label="Mật khẩu" name="password" register={form.register} error={form.formState.errors.password?.message} placeholder="Tối thiểu 8 ký tự" type="password" /><Button type="primary" htmlType="submit" size="large" block loading={form.formState.isSubmitting}>{isRegister ? 'Tạo tài khoản' : 'Đăng nhập'} <span>→</span></Button></form><div className="auth-switch">{isRegister ? 'Đã có tài khoản?' : 'Chưa có tài khoản?'} <Link to={isRegister ? '/login' : '/register'}>{isRegister ? 'Đăng nhập' : 'Đăng ký miễn phí'}</Link></div></div></div>
  </div>
}

function Field({ label, name, register, error, placeholder, type = 'text' }: { label: string; name: string; register: any; error?: any; placeholder: string; type?: string }) {
  return <label>{label}<input {...register(name)} type={type} placeholder={placeholder} />{error && <small>{String(error)}</small>}</label>
}

function AppShell() {
  const { user, logout } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()
  const menuItems = [{ key: '/', icon: <AppstoreOutlined />, label: 'Tổng quan' }, { key: '/documents', icon: <BookOutlined />, label: 'Tài liệu của tôi' }]
  return <Layout className="app-layout"><Sider breakpoint="lg" collapsedWidth="0" className="app-sider"><div className="logo brand-font"><span className="brand-mark">✦</span> EduMind <em>AI</em></div><div className="sider-label">WORKSPACE</div><Menu mode="inline" selectedKeys={[location.pathname.startsWith('/documents') ? '/documents' : '/']} items={menuItems} onClick={({ key }) => navigate(key)} /><div className="sider-bottom"><div className="plan-card"><ThunderboltFilled /><div><b>AI Learning Plan</b><span>Free plan · 8/20 uses</span></div></div><Button type="text" icon={<SettingOutlined />} onClick={() => message.info('Cài đặt sẽ có trong Phase 3')}>Cài đặt</Button></div></Sider><Layout><Header className="app-header"><Typography.Text className="header-caption">PERSONAL LEARNING SPACE</Typography.Text><Space><Badge dot color="#22c55e" offset={[-3, 3]}><Avatar style={{ background: '#dbeafe', color: '#2563eb' }}>{user?.firstName?.[0] ?? 'U'}</Avatar></Badge><Typography.Text strong>{user?.firstName} {user?.lastName}</Typography.Text><Button type="text" icon={<LogoutOutlined />} onClick={logout} /></Space></Header><Content className="app-content"><Routes><Route path="/" element={<DashboardPage />} /><Route path="/documents" element={<DocumentsPage />} /><Route path="/documents/:id" element={<DocumentDetailPage />} /></Routes></Content></Layout></Layout>
}

function DashboardPage() {
  const { user } = useAuth()
  const { data, isLoading } = useQuery({ queryKey: ['documents', 'dashboard'], queryFn: () => api.get<PagedResponse<DocumentItem>>('/documents', { params: { page: 1, pageSize: 5 } }).then(r => r.data) })
  const docs = data?.items ?? []
  return <div className="page"><div className="hero-row"><div><Typography.Text className="eyebrow">YOUR LEARNING HUB</Typography.Text><Typography.Title>Xin chào, {user?.firstName} <span className="wave">👋</span></Typography.Title><Typography.Paragraph>Hôm nay bạn muốn khám phá điều gì?</Typography.Paragraph></div><Link to="/documents"><Button type="primary" icon={<CloudUploadOutlined />} size="large">Tải tài liệu mới</Button></Link></div><Row gutter={[16, 16]} className="stats-row"><Col xs={24} sm={8}><Card><Statistic title="Tổng tài liệu" value={data?.totalCount ?? 0} prefix={<BookOutlined />} /></Card></Col><Col xs={24} sm={8}><Card><Statistic title="Đã xử lý" value={docs.filter(d => d.status === 'Processed').length} prefix={<RobotOutlined />} /></Card></Col><Col xs={24} sm={8}><Card><Statistic title="Tiến độ tuần này" value={0} suffix="%" prefix={<ThunderboltFilled />} /></Card></Col></Row><Row gutter={[20, 20]}><Col xs={24} lg={15}><Card title="Tài liệu gần đây" extra={<Link to="/documents">Xem tất cả →</Link>} className="content-card">{isLoading ? <Skeleton active /> : docs.length === 0 ? <Empty description="Chưa có tài liệu" /> : <List dataSource={docs} renderItem={doc => <List.Item actions={[<Link key="open" to={`/documents/${doc.id}`}>Mở</Link>]}><List.Item.Meta avatar={<span className="file-icon"><FileTextOutlined /></span>} title={doc.originalFileName} description={`${formatBytes(doc.fileSizeBytes)} · ${formatDate(doc.createdAtUtc)}`} /><Tag color={statusColor(doc.status)}>{doc.status}</Tag></List.Item>} />}</Card></Col><Col xs={24} lg={9}><Card className="focus-card"><Typography.Text className="eyebrow">GỢI Ý HÔM NAY</Typography.Text><Typography.Title level={3}>Xây dựng thói quen học tập</Typography.Title><Typography.Paragraph type="secondary">Tải tài liệu đầu tiên để AI giúp bạn tóm tắt, tạo mind map và thiết kế quiz ôn tập.</Typography.Paragraph><Progress percent={0} showInfo={false} strokeColor="#3b82f6" /><Typography.Text type="secondary">Sẵn sàng bắt đầu</Typography.Text></Card></Col></Row></div>
}

function DocumentsPage() {
  const queryClient = useQueryClient()
  const [search, setSearch] = useState('')
  const [uploading, setUploading] = useState(false)
  const { data, isLoading } = useQuery({ queryKey: ['documents', search], queryFn: () => api.get<PagedResponse<DocumentItem>>('/documents', { params: { search, page: 1, pageSize: 20 } }).then(r => r.data) })

  async function uploadFile(file: File) {
    const form = new FormData(); form.append('file', file); setUploading(true)
    try { await api.post('/documents', form, { headers: { 'Content-Type': 'multipart/form-data' } }); message.success('Đã tải tài liệu. Hệ thống đang xử lý.'); queryClient.invalidateQueries({ queryKey: ['documents'] }) } catch (error) { message.error(errorMessage(error)) } finally { setUploading(false) }
  }

  function remove(id: string) {
    Modal.confirm({ title: 'Xóa tài liệu này?', content: 'Các kết quả AI liên quan cũng sẽ bị xóa.', okText: 'Xóa', okType: 'danger', cancelText: 'Hủy', onOk: async () => { await api.delete(`/documents/${id}`); queryClient.invalidateQueries({ queryKey: ['documents'] }); message.success('Đã xóa tài liệu') } })
  }

  return <div className="page"><div className="hero-row"><div><Typography.Text className="eyebrow">YOUR LIBRARY</Typography.Text><Typography.Title level={1}>Tài liệu của tôi</Typography.Title><Typography.Paragraph type="secondary">Mỗi tài liệu là một bước tiến trong hành trình học tập.</Typography.Paragraph></div><Upload accept=".pdf,.docx,.txt" showUploadList={false} beforeUpload={file => { void uploadFile(file); return false }}><Button type="primary" icon={<CloudUploadOutlined />} size="large" loading={uploading}>Tải tài liệu</Button></Upload></div><Card className="content-card"><Flex gap={12} className="toolbar" wrap><Input prefix={<SearchOutlined />} placeholder="Tìm kiếm tài liệu..." value={search} onChange={event => setSearch(event.target.value)} allowClear /><Select defaultValue="all" options={[{ value: 'all', label: 'Tất cả loại file' }, { value: 'Pdf', label: 'PDF' }, { value: 'Docx', label: 'DOCX' }, { value: 'Txt', label: 'TXT' }]} /></Flex>{isLoading ? <Skeleton active /> : data?.items.length === 0 ? <Empty description="Không tìm thấy tài liệu" /> : <List className="documents-list" dataSource={data?.items} renderItem={doc => <List.Item actions={[<Link key="detail" to={`/documents/${doc.id}`}>Chi tiết</Link>, <Button key="delete" type="text" danger onClick={() => remove(doc.id)}>Xóa</Button>]}><List.Item.Meta avatar={<span className={`file-icon ${doc.fileType.toLowerCase()}`}><FilePdfOutlined /></span>} title={<Link to={`/documents/${doc.id}`}>{doc.originalFileName}</Link>} description={`${formatBytes(doc.fileSizeBytes)} · Tải lên ${formatDate(doc.createdAtUtc)}`} /><Tag color={statusColor(doc.status)}>{doc.status === 'Processed' ? 'Sẵn sàng' : doc.status === 'Processing' ? 'Đang xử lý' : doc.status}</Tag></List.Item>} />}</Card></div>
}

function DocumentDetailPage() {
  const { id = '' } = useParams(); const queryClient = useQueryClient(); const [active, setActive] = useState('summary'); const [chatInput, setChatInput] = useState(''); const [session, setSession] = useState<ChatSession | null>(null)
  const { data: document, isLoading } = useQuery({ queryKey: ['document', id], queryFn: () => api.get<DocumentDetail>(`/documents/${id}`).then(r => r.data) })
  const { data: summary, isLoading: summaryLoading } = useQuery({ queryKey: ['summary', id], queryFn: () => api.get<Summary>(`/documents/${id}/summary`).then(r => r.data), enabled: Boolean(document?.hasSummary) })
  const { data: mindmap } = useQuery({ queryKey: ['mindmap', id], queryFn: () => api.get<MindMap>(`/documents/${id}/mindmap`).then(r => r.data), enabled: Boolean(document?.hasMindMap) })
  const { data: flashcards } = useQuery({ queryKey: ['flashcards', id], queryFn: () => api.get<Flashcards>(`/documents/${id}/flashcards`).then(r => r.data), enabled: Boolean(document?.flashcardCount) })
  const { data: quiz } = useQuery({ queryKey: ['quiz', id], queryFn: () => api.get<Quiz>(`/documents/${id}/quiz`).then(r => r.data), enabled: Boolean(document?.quizCount) })
  const chatQuery = useQuery({ queryKey: ['chat', session?.id], queryFn: () => api.get<ChatMessage[]>(`/chat/sessions/${session?.id}/messages`).then(r => r.data), enabled: Boolean(session) })
  const generate = useMutation({ mutationFn: (type: string) => api.post(`/documents/${id}/${type}`, { forceRegenerate: false }), onSuccess: (_, type) => { queryClient.invalidateQueries({ queryKey: ['document', id] }); queryClient.invalidateQueries({ queryKey: [type, id] }); message.success(`Đã tạo ${type}`) }, onError: error => message.error(errorMessage(error)) })

  useEffect(() => { if (document?.status === 'Processing') { const timer = setInterval(() => queryClient.invalidateQueries({ queryKey: ['document', id] }), 2500); return () => clearInterval(timer) } }, [document?.status, id, queryClient])
  async function sendChat() { if (!chatInput.trim()) return; try { let current = session; if (!current) { const result = await api.post<ChatSession>(`/documents/${id}/chat/sessions`, { title: 'Trao đổi về tài liệu' }); current = result.data; setSession(current) } await api.post(`/chat/sessions/${current.id}/messages`, { content: chatInput }); setChatInput(''); await chatQuery.refetch() } catch (error) { message.error(errorMessage(error)) } }
  if (isLoading) return <div className="page"><Skeleton active /></div>
  if (!document) return <div className="page"><Alert type="error" message="Không tìm thấy tài liệu" /></div>

  const tabs = [{ key: 'summary', label: '✦ Tóm tắt', children: <SummaryPanel summary={summary} loading={summaryLoading} onGenerate={() => generate.mutate('summary')} /> }, { key: 'mindmap', label: '⌁ Mind Map', children: <MindMapPanel data={mindmap} onGenerate={() => generate.mutate('mindmap')} /> }, { key: 'flashcards', label: '◈ Flashcards', children: <FlashcardsPanel data={flashcards} onGenerate={() => generate.mutate('flashcards')} /> }, { key: 'quiz', label: '▣ Quiz', children: <QuizPanel data={quiz} onGenerate={() => generate.mutate('quiz')} /> }, { key: 'chat', label: '✧ Hỏi AI', children: <ChatPanel messages={chatQuery.data ?? []} value={chatInput} onChange={setChatInput} onSend={() => void sendChat()} /> }]
  return <div className="page detail-page"><Link to="/documents" className="back-link">← Quay lại thư viện</Link><div className="detail-heading"><div><Tag color="blue">{document.fileType}</Tag><Typography.Title level={1}>{document.originalFileName}</Typography.Title><Typography.Text type="secondary">{formatBytes(document.fileSizeBytes)} · {formatDate(document.createdAtUtc)}</Typography.Text></div><Space><Tag color={statusColor(document.status)}>{document.status === 'Processed' ? 'Sẵn sàng' : document.status}</Tag><Button href={`${import.meta.env.VITE_API_URL ?? 'http://localhost:5194/api'}/documents/${id}/download`} target="_blank">Tải xuống</Button></Space></div>{document.processingError && <Alert type="error" showIcon message={document.processingError} />}{document.status !== 'Processed' && <Alert className="processing-alert" type="info" showIcon message="AI features sẽ sẵn sàng sau khi tài liệu được xử lý." description="Bạn có thể rời trang, hệ thống sẽ cập nhật trạng thái tự động." />}{document.status === 'Processed' && <Card className="detail-tabs content-card"><Tabs activeKey={active} onChange={setActive} items={tabs} /></Card>}</div>
}

function EmptyAi({ title, description, action }: { title: string; description: string; action: () => void }) { return <div className="ai-empty"><div className="ai-empty-icon">✦</div><Typography.Title level={3}>{title}</Typography.Title><Typography.Paragraph type="secondary">{description}</Typography.Paragraph><Button type="primary" onClick={action}>Tạo bằng AI</Button></div> }
function SummaryPanel({ summary, loading, onGenerate }: { summary?: Summary; loading: boolean; onGenerate: () => void }) { if (loading) return <Skeleton active />; if (!summary) return <EmptyAi title="Chưa có bản tóm tắt" description="AI sẽ chắt lọc các ý chính, định nghĩa và checklist ôn tập cho bạn." action={onGenerate} />; return <div className="summary-content"><Typography.Text className="ai-meta">GENERATED BY {summary.model}</Typography.Text><div className="markdown-lite">{summary.content.split('\n').map((line, index) => <p key={index}>{line || <>&nbsp;</>}</p>)}</div><Button onClick={onGenerate}>Tạo lại</Button></div> }
function MindMapPanel({ data, onGenerate }: { data?: MindMap; onGenerate: () => void }) { if (!data) return <EmptyAi title="Biến tài liệu thành bản đồ" description="Nhìn thấy mối liên hệ giữa các ý tưởng bằng một mind map tương tác." action={onGenerate} />; const nodes: Node[] = data.nodes.map(node => ({ id: node.id, data: { label: <div className="flow-node"><b>{node.label}</b>{node.description && <small>{node.description}</small>}</div> }, position: { x: node.positionX, y: node.positionY }, sourcePosition: Position.Bottom, targetPosition: Position.Top })); const edges: Edge[] = data.nodes.filter(node => node.parentNodeId).map(node => ({ id: `${node.parentNodeId}-${node.id}`, source: node.parentNodeId!, target: node.id, animated: true })); return <div className="mindmap-wrap"><Typography.Title level={3}>{data.title}</Typography.Title><div className="mindmap-canvas"><ReactFlow nodes={nodes} edges={edges} fitView /></div></div> }
function FlashcardsPanel({ data, onGenerate }: { data?: Flashcards; onGenerate: () => void }) { const [index, setIndex] = useState(0); const [flipped, setFlipped] = useState(false); if (!data) return <EmptyAi title="Ôn tập chủ động" description="Tạo flashcard từ những điểm quan trọng nhất trong tài liệu." action={onGenerate} />; const card = data.items[index]; return <div className="flashcards-panel"><Typography.Title level={3}>Flashcards <Tag>{index + 1} / {data.items.length}</Tag></Typography.Title><div className="flashcard" onClick={() => setFlipped(value => !value)}><Typography.Text className="eyebrow">{flipped ? 'ANSWER' : 'QUESTION'}</Typography.Text><Typography.Title level={2}>{flipped ? card.answer : card.question}</Typography.Title>{flipped && card.explanation && <Typography.Paragraph>{card.explanation}</Typography.Paragraph>}<Typography.Text type="secondary">Nhấn để lật thẻ</Typography.Text></div><Space><Button disabled={index === 0} onClick={() => { setIndex(value => value - 1); setFlipped(false) }}>← Trước</Button><Button type="primary" disabled={index === data.items.length - 1} onClick={() => { setIndex(value => value + 1); setFlipped(false) }}>Tiếp →</Button></Space></div> }
function QuizPanel({ data, onGenerate }: { data?: Quiz; onGenerate: () => void }) { const [answers, setAnswers] = useState<Record<string, string>>({}); const [result, setResult] = useState<QuizResult>(); const submit = async () => { try { const response = await api.post<QuizResult>(`/quizzes/${data?.id}/submit`, { answers: Object.entries(answers).map(([questionId, selectedOptionId]) => ({ questionId, selectedOptionId })) }); setResult(response.data); message.success(`Bạn đạt ${response.data.score}/${response.data.totalQuestions} câu`) } catch (error) { message.error(errorMessage(error)) } }; if (!data) return <EmptyAi title="Kiểm tra kiến thức" description="Tạo một quiz ngắn để kiểm tra bạn đã nắm tài liệu đến đâu." action={onGenerate} />; return <div className="quiz-panel"><Typography.Title level={3}>{data.title}</Typography.Title>{data.questions.map((question, index) => <Card key={question.id} className="quiz-question"><Typography.Text strong>Câu {index + 1}</Typography.Text><Typography.Paragraph>{question.content}</Typography.Paragraph><Radio.Group value={answers[question.id]} onChange={event => { setAnswers({ ...answers, [question.id]: event.target.value }); setResult(undefined) }} options={question.options.map(option => ({ label: option.text, value: option.id }))} /></Card>)}<Space><Button type="primary" onClick={() => void submit}>Nộp bài</Button>{result && <Tag color="green">Kết quả: {result.score}/{result.totalQuestions} ({result.percentage}%)</Tag>}</Space></div> }
function ChatPanel({ messages, value, onChange, onSend }: { messages: ChatMessage[]; value: string; onChange: (value: string) => void; onSend: () => void }) { return <div className="chat-panel"><div className="chat-messages">{messages.length === 0 ? <Empty description="Đặt câu hỏi về nội dung tài liệu" /> : messages.map(item => <div key={item.id} className={`chat-message ${item.role === 'user' ? 'user' : 'assistant'}`}><Avatar>{item.role === 'user' ? 'U' : <RobotOutlined />}</Avatar><div><Typography.Text strong>{item.role === 'user' ? 'Bạn' : 'EduMind AI'}</Typography.Text><p>{item.content}</p></div></div>)}</div><Input.TextArea value={value} onChange={event => onChange(event.target.value)} onPressEnter={event => { if (!event.shiftKey) { event.preventDefault(); onSend() } }} placeholder="Hỏi AI về tài liệu này..." autoSize={{ minRows: 2, maxRows: 5 }} /><Button type="primary" onClick={onSend} className="chat-send">Gửi câu hỏi</Button></div> }

export default function App() { return <Routes><Route path="/login" element={<AuthPage mode="login" />} /><Route path="/register" element={<AuthPage mode="register" />} /><Route path="*" element={<Protected><AppShell /></Protected>} /></Routes> }
