import type { Screening } from '../types/screening'
import { getAuthHeader } from './authHeader'

const API_BASE_URL = 'http://localhost:5267'

export async function getAllScreenings(params?: {
    movieId?: string
    date?: string
    includeCancelled?: boolean
}): Promise<Screening[]> {
    const url = new URL(`${API_BASE_URL}/api/screenings`)

    if (params?.movieId) url.searchParams.set('movieId', params.movieId)
    if (params?.date) url.searchParams.set('date', params.date)
    if (params?.includeCancelled) url.searchParams.set('includeCancelled', 'true')

    const response = await fetch(url.toString(), {
        headers: params?.includeCancelled ? { ...getAuthHeader() } : undefined,
    })

    if (!response.ok) {
        throw new Error(`Failed to fetch screenings: ${response.status}`)
    }

    return response.json()
}

export async function getScreeningById(id: string): Promise<Screening> {
    const response = await fetch(`${API_BASE_URL}/api/screenings/${id}`)

    if (!response.ok) {
        throw new Error(`Failed to fetch screening: ${response.status}`)
    }

    return response.json()
}

export async function createScreening(screening: {
    movieId: string
    auditoriumId: string
    startTime: string
}): Promise<Screening> {
    const response = await fetch(`${API_BASE_URL}/api/screenings`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
        body: JSON.stringify(screening),
    })

    if (!response.ok) {
        if (response.status === 409) {
            throw new Error(
                'Det finns redan en visning i den valda salongen som överlappar med den här tiden.',
            )
        }

        throw new Error(`Failed to create screening: ${response.status}`)
    }

    return response.json()
}

export async function updateScreening(
    id: string,
    screening: {
        movieId: string
        auditoriumId: string
        startTime: string
    },
): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/api/screenings/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
        body: JSON.stringify(screening),
    })

    if (!response.ok) {
        if (response.status === 409) {
            throw new Error(
                'Det finns redan en visning i den valda salongen som överlappar med den här tiden.',
            )
        }

        throw new Error(`Failed to update screening: ${response.status}`)
    }
}

export async function cancelScreening(id: string): Promise<void> {
    const response = await fetch(
        `${API_BASE_URL}/api/screenings/${id}/cancel`,
        {
            method: 'POST',
            headers: { ...getAuthHeader() },
        },
    )

    if (!response.ok) {
        throw new Error(`Failed to cancel screening: ${response.status}`)
    }
}