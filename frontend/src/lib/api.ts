import axios from 'axios'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://127.0.0.1:5194/api',
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('edumind.accessToken')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('edumind.accessToken')
      localStorage.removeItem('edumind.auth')
    }
    return Promise.reject(error)
  },
)
