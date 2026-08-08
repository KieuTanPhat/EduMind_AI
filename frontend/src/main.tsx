import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ConfigProvider } from 'antd'
import { BrowserRouter } from 'react-router-dom'
import { AuthProvider } from './auth/AuthContext'
import App from './App'
import './index.css'

const queryClient = new QueryClient({
  defaultOptions: { queries: { staleTime: 30_000, retry: 1 } },
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <ConfigProvider theme={{ token: { colorPrimary: '#377ff0', colorLink: '#245dca', colorText: '#17263b', colorTextSecondary: '#73839a', colorBorder: '#e4eaf2', colorBgLayout: '#f7f9fc', borderRadius: 10, borderRadiusLG: 16, fontFamily: 'DM Sans, system-ui, sans-serif', controlHeight: 40 }, components: { Card: { paddingLG: 22 }, Tabs: { itemColor: '#73839a', itemSelectedColor: '#245dca', inkBarColor: '#377ff0' }, Menu: { itemSelectedBg: '#eaf2ff', itemSelectedColor: '#245dca', itemHoverBg: '#f5f8ff' } } }}>
        <BrowserRouter>
          <AuthProvider>
            <App />
          </AuthProvider>
        </BrowserRouter>
      </ConfigProvider>
    </QueryClientProvider>
  </StrictMode>,
)
