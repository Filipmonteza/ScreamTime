import type { ScreeningSeatMap } from '../types/seatMap'
import { getAuthHeader } from './authHeader'
import type { BookingConfirmation, BookingListItem } from '../types/booking'

const API_BASE_URL = 'http://localhost:5267'

export async function getSeatMap(screeningId: string): Promise<ScreeningSeatMap> {
    const response = await fetch(`${API_BASE_URL}/api/bookings/screenings/${screeningId}/seats`, {
        headers: { ...getAuthHeader() },
    })
    if (!response.ok) {
        throw new Error(`Failed to fetch seat map: ${response.status}`)
    }
    return response.json()
}

export async function createBooking(
    screeningId: string,
    seatIds: string[],
): Promise<BookingConfirmation> {
    const response = await fetch(`${API_BASE_URL}/api/bookings`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            ...getAuthHeader(),
        },
        body: JSON.stringify({ screeningId, seatIds }),
    })
    if (!response.ok) {
        const data = await response.json().catch(() => null)
        const error = new Error(data?.detail ?? `Failed to create booking: ${response.status}`) as Error & { status: number }
        error.status = response.status
        throw error
    }
    return response.json()
}

export async function getCurrentBookings(): Promise<BookingListItem[]> {
    const response = await fetch(`${API_BASE_URL}/api/bookings/current`, {
        headers: { ...getAuthHeader() },
    })
    if (!response.ok) {
        throw new Error(`Failed to fetch current bookings: ${response.status}`)
    }
    return response.json()
}

export async function getBookingHistory(): Promise<BookingListItem[]> {
    const response = await fetch(`${API_BASE_URL}/api/bookings/history`, {
        headers: { ...getAuthHeader() },
    })
    if (!response.ok) {
        throw new Error(`Failed to fetch booking history: ${response.status}`)
    }
    return response.json()
}

export async function cancelBooking(bookingId: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/api/bookings/${bookingId}/cancel`, {
        method: 'POST',
        headers: { ...getAuthHeader() },
    })
    if (!response.ok) {
        const data = await response.json().catch(() => null)
        const error = new Error(data?.detail ?? `Failed to cancel booking: ${response.status}`) as Error & { status: number }
        error.status = response.status
        throw error
    }
}
