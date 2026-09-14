import type { Screening } from '../types/screening'
import { getAuthHeader } from './authHeader'

const API_BASE_URL = 'http://localhost:5267'

export async function getAllScreenings(): Promise<Screening[]> {
    const response = await fetch(`${API_BASE_URL}/api/screenings`, {
        headers: { ...getAuthHeader() },
    })
    if (!response.ok) {
        throw new Error(`Failed to fetch screenings: ${response.status}`)
    }
    return response.json()
}