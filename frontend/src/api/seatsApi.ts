import type { SeatType } from '../types/seatType'
import { getAuthHeader } from './authHeader'

const API_BASE_URL = 'http://localhost:5267'

export async function getAllSeatTypes(): Promise<SeatType[]> {
    const response = await fetch(`${API_BASE_URL}/api/seats/types`, {
        headers: { ...getAuthHeader() },
    })
    if (!response.ok) {
        throw new Error(`Failed to fetch seat types: ${response.status}`)
    }
    return response.json()
}