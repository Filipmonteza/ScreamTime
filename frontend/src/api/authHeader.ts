import type { AuthResponse } from '../types/auth'

const STORAGE_KEY = 'auth'

export function getAuthHeader(): Record<string, string> {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return {}
    const parsed: AuthResponse = JSON.parse(raw)
    if (new Date(parsed.expiration) <= new Date()) return {}
    return { Authorization: `Bearer ${parsed.token}` }
}