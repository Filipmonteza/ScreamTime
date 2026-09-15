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

export type BookingStatus = (typeof BookingStatus)[keyof typeof BookingStatus];
