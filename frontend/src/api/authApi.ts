import type { AuthResponse } from '../types/auth'

const API_BASE_URL = 'http://localhost:5267'

export async function login(email: string, password: string): Promise<AuthResponse> {
    const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
    })
    if (!response.ok) {
        const data = await response.json().catch(() => null)
        throw new Error(data?.message ?? `Failed to log in: ${response.status}`)
    }
    return response.json()
}

export async function register(email: string, password: string, confirmPassword: string): Promise<AuthResponse> {
    const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password, confirmPassword }),
    })
    if (!response.ok) {
        const data = await response.json().catch(() => null)
        throw new Error(data?.message ?? data?.errors?.[0]?.description ?? `Failed to register: ${response.status}`)
    }
    return response.json()
}

export async function logout(token: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/api/auth/logout`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}` },
    })
    if (!response.ok) {
        throw new Error(`Failed to log out: ${response.status}`)
    }
}