import { useEffect, useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Alert, Avatar, Badge, Button, Card, Col, Empty, Flex, Input, Layout, List, Menu, Modal, Pagination, Popconfirm, Radio, Row, Select, Skeleton, Space, Statistic, Tag, Tabs, Typography, Upload, message } from 'antd'
import { AppstoreOutlined, BookOutlined, CloudUploadOutlined, DownloadOutlined, FilePdfOutlined, FileTextOutlined, LogoutOutlined, ReloadOutlined, RobotOutlined, SearchOutlined, SettingOutlined, ThunderboltFilled } from '@ant-design/icons'
import { Position, ReactFlow, type Edge, type Node } from '@xyflow/react'
import '@xyflow/react/dist/style.css'
import { Link, Navigate, Route, Routes, useLocation, useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { api } from './lib/api'
import { useAuth } from './auth/AuthContext'
import type { AdminDocument, AdminStats, AdminUser, AiUsageSummary, Captcha, ChatMessage, ChatSession, Dashboard, DocumentDetail, DocumentItem, Flashcards, MindMap, PagedResponse, Quiz, QuizResult, Summary } from './types'
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
  const { data: captcha, refetch: refreshCaptcha, isFetching: captchaLoading } = useQuery({ queryKey: ['register-captcha'], queryFn: () => api.get<Captcha>('/auth/captcha').then(response => response.data), enabled: isRegister })
  const schema = isRegister
    ? z.object({ email: z.string().email('Email không hợp lệ'), password: z.string().min(8, 'Mật khẩu tối thiểu 8 ký tự').regex(/[A-Z]/, 'Mật khẩu phải có chữ hoa').regex(/[a-z]/, 'Mật khẩu phải có chữ thường').regex(/[0-9]/, 'Mật khẩu phải có chữ số'), firstName: z.string().min(1, 'Bắt buộc'), lastName: z.string().min(1, 'Bắt buộc'), captchaAnswer: z.string().min(1, 'Vui lòng nhập CAPTCHA') })
    : z.object({ email: z.string().min(1, 'Bắt buộc').refine(value => value.toLowerCase() === 'admin' || z.string().email().safeParse(value).success, 'Nhập email hoặc tài khoản admin'), password: z.string().min(1, 'Bắt buộc'), firstName: z.string().optional(), lastName: z.string().optional() })
  const form = useForm<any>({ resolver: zodResolver(schema), defaultValues: { email: '', password: '', firstName: '', lastName: '', captchaAnswer: '' } })

  if (isAuthenticated) return <Navigate to="/" replace />

  async function submit(values: any) {
    try {
      setError('')
      if (isRegister) {
        if (!captcha) { setError('Đang tải CAPTCHA, vui lòng thử lại.'); return }
        const registration = await register(values.email, values.password, values.firstName, values.lastName, captcha.id, values.captchaAnswer)
        if (registration.developmentOtp) sessionStorage.setItem('edumind.devOtp', registration.developmentOtp)
        sessionStorage.setItem('edumind.otpEmail', registration.email)
        navigate(`/verify-otp?email=${encodeURIComponent(registration.email)}`)
      } else {
        await login(values.email, values.password)
        navigate('/')
      }
    } catch (requestError) { setError(errorMessage(requestError)) }
  }

  return <div className="auth-page">
    <div className="auth-art"><div className="orb orb-a" /><div className="orb orb-b" /><div className="auth-brand brand-font"><span className="brand-mark">✦</span> EduMind AI</div><div className="auth-copy"><Typography.Title>Học sâu hơn.<br /><span>Nhớ lâu hơn.</span></Typography.Title><Typography.Paragraph>Biến mọi tài liệu thành lộ trình học riêng dành cho bạn với sức mạnh của AI.</Typography.Paragraph><div className="auth-pills"><span>✦ Tóm tắt thông minh</span><span>⌁ Mind map trực quan</span><span>◈ Quiz cá nhân hóa</span></div></div></div>
    <div className="auth-form-wrap"><div className="auth-form"><Typography.Text className="eyebrow">WELCOME BACK</Typography.Text><Typography.Title level={2}>{isRegister ? 'Tạo tài khoản mới' : 'Chào mừng trở lại'}</Typography.Title><Typography.Paragraph type="secondary">{isRegister ? 'Bắt đầu hành trình học tập thông minh.' : 'Tiếp tục hành trình học tập của bạn.'}</Typography.Paragraph>{error && <Alert showIcon type="error" message={error} className="form-alert" />}<form onSubmit={form.handleSubmit(submit)}>{isRegister && <Row gutter={12}><Col span={12}><Field label="Họ" name="firstName" register={form.register} error={form.formState.errors.firstName?.message} placeholder="Nguyễn" /></Col><Col span={12}><Field label="Tên" name="lastName" register={form.register} error={form.formState.errors.lastName?.message} placeholder="An" /></Col></Row>}<Field label="Email" name="email" register={form.register} error={form.formState.errors.email?.message} placeholder="you@example.com" type="email" /><Field label="Mật khẩu" name="password" register={form.register} error={form.formState.errors.password?.message} placeholder="Tối thiểu 8 ký tự" type="password" />{isRegister && <div className="captcha-box"><div className="captcha-question"><div><Typography.Text strong>CAPTCHA</Typography.Text>{captcha?.imageDataUrl ? <img src={captcha.imageDataUrl} alt="Mã CAPTCHA" className="captcha-image" /> : <div className="captcha-image-placeholder">Đang tải...</div>}</div><Button type="text" icon={<ReloadOutlined />} loading={captchaLoading} onClick={() => { form.setValue('captchaAnswer', ''); void refreshCaptcha() }} aria-label="Đổi CAPTCHA" /></div><Field label="Nhập các ký tự trong ảnh" name="captchaAnswer" register={form.register} error={form.formState.errors.captchaAnswer?.message} placeholder="Ví dụ: A7KQZ" /></div>}<Button type="primary" htmlType="submit" size="large" block loading={form.formState.isSubmitting}>{isRegister ? 'Tiếp tục xác nhận email' : 'Đăng nhập'} <span>→</span></Button></form><div className="auth-switch">{isRegister ? 'Đã có tài khoản?' : 'Chưa có tài khoản?'} <Link to={isRegister ? '/login' : '/register'}>{isRegister ? 'Đăng nhập' : 'Đăng ký miễn phí'}</Link></div></div></div>
  </div>
}

function Field({ label, name, register, error, placeholder, type = 'text' }: { label: string; name: string; register: any; error?: any; placeholder: string; type?: string }) {
  return <label>{label}<input {...register(name)} type={name === 'email' ? 'text' : type} placeholder={placeholder} />{error && <small>{String(error)}</small>}</label>
}

function AppShell() {
  const { user, logout } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()
  const menuItems = [{ key: '/', icon: <AppstoreOutlined />, label: 'Tổng quan' }, { key: '/documents', icon: <BookOutlined />, label: 'Tài liệu của tôi' }, { key: '/tools/summary', icon: <RobotOutlined />, label: 'Tóm tắt' }, { key: '/tools/mindmap', icon: <ThunderboltFilled />, label: 'Mind Map' }, { key: '/tools/flashcards', icon: <BookOutlined />, label: 'Flashcards' }, { key: '/tools/quiz', icon: <AppstoreOutlined />, label: 'Quiz' }, { key: '/tools/chat', icon: <RobotOutlined />, label: 'Hỏi AI' }, { key: '/preferences', icon: <SettingOutlined />, label: 'Plus & hỗ trợ' }, ...(user?.roles.includes('Admin') ? [{ key: '/admin', icon: <SettingOutlined />, label: 'Quản trị hệ thống' }] : [])]
  return <Layout className="app-layout"><Sider breakpoint="lg" collapsedWidth="0" className="app-sider"><div className="logo brand-font"><span className="brand-mark">✦</span> EduMind <em>AI</em></div><div className="sider-label">WORKSPACE</div><Menu mode="inline" selectedKeys={[location.pathname.startsWith('/tools/') ? location.pathname : location.pathname.startsWith('/documents') ? '/documents' : location.pathname.startsWith('/admin') ? '/admin' : location.pathname.startsWith('/preferences') ? '/preferences' : '/']} items={menuItems} onClick={({ key }) => navigate(key)} /><div className="sider-bottom"><div className={`plan-card ${user?.isPlus ? 'is-plus' : ''}`}><ThunderboltFilled /><div><b>{user?.isPlus ? 'EduMind Plus' : 'Free learning plan'}</b><span>{user?.isPlus ? 'Không giới hạn tính năng' : '2 tài liệu/ngày · 3 summary/ngày'}</span></div></div><Button type="text" icon={<SettingOutlined />} onClick={() => navigate('/preferences')}>Cài đặt & hỗ trợ</Button></div></Sider><Layout><Header className="app-header"><Typography.Text className="header-caption">PERSONAL LEARNING SPACE</Typography.Text><Space><Badge dot color="#22c55e" offset={[-3, 3]}><Avatar style={{ background: '#dbeafe', color: '#2563eb' }}>{user?.firstName?.[0] ?? 'U'}</Avatar></Badge><Typography.Text strong>{user?.firstName} {user?.lastName}</Typography.Text><Tag className={`header-plan-badge ${user?.isPlus ? 'is-plus' : 'is-free'}`}>{user?.isPlus ? 'PLUS' : 'FREE'}</Tag><Button type="text" icon={<LogoutOutlined />} onClick={logout} /></Space></Header><Content className="app-content"><Routes><Route path="/" element={<DashboardPage />} /><Route path="/documents" element={<DocumentsPage />} /><Route path="/documents/:id" element={<DocumentDetailPage />} /><Route path="/tools/:tool" element={<ToolWorkspacePage />} /></Routes>{location.pathname.startsWith('/admin') ? <><AdminPage /><AdminExtras /></> : <Routes><Route path="/preferences" element={<PreferencesPage />} /></Routes>}</Content></Layout></Layout>
}

function ToolWorkspacePage() {
  const { tool = 'summary' } = useParams()
  const navigate = useNavigate()
  const [documentId, setDocumentId] = useState<string>()
  const { data, isLoading } = useQuery({ queryKey: ['documents', 'tool-sidebar'], queryFn: () => api.get<PagedResponse<DocumentItem>>('/documents', { params: { page: 1, pageSize: 50 } }).then(r => r.data) })
  const metadata: Record<string, { label: string; description: string; icon: string }> = { summary: { label: 'Tóm tắt', description: 'Chắt lọc ý chính và cấu trúc tài liệu.', icon: '✦' }, mindmap: { label: 'Mind Map', description: 'Nhìn thấy mối liên hệ giữa các khái niệm.', icon: '⌁' }, flashcards: { label: 'Flashcards', description: 'Ôn tập chủ động bằng thẻ ghi nhớ.', icon: '◈' }, quiz: { label: 'Quiz', description: 'Kiểm tra mức độ nắm bài ngay lập tức.', icon: '▣' }, chat: { label: 'Hỏi AI', description: 'Đặt câu hỏi dựa trên chính tài liệu.', icon: '✧' } }
  const current = metadata[tool] ?? metadata.summary
  return <div className="page tool-workspace-page"><div className="hero-row"><div><Typography.Text className="eyebrow">AI STUDY TOOL</Typography.Text><Typography.Title level={1}><span className="tool-hero-icon">{current.icon}</span>{current.label}</Typography.Title><Typography.Paragraph type="secondary">{current.description} Chọn một tài liệu để bắt đầu phân tích.</Typography.Paragraph></div></div><Card className="tool-picker-card content-card" loading={isLoading}><Typography.Title level={3}>Chọn tài liệu</Typography.Title><Typography.Paragraph type="secondary">Kết quả chỉ được tạo từ tài liệu bạn chọn.</Typography.Paragraph><Select showSearch optionFilterProp="label" className="tool-picker-select" placeholder="Chọn tài liệu cần phân tích" value={documentId} options={(data?.items ?? []).map(document => ({ value: document.id, label: document.originalFileName }))} onChange={setDocumentId} notFoundContent="Bạn chưa tải tài liệu nào" /><Button type="primary" size="large" disabled={!documentId} onClick={() => navigate(`/documents/${documentId}?tab=${tool}`)}>Phân tích tài liệu <span>→</span></Button></Card></div>
}

function LearningTools({ documents }: { documents: DocumentItem[] }) {
  const navigate = useNavigate()
  const [selected, setSelected] = useState<Record<string, string>>({})
  const tools = [{ key: 'summary', label: 'Tóm tắt', description: 'Chắt lọc ý chính và cấu trúc tài liệu.', icon: '✦' }, { key: 'mindmap', label: 'Mind Map', description: 'Nhìn thấy mối liên hệ giữa các khái niệm.', icon: '⌁' }, { key: 'flashcards', label: 'Flashcards', description: 'Ôn tập chủ động bằng thẻ ghi nhớ.', icon: '◈' }, { key: 'quiz', label: 'Quiz', description: 'Kiểm tra mức độ nắm bài ngay lập tức.', icon: '▣' }, { key: 'chat', label: 'Hỏi AI', description: 'Đặt câu hỏi dựa trên chính tài liệu.', icon: '✧' }]
  function openTool(key: string) { const documentId = selected[key]; if (!documentId) { message.info('Hãy chọn tài liệu trước khi phân tích.'); return } navigate(`/documents/${documentId}?tab=${key}`) }
  return <section className="learning-tools"><div className="section-heading"><div><Typography.Text className="eyebrow">AI STUDY TOOLS</Typography.Text><Typography.Title level={2}>Chọn công cụ, chọn tài liệu, bắt đầu học</Typography.Title></div><Typography.Text type="secondary">Mỗi công cụ phân tích một tài liệu cụ thể để kết quả chính xác hơn.</Typography.Text></div><Row gutter={[14, 14]}>{tools.map(tool => <Col xs={24} sm={12} lg={8} xl={Math.ceil(24 / tools.length)} key={tool.key}><Card className="tool-card" bordered={false}><div className="tool-icon">{tool.icon}</div><Typography.Title level={4}>{tool.label}</Typography.Title><Typography.Paragraph type="secondary">{tool.description}</Typography.Paragraph><Select className="tool-select" placeholder="Chọn tài liệu" value={selected[tool.key] || undefined} options={documents.map(document => ({ value: document.id, label: document.originalFileName }))} onChange={value => setSelected({ ...selected, [tool.key]: value })} notFoundContent="Chưa có tài liệu" /><Button className="tool-action" type="link" onClick={() => openTool(tool.key)}>Mở công cụ <span>→</span></Button></Card></Col>)}</Row></section>
}

function DashboardPage() {
  const { user } = useAuth()
  const { data, isLoading } = useQuery({ queryKey: ['dashboard'], queryFn: () => api.get<Dashboard>('/dashboard').then(r => r.data) })
  const { data: library } = useQuery({ queryKey: ['documents', 'tool-picker'], queryFn: () => api.get<PagedResponse<DocumentItem>>('/documents', { params: { page: 1, pageSize: 50 } }).then(r => r.data) })
  const docs = data?.recentDocuments ?? []
  return <div className="page"><div className="hero-row"><div><Typography.Text className="eyebrow">YOUR LEARNING HUB</Typography.Text><Typography.Title>Xin chào, {user?.firstName} <span className="wave">👋</span></Typography.Title><Typography.Paragraph>Chọn đúng tài liệu, rồi để AI đồng hành cùng bạn.</Typography.Paragraph></div><Link to="/documents"><Button type="primary" icon={<CloudUploadOutlined />} size="large">Tải tài liệu mới</Button></Link></div><Row gutter={[16, 16]} className="stats-row"><Col xs={24} sm={8}><Card><Statistic title="Tổng tài liệu" value={data?.totalDocuments ?? 0} prefix={<BookOutlined />} /></Card></Col><Col xs={24} sm={8}><Card><Statistic title="Đã xử lý" value={data?.processedDocuments ?? 0} prefix={<RobotOutlined />} /></Card></Col><Col xs={24} sm={8}><Card><Statistic title="Điểm quiz trung bình" value={data?.averageQuizPercentage ?? 0} suffix="%" prefix={<ThunderboltFilled />} /></Card></Col></Row><LearningTools documents={library?.items ?? []} /><Row gutter={[20, 20]}><Col xs={24}><Card title="Tài liệu gần đây" extra={<Link to="/documents">Xem tất cả →</Link>} className="content-card">{isLoading ? <Skeleton active /> : docs.length === 0 ? <Empty description="Chưa có tài liệu" /> : <List dataSource={docs} renderItem={doc => <List.Item actions={[<Link key="open" to={`/documents/${doc.id}`}>Mở</Link>]}><List.Item.Meta avatar={<span className="file-icon"><FileTextOutlined /></span>} title={doc.originalFileName} description={`${formatBytes(doc.fileSizeBytes)} · ${formatDate(doc.createdAtUtc)}`} /><Tag color={statusColor(doc.status)}>{doc.status}</Tag></List.Item>} />}</Card></Col></Row></div>
}

function AdminPage() {
  const [search, setSearch] = useState('')
  const [documentPage, setDocumentPage] = useState(1)
  const queryClient = useQueryClient()
  const { data: stats } = useQuery({ queryKey: ['admin-stats'], queryFn: () => api.get<AdminStats>('/admin/statistics').then(r => r.data) })
  const { data: users, isLoading } = useQuery({ queryKey: ['admin-users', search], queryFn: () => api.get<PagedResponse<AdminUser>>('/admin/users', { params: { search, page: 1, pageSize: 100 } }).then(r => r.data) })
  const { data: documents } = useQuery({ queryKey: ['admin-documents', documentPage], queryFn: () => api.get<PagedResponse<AdminDocument>>('/admin/documents', { params: { page: documentPage, pageSize: 50 } }).then(r => r.data) })
  const { data: usage } = useQuery({ queryKey: ['admin-ai-usage'], queryFn: () => api.get<AiUsageSummary[]>('/admin/ai-usage').then(r => r.data) })

  async function updateUser(id: string, action: 'activate' | 'deactivate' | 'plus') {
    try {
      if (action === 'plus') await api.post(`/admin/users/${id}/plus`, { durationDays: 30 })
      else await api.post(`/admin/users/${id}/${action}`)
      message.success(action === 'plus' ? 'Đã cấp Plus 30 ngày.' : action === 'activate' ? 'Đã kích hoạt lại tài khoản.' : 'Đã vô hiệu hóa tài khoản.')
      queryClient.invalidateQueries({ queryKey: ['admin-users'] })
    } catch (error) { message.error(errorMessage(error)) }
  }

  async function permanentlyDeleteUser(id: string) {
    try { await api.delete(`/admin/users/${id}`); message.success('Đã xóa vĩnh viễn tài khoản và toàn bộ dữ liệu liên quan.'); queryClient.invalidateQueries({ queryKey: ['admin-users'] }); queryClient.invalidateQueries({ queryKey: ['admin-stats'] }) } catch (error) { message.error(errorMessage(error)) }
  }

  async function downloadDocument(item: AdminDocument) {
    try {
      const response = await api.get(`/admin/documents/${item.id}/download`, { responseType: 'blob' })
      const url = URL.createObjectURL(response.data)
      const anchor = document.createElement('a'); anchor.href = url; anchor.download = item.originalFileName; anchor.click(); URL.revokeObjectURL(url)
    } catch (error) { message.error(errorMessage(error)) }
  }

  return <div className="page"><div className="hero-row"><div><Typography.Text className="eyebrow">ADMIN CONSOLE</Typography.Text><Typography.Title level={1}>Quản trị hệ thống</Typography.Title><Typography.Paragraph type="secondary">Quản lý tài khoản, cấp Plus và kiểm tra toàn bộ tài liệu đã tải lên.</Typography.Paragraph></div></div><Row gutter={[16, 16]} className="stats-row"><Col xs={12} lg={6}><Card><Statistic title="Người dùng" value={stats?.totalUsers ?? 0} /></Card></Col><Col xs={12} lg={6}><Card><Statistic title="Tài liệu" value={stats?.totalDocuments ?? 0} /></Card></Col><Col xs={12} lg={6}><Card><Statistic title="Dung lượng" value={formatBytes(stats?.storageBytes ?? 0)} /></Card></Col><Col xs={12} lg={6}><Card><Statistic title="AI requests" value={stats?.aiRequestCount ?? 0} /></Card></Col></Row><Tabs items={[{ key: 'users', label: 'Quản lý tài khoản', children: <Card className="content-card"><Input prefix={<SearchOutlined />} placeholder="Tìm email hoặc tên..." value={search} onChange={event => setSearch(event.target.value)} allowClear style={{ maxWidth: 420, marginBottom: 16 }} />{isLoading ? <Skeleton active /> : <List dataSource={users?.items ?? []} renderItem={item => <List.Item actions={[<Button key="plus" type="link" onClick={() => void updateUser(item.id, 'plus')}>Cấp Plus 30 ngày</Button>, item.isActive ? <Popconfirm key="deactivate" title="Vô hiệu hóa tài khoản này?" onConfirm={() => void updateUser(item.id, 'deactivate')}><Button danger type="text">Vô hiệu hóa</Button></Popconfirm> : <Button key="activate" type="text" onClick={() => void updateUser(item.id, 'activate')}>Kích hoạt lại</Button>, <Popconfirm key="delete" title="Xóa vĩnh viễn tài khoản và toàn bộ dữ liệu?" description="Không thể hoàn tác. Email sẽ được phép đăng ký lại." okText="Xóa vĩnh viễn" cancelText="Hủy" okType="danger" onConfirm={() => void permanentlyDeleteUser(item.id)}><Button danger type="text">Xóa vĩnh viễn</Button></Popconfirm>]}><List.Item.Meta title={<Space>{item.firstName} {item.lastName}{item.isPlus ? <Tag color="purple">PLUS</Tag> : <Tag>FREE</Tag>}</Space>} description={`${item.email} · ${item.roles.join(', ')} · ${item.isActive ? 'Đang hoạt động' : 'Đã vô hiệu hóa'} · ${formatDate(item.createdAtUtc)}`} /></List.Item>} />}</Card> }, { key: 'documents', label: 'Toàn bộ tài liệu', children: <Card className="content-card"><List dataSource={documents?.items ?? []} renderItem={item => <List.Item actions={[<Button key="download" icon={<DownloadOutlined />} onClick={() => void downloadDocument(item)}>Xem / tải xuống</Button>]}><List.Item.Meta title={item.originalFileName} description={`${item.ownerEmail} · ${formatBytes(item.fileSizeBytes)} · ${formatDate(item.createdAtUtc)}`} /><Tag color={statusColor(item.status)}>{item.status}</Tag></List.Item>} />{documents && <Pagination current={documents.page} pageSize={documents.pageSize} total={documents.totalCount} showSizeChanger={false} onChange={setDocumentPage} style={{ marginTop: 16, textAlign: 'right' }} />}</Card> }, { key: 'usage', label: 'AI usage', children: <Card className="content-card"><List dataSource={usage ?? []} renderItem={item => <List.Item><List.Item.Meta title={item.operation} description={`${item.requestCount} requests`} /><Typography.Text>{item.inputTokens + item.outputTokens} tokens</Typography.Text></List.Item>} /></Card> }]} /></div>
}

export function LegacyAdminPage() {
  const [search, setSearch] = useState('')
  const queryClient = useQueryClient()
  const { data: stats } = useQuery({ queryKey: ['admin-stats'], queryFn: () => api.get<AdminStats>('/admin/statistics').then(r => r.data) })
  const { data: users, isLoading } = useQuery({ queryKey: ['admin-users', search], queryFn: () => api.get<PagedResponse<AdminUser>>('/admin/users', { params: { search, page: 1, pageSize: 20 } }).then(r => r.data) })
  const { data: documents } = useQuery({ queryKey: ['admin-documents'], queryFn: () => api.get<PagedResponse<AdminDocument>>('/admin/documents', { params: { page: 1, pageSize: 20 } }).then(r => r.data) })
  const { data: usage } = useQuery({ queryKey: ['admin-ai-usage'], queryFn: () => api.get<AiUsageSummary[]>('/admin/ai-usage').then(r => r.data) })
  const deactivate = async (id: string) => { try { await api.post(`/admin/users/${id}/deactivate`); message.success('Đã vô hiệu hóa tài khoản'); queryClient.invalidateQueries({ queryKey: ['admin-users'] }) } catch (error) { message.error(errorMessage(error)) } }
  return <div className="page"><div className="hero-row"><div><Typography.Text className="eyebrow">ADMIN CONSOLE</Typography.Text><Typography.Title level={1}>Quản trị hệ thống</Typography.Title><Typography.Paragraph type="secondary">Theo dõi người dùng, tài liệu và mức sử dụng AI.</Typography.Paragraph></div></div><Row gutter={[16, 16]} className="stats-row"><Col xs={12} lg={6}><Card><Statistic title="Người dùng" value={stats?.totalUsers ?? 0} /></Card></Col><Col xs={12} lg={6}><Card><Statistic title="Tài liệu" value={stats?.totalDocuments ?? 0} /></Card></Col><Col xs={12} lg={6}><Card><Statistic title="Dung lượng" value={formatBytes(stats?.storageBytes ?? 0)} /></Card></Col><Col xs={12} lg={6}><Card><Statistic title="AI requests" value={stats?.aiRequestCount ?? 0} /></Card></Col></Row><Tabs items={[{ key: 'users', label: 'Người dùng', children: <Card className="content-card"><Input prefix={<SearchOutlined />} placeholder="Tìm email hoặc tên..." value={search} onChange={event => setSearch(event.target.value)} allowClear style={{ maxWidth: 420, marginBottom: 16 }} />{isLoading ? <Skeleton active /> : <List dataSource={users?.items ?? []} renderItem={item => <List.Item actions={item.isActive ? [<Button key="deactivate" danger type="text" onClick={() => void deactivate(item.id)}>Vô hiệu hóa</Button>] : [<Tag key="inactive" color="default">Đã khóa</Tag>]}><List.Item.Meta title={`${item.firstName} ${item.lastName}`} description={`${item.email} · ${item.roles.join(', ')} · ${formatDate(item.createdAtUtc)}`} /></List.Item>} />}</Card> }, { key: 'documents', label: 'Tài liệu', children: <Card className="content-card"><List dataSource={documents?.items ?? []} renderItem={item => <List.Item><List.Item.Meta title={item.originalFileName} description={`${item.ownerEmail} · ${formatBytes(item.fileSizeBytes)} · ${formatDate(item.createdAtUtc)}`} /><Tag color={statusColor(item.status)}>{item.status}</Tag></List.Item>} /></Card> }, { key: 'usage', label: 'AI usage', children: <Card className="content-card"><List dataSource={usage ?? []} renderItem={item => <List.Item><List.Item.Meta title={item.operation} description={`${item.requestCount} requests`} /><Typography.Text>{item.inputTokens + item.outputTokens} tokens</Typography.Text></List.Item>} /></Card> }]} /></div>
}

function AdminExtras() {
  const queryClient = useQueryClient()
  const [reply, setReply] = useState<Record<string, string>>({})
  const { data: requests } = useQuery({ queryKey: ['admin-plus-requests'], queryFn: () => api.get<import('./types').PlusRequestAdmin[]>('/admin/plus-requests').then(r => r.data) })
  const { data: tickets } = useQuery({ queryKey: ['admin-support-tickets'], queryFn: () => api.get<import('./types').SupportTicket[]>('/admin/support-tickets').then(r => r.data) })
  async function process(id: string, approve: boolean) { try { await api.post(`/admin/plus-requests/${id}/process`, { approve, durationDays: approve ? 30 : null, note: approve ? 'Đã kiểm tra chuyển khoản.' : 'Không xác nhận được giao dịch.' }); message.success(approve ? 'Đã cấp Plus 30 ngày.' : 'Đã từ chối yêu cầu.'); queryClient.invalidateQueries({ queryKey: ['admin-plus-requests'] }) } catch (error) { message.error(errorMessage(error)) } }
  async function resolve(id: string) { const value = reply[id]?.trim(); if (!value) return; try { await api.post(`/admin/support-tickets/${id}/resolve`, value, { headers: { 'Content-Type': 'application/json' } }); message.success('Đã phản hồi hỗ trợ.'); queryClient.invalidateQueries({ queryKey: ['admin-support-tickets'] }) } catch (error) { message.error(errorMessage(error)) } }
  return <div className="admin-extras"><Row gutter={[20, 20]}><Col xs={24} lg={12}><Card className="content-card" title="Yêu cầu cấp Plus"><List dataSource={requests ?? []} locale={{ emptyText: 'Chưa có yêu cầu Plus' }} renderItem={item => <List.Item actions={item.status === 'Pending' ? [<Button key="reject" danger onClick={() => void process(item.id, false)}>Từ chối</Button>, <Button key="approve" type="primary" onClick={() => void process(item.id, true)}>Cấp Plus 30 ngày</Button>] : [<Tag key="status">{item.status}</Tag>]}><List.Item.Meta title={`${item.fullName} · ${item.email}`} description={`${item.transferContent} · ${item.amountVnd.toLocaleString('vi-VN')}₫`} /></List.Item>} /></Card></Col><Col xs={24} lg={12}><Card className="content-card" title="Hỗ trợ khách hàng"><List dataSource={tickets ?? []} locale={{ emptyText: 'Không có ticket hỗ trợ' }} renderItem={item => <List.Item actions={item.status === 'Open' ? [<Button key="resolve" type="primary" disabled={!reply[item.id]?.trim()} onClick={() => void resolve(item.id)}>Đã phản hồi</Button>] : [<Tag key="resolved" color="green">Đã xử lý</Tag>]}><List.Item.Meta title={item.subject} description={item.message} />{item.status === 'Open' && <Input value={reply[item.id] ?? ''} onChange={event => setReply({ ...reply, [item.id]: event.target.value })} placeholder="Nhập phản hồi..." />}</List.Item>} /></Card></Col></Row></div>
}

function PreferencesPage() {
  const { user } = useAuth()
  const [showPayment, setShowPayment] = useState(false)
  const [supportMessage, setSupportMessage] = useState('')
  const [supportMessages, setSupportMessages] = useState([{ role: 'assistant', content: 'Xin chào! Nếu bạn đã chuyển khoản nhưng chưa được cấp Plus, hãy gửi nội dung giao dịch tại đây.' }])
  const plusRequest = useMutation({ mutationFn: () => api.post('/billing/plus-requests', { note: 'Đã chuyển khoản, vui lòng kiểm tra và cấp Plus.' }), onSuccess: () => message.success('Đã gửi yêu cầu Plus. Admin sẽ kiểm tra giao dịch.'), onError: error => message.error(errorMessage(error)) })
  const sendSupport = useMutation({ mutationFn: () => api.post('/support/tickets', { subject: 'Hỗ trợ cấp Plus', message: supportMessage }), onSuccess: () => { setSupportMessages(items => [...items, { role: 'user', content: supportMessage }, { role: 'assistant', content: 'Đã nhận yêu cầu. Admin sẽ kiểm tra và phản hồi sớm nhất.' }]); setSupportMessage('') }, onError: error => message.error(errorMessage(error)) })
  const transferContent = `${user?.email ?? ''} DKI PLUS`
  const qrUrl = `https://img.vietqr.io/image/MB-0377599221-compact2.png?amount=49000&addInfo=${encodeURIComponent(transferContent)}&accountName=EDUMIND%20AI`
  return <div className="page settings-page"><div className="hero-row"><div><Typography.Text className="eyebrow">ACCOUNT & PLAN</Typography.Text><Typography.Title level={1}>Tài khoản của bạn</Typography.Title><Typography.Paragraph type="secondary">Quản lý trạng thái gói và nhận hỗ trợ từ EduMind AI.</Typography.Paragraph></div></div><Row gutter={[20, 20]}><Col xs={24} lg={15}><Card className={`plan-status-card ${user?.isPlus ? 'is-plus' : 'is-free'}`}><div className="plan-status-top"><div><Typography.Text className="eyebrow">CURRENT PLAN</Typography.Text><Typography.Title level={2}>{user?.isPlus ? 'EduMind Plus' : 'EduMind Free'}</Typography.Title><Typography.Paragraph>{user?.isPlus ? 'Bạn đang sử dụng toàn bộ không gian học tập không giới hạn.' : 'Bạn đang dùng gói miễn phí với giới hạn hằng ngày.'}</Typography.Paragraph></div><Tag className="plan-badge" color={user?.isPlus ? 'purple' : 'blue'}>{user?.isPlus ? 'PLUS' : 'FREE'}</Tag></div><div className="benefit-grid"><span>✓ {user?.isPlus ? 'Không giới hạn tài liệu' : '2 tài liệu mỗi ngày'}</span><span>✓ {user?.isPlus ? 'Summary & Hỏi AI không giới hạn' : '3 summary · 5 câu hỏi/ngày'}</span><span>✓ Mind Map & Flashcards {user?.isPlus ? 'đã mở khóa' : 'chỉ dành cho Plus'}</span><span>✓ Quiz và theo dõi tiến độ học tập</span></div>{!user?.isPlus && <Button className="plus-cta" type="primary" size="large" onClick={() => setShowPayment(value => !value)}>{showPayment ? 'Ẩn thông tin thanh toán' : 'Đăng ký Plus — 49.000₫'}</Button>}</Card></Col>{!user?.isPlus && showPayment && <Col xs={24} lg={9}><Card className="plus-card payment-card" bordered={false}><Typography.Text className="eyebrow">THANH TOÁN PLUS</Typography.Text><Typography.Title level={3}>Quét QR để đăng ký</Typography.Title><img className="payment-qr" src={qrUrl} alt="QR chuyển khoản MB Bank" /><Typography.Text className="transfer-note">MB Bank · 0377599221<br />Số tiền: <b>49.000₫</b><br />Nội dung: <b>{transferContent}</b></Typography.Text><Button type="primary" block loading={plusRequest.isPending} onClick={() => plusRequest.mutate()}>Tôi đã chuyển khoản — gửi yêu cầu</Button></Card></Col>}<Col xs={24}><Card className="content-card support-chat-card" title="Hỗ trợ khách hàng"><div className="support-chat-window">{supportMessages.map((item, index) => <div key={`${item.role}-${index}`} className={`support-bubble ${item.role}`}>{item.content}</div>)}</div><div className="support-chat-input"><Input value={supportMessage} onChange={event => setSupportMessage(event.target.value)} onPressEnter={() => { if (supportMessage.trim()) sendSupport.mutate() }} placeholder="Nhập nội dung cần hỗ trợ..." /><Button type="primary" loading={sendSupport.isPending} disabled={!supportMessage.trim()} onClick={() => sendSupport.mutate()}>Gửi</Button></div></Card></Col></Row></div>
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
  const { id = '' } = useParams(); const location = useLocation(); const queryClient = useQueryClient(); const [active, setActive] = useState(new URLSearchParams(location.search).get('tab') ?? 'summary'); const [chatInput, setChatInput] = useState(''); const [session, setSession] = useState<ChatSession | null>(null)
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
  return <div className="page detail-page"><Link to="/documents" className="back-link">← Quay lại thư viện</Link><div className="detail-heading"><div><Tag color="blue">{document.fileType}</Tag><Typography.Title level={1}>{document.originalFileName}</Typography.Title><Typography.Text type="secondary">{formatBytes(document.fileSizeBytes)} · {formatDate(document.createdAtUtc)}</Typography.Text></div><Space><Tag color={statusColor(document.status)}>{document.status === 'Processed' ? 'Sẵn sàng' : document.status}</Tag><Button href={`${import.meta.env.VITE_API_URL ?? 'http://127.0.0.1:5194/api'}/documents/${id}/download`} target="_blank">Tải xuống</Button></Space></div>{document.processingError && <Alert type="error" showIcon message={document.processingError} />}{document.status !== 'Processed' && <Alert className="processing-alert" type="info" showIcon message="AI features sẽ sẵn sàng sau khi tài liệu được xử lý." description="Bạn có thể rời trang, hệ thống sẽ cập nhật trạng thái tự động." />}{document.status === 'Processed' && <Card className="detail-tabs content-card"><Tabs activeKey={active} onChange={setActive} items={tabs} /></Card>}</div>
}

function EmptyAi({ title, description, action }: { title: string; description: string; action: () => void }) { return <div className="ai-empty"><div className="ai-empty-icon">✦</div><Typography.Title level={3}>{title}</Typography.Title><Typography.Paragraph type="secondary">{description}</Typography.Paragraph><Button type="primary" onClick={action}>Tạo bằng AI</Button></div> }
function SummaryPanel({ summary, loading, onGenerate }: { summary?: Summary; loading: boolean; onGenerate: () => void }) { if (loading) return <Skeleton active />; if (!summary) return <EmptyAi title="Chưa có bản tóm tắt" description="AI sẽ chắt lọc các ý chính, định nghĩa và checklist ôn tập cho bạn." action={onGenerate} />; return <div className="summary-content"><Typography.Text className="ai-meta">GENERATED BY {summary.model}</Typography.Text><div className="markdown-lite">{summary.content.split('\n').map((line, index) => <p key={index}>{line || <>&nbsp;</>}</p>)}</div><Button onClick={onGenerate}>Tạo lại</Button></div> }
function MindMapPanel({ data, onGenerate }: { data?: MindMap; onGenerate: () => void }) { if (!data) return <EmptyAi title="Biến tài liệu thành bản đồ" description="Nhìn thấy mối liên hệ giữa các ý tưởng bằng một mind map tương tác." action={onGenerate} />; const nodes: Node[] = data.nodes.map(node => ({ id: node.id, data: { label: <div className="flow-node"><b>{node.label}</b>{node.description && <small>{node.description}</small>}</div> }, position: { x: node.positionX, y: node.positionY }, sourcePosition: Position.Bottom, targetPosition: Position.Top })); const edges: Edge[] = data.nodes.filter(node => node.parentNodeId).map(node => ({ id: `${node.parentNodeId}-${node.id}`, source: node.parentNodeId!, target: node.id, animated: true })); return <div className="mindmap-wrap"><Typography.Title level={3}>{data.title}</Typography.Title><div className="mindmap-canvas"><ReactFlow nodes={nodes} edges={edges} fitView /></div></div> }
function FlashcardsPanel({ data, onGenerate }: { data?: Flashcards; onGenerate: () => void }) { const [index, setIndex] = useState(0); const [flipped, setFlipped] = useState(false); if (!data) return <EmptyAi title="Ôn tập chủ động" description="Tạo flashcard từ những điểm quan trọng nhất trong tài liệu." action={onGenerate} />; const card = data.items[index]; const review = async (status: string) => { try { await api.post(`/flashcards/${card.id}/review`, { status }); message.success(status === 'known' ? 'Đã ghi nhận: bạn đã nhớ' : 'Đã ghi nhận: cần ôn lại') } catch (error) { message.error(errorMessage(error)) } }; return <div className="flashcards-panel"><Typography.Title level={3}>Flashcards <Tag>{index + 1} / {data.items.length}</Tag></Typography.Title><div className="flashcard" onClick={() => setFlipped(value => !value)}><Typography.Text className="eyebrow">{flipped ? 'ANSWER' : 'QUESTION'}</Typography.Text><Typography.Title level={2}>{flipped ? card.answer : card.question}</Typography.Title>{flipped && card.explanation && <Typography.Paragraph>{card.explanation}</Typography.Paragraph>}<Typography.Text type="secondary">Nhấn để lật thẻ</Typography.Text></div><Space wrap><Button danger onClick={() => void review('unknown')}>Chưa nhớ</Button><Button onClick={() => void review('review')}>Ôn lại</Button><Button type="primary" onClick={() => void review('known')}>Đã nhớ</Button></Space><Space><Button disabled={index === 0} onClick={() => { setIndex(value => value - 1); setFlipped(false) }}>← Trước</Button><Button type="primary" disabled={index === data.items.length - 1} onClick={() => { setIndex(value => value + 1); setFlipped(false) }}>Tiếp →</Button></Space></div> }
function QuizPanel({ data, onGenerate }: { data?: Quiz; onGenerate: () => void }) { const [answers, setAnswers] = useState<Record<string, string>>({}); const [result, setResult] = useState<QuizResult>(); const submit = async () => { try { const response = await api.post<QuizResult>(`/quizzes/${data?.id}/submit`, { answers: Object.entries(answers).map(([questionId, selectedOptionId]) => ({ questionId, selectedOptionId })) }); setResult(response.data); message.success(`Bạn đạt ${response.data.score}/${response.data.totalQuestions} câu`) } catch (error) { message.error(errorMessage(error)) } }; if (!data) return <EmptyAi title="Kiểm tra kiến thức" description="Tạo một quiz ngắn để kiểm tra bạn đã nắm tài liệu đến đâu." action={onGenerate} />; return <div className="quiz-panel"><Typography.Title level={3}>{data.title}</Typography.Title>{data.questions.map((question, index) => <Card key={question.id} className="quiz-question"><Typography.Text strong>Câu {index + 1}</Typography.Text><Typography.Paragraph>{question.content}</Typography.Paragraph><Radio.Group value={answers[question.id]} onChange={event => { setAnswers({ ...answers, [question.id]: event.target.value }); setResult(undefined) }} options={question.options.map(option => ({ label: option.text, value: option.id }))} /></Card>)}<Space><Button type="primary" onClick={() => void submit}>Nộp bài</Button>{result && <Tag color="green">Kết quả: {result.score}/{result.totalQuestions} ({result.percentage}%)</Tag>}</Space></div> }
function ChatPanel({ messages, value, onChange, onSend }: { messages: ChatMessage[]; value: string; onChange: (value: string) => void; onSend: () => void }) { return <div className="chat-panel"><div className="chat-messages">{messages.length === 0 ? <Empty description="Đặt câu hỏi về nội dung tài liệu" /> : messages.map(item => <div key={item.id} className={`chat-message ${item.role === 'user' ? 'user' : 'assistant'}`}><Avatar>{item.role === 'user' ? 'U' : <RobotOutlined />}</Avatar><div><Typography.Text strong>{item.role === 'user' ? 'Bạn' : 'EduMind AI'}</Typography.Text><p>{item.content}</p></div></div>)}</div><Input.TextArea value={value} onChange={event => onChange(event.target.value)} onPressEnter={event => { if (!event.shiftKey) { event.preventDefault(); onSend() } }} placeholder="Hỏi AI về tài liệu này..." autoSize={{ minRows: 2, maxRows: 5 }} /><Button type="primary" onClick={onSend} className="chat-send">Gửi câu hỏi</Button></div> }

function VerifyOtpPage() {
  const email = new URLSearchParams(window.location.search).get('email') ?? sessionStorage.getItem('edumind.otpEmail') ?? ''
  const [code, setCode] = useState('')
  const [error, setError] = useState('')
  const [success, setSuccess] = useState(false)
  const [loading, setLoading] = useState(false)
  const [resending, setResending] = useState(false)
  const [developmentOtp, setDevelopmentOtp] = useState(sessionStorage.getItem('edumind.devOtp') ?? '')

  async function verify() {
    try {
      setError(''); setLoading(true)
      await api.post('/auth/verify-otp', { email, code })
      setSuccess(true); sessionStorage.removeItem('edumind.devOtp')
    } catch (requestError) { setError(errorMessage(requestError)) } finally { setLoading(false) }
  }

  async function resend() {
    try {
      setError(''); setResending(true)
      const response = await api.post<{ developmentOtp?: string }>('/auth/resend-otp', { email })
      if (response.data.developmentOtp) { setDevelopmentOtp(response.data.developmentOtp); sessionStorage.setItem('edumind.devOtp', response.data.developmentOtp) }
      message.success('Đã gửi lại mã OTP.')
    } catch (requestError) { setError(errorMessage(requestError)) } finally { setResending(false) }
  }

  return <div className="auth-page verify-page"><div className="auth-form-wrap"><div className="auth-form verify-card"><Typography.Text className="eyebrow">EMAIL OTP</Typography.Text><Typography.Title level={2}>{success ? 'Xác nhận thành công' : 'Nhập mã OTP'}</Typography.Title>{success ? <><Typography.Paragraph type="secondary">Email <b>{email}</b> đã được xác nhận. Bạn có thể đăng nhập ngay.</Typography.Paragraph><Link to="/login"><Button type="primary" block>Đăng nhập</Button></Link></> : <><Typography.Paragraph type="secondary">Mã 6 chữ số đã được gửi đến <b>{email}</b>. Mã có hiệu lực trong 10 phút.</Typography.Paragraph>{developmentOtp && <Alert type="info" showIcon message={`Development OTP: ${developmentOtp}`} description="SMTP chưa cấu hình nên mã được hiển thị để kiểm thử local." />}{error && <Alert showIcon type="error" message={error} className="form-alert" />}<Input className="otp-input" value={code} onChange={event => setCode(event.target.value.replace(/\D/g, '').slice(0, 6))} maxLength={6} placeholder="000000" size="large" inputMode="numeric" /><Button type="primary" block size="large" loading={loading} disabled={code.length !== 6} onClick={() => void verify()}>Xác nhận email</Button><Button type="link" block loading={resending} onClick={() => void resend()}>Gửi lại mã OTP</Button></>}</div></div></div>
}

function VerifyPendingPage() {
  const email = new URLSearchParams(window.location.search).get('email')
  const developmentUrl = sessionStorage.getItem('edumind.devVerificationUrl')
  return <div className="auth-page verify-page"><div className="auth-form-wrap"><div className="auth-form verify-card"><Typography.Text className="eyebrow">CHECK YOUR INBOX</Typography.Text><Typography.Title level={2}>Xác nhận email của bạn</Typography.Title><Typography.Paragraph type="secondary">Chúng tôi đã gửi liên kết xác nhận đến <b>{email}</b>. Hãy click vào liên kết trước khi đăng nhập.</Typography.Paragraph>{developmentUrl && <Alert type="info" showIcon message="Development mode" description={<a href={developmentUrl}>Mở liên kết xác nhận email</a>} /> }<div className="auth-switch"><Link to="/login">Quay lại đăng nhập</Link></div></div></div></div>
}

function VerifyEmailPage() {
  const [state, setState] = useState<'loading' | 'success' | 'error'>('loading')
  const [detail, setDetail] = useState('Đang xác nhận liên kết email của bạn...')
  useEffect(() => {
    const token = new URLSearchParams(window.location.search).get('token')
    if (!token) { setState('error'); setDetail('Liên kết xác nhận không hợp lệ.'); return }
    api.get('/auth/verify-email', { params: { token } }).then(() => { setState('success'); setDetail('Email đã được xác nhận. Bạn có thể đăng nhập ngay bây giờ.') }).catch(error => { setState('error'); setDetail(errorMessage(error)) })
  }, [])
  return <div className="auth-page verify-page"><div className="auth-form-wrap"><div className="auth-form verify-card"><Typography.Text className="eyebrow">EMAIL VERIFICATION</Typography.Text><Typography.Title level={2}>{state === 'loading' ? 'Đang xác nhận...' : state === 'success' ? 'Xác nhận thành công' : 'Không thể xác nhận'}</Typography.Title><Typography.Paragraph type="secondary">{detail}</Typography.Paragraph>{state !== 'loading' && <Link to={state === 'success' ? '/login' : '/register'}><Button type="primary" block>{state === 'success' ? 'Đăng nhập' : 'Tạo liên kết mới'}</Button></Link>}</div></div></div>
}

export default function App() { return <Routes><Route path="/login" element={<AuthPage mode="login" />} /><Route path="/register" element={<AuthPage mode="register" />} /><Route path="/verify-otp" element={<VerifyOtpPage />} /><Route path="/verify-pending" element={<VerifyPendingPage />} /><Route path="/verify-email" element={<VerifyEmailPage />} /><Route path="*" element={<Protected><AppShell /></Protected>} /></Routes> }
