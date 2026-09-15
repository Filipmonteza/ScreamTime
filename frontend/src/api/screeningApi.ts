import type { Screening } from '../types/screening'
import { getAuthHeader } from './authHeader'

const API_BASE_URL = 'http://localhost:5267'

export async function getAllScreenings(params?: { movieId?: string }): Promise<Screening[]> {
    const url = new URL(`${API_BASE_URL}/api/screenings`)
    if (params?.movieId) url.searchParams.set('movieId', params.movieId)
    const response = await fetch(url, {
        headers: { ...getAuthHeader() },
    })
    if (!response.ok) {
        throw new Error(`Failed to fetch screenings: ${response.status}`)
    }
    return response.json()
}
