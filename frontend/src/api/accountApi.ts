import { getAuthHeader } from './authHeader'
import type { AuthResponse } from '../types/auth'
import { extractErrorMessage } from './apiError'

const API_BASE_URL = 'http://localhost:5267'

export async function updateEmail(newEmail: string, currentPassword: string): Promise<AuthResponse> {
    const response = await fetch(`${API_BASE_URL}/api/account/email`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
        body: JSON.stringify({ newEmail, currentPassword }),
    })
    if (!response.ok) {
        throw new Error(await extractErrorMessage(response, `Failed to update email: ${response.status}`))
    }
    return response.json()
}

export async function updatePassword(currentPassword: string, newPassword: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/api/account/password`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
        body: JSON.stringify({ currentPassword, newPassword }),
    })
    if (!response.ok) {
        throw new Error(await extractErrorMessage(response, `Failed to update password: ${response.status}`))
    }
}

export async function deactivateAccount(): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/api/account/deactivate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
    })
    if (!response.ok) {
        throw new Error(await extractErrorMessage(response, `Failed to deactivate account: ${response.status}`))
    }
}

