import type { ScreeningSeatMap } from '../types/seatMap'
import { getAuthHeader } from './authHeader'
import type { BookingConfirmation, BookingListItem } from '../types/booking'
import type { PagedBookingResult } from '../types/booking'
import { extractErrorMessage } from './apiError'

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
        const error = new Error(await extractErrorMessage(response, `Failed to create booking: ${response.status}`)) as Error & { status: number }
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
        const error = new Error(await extractErrorMessage(response, `Failed to cancel booking: ${response.status}`)) as Error & { status: number }
        error.status = response.status
        throw error
    }
}

export async function searchBookings(
    search: string,
    page = 1,
    pageSize = 12,
): Promise<PagedBookingResult> {
    const url = new URL(`${API_BASE_URL}/api/bookings/search`)
    url.searchParams.set('search', search)
    url.searchParams.set('page', String(page))
    url.searchParams.set('pageSize', String(pageSize))

    const response = await fetch(url.toString(), {
        headers: { ...getAuthHeader() },
    })
    if (!response.ok) {
        throw new Error(`Failed to search bookings: ${response.status}`)
    }
    return response.json()
}

