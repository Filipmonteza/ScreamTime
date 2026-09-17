export interface BookedSeat {
    seatId: string
    rowLabel: string
    seatNumber: number
    seatType: string
    seatPrice: number
}

export interface BookingConfirmation {
    bookingId: string
    screeningId: string
    bookingReference: string
    status: BookingStatus
    movieTitle: string
    screeningStartTime: string
    auditoriumName: string
    seats: BookedSeat[]
    totalAmount: number
    createdAt: string
}

export interface BookingListItem {
    bookingId: string
    screeningId: string
    bookingReference: string
    status: BookingStatus
    movieTitle: string
    screeningStartTime: string
    auditoriumName: string
    seats: BookedSeat[]
    totalAmount: number
}

export const BookingStatus = {
    Reserved: 0,
    Paid: 1,
    Cancelled: 2,
} as const;

// --- Admin/staff search (GET /api/bookings/search) ---

export interface BookingSearchSeat {
    rowLabel: string
    seatNumber: number
    price: number
}

export interface BookingSearchItem {
    bookingId: string
    bookingReference: string
    customerEmail: string
    movieTitle: string
    auditoriumName: string
    screeningStartTime: string
    seats: BookingSearchSeat[]
    status: BookingStatus
}

export interface PagedBookingResult {
    items: BookingSearchItem[]
    totalCount: number
    page: number
    pageSize: number
    totalPages: number
}

export type BookingStatus = (typeof BookingStatus)[keyof typeof BookingStatus];
