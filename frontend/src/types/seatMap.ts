export interface SeatAvailability {
    seatId: string
    rowLabel: string
    seatNumber: number
    seatType: string
    price: number
    isAvailable: boolean
}

export interface ScreeningSeatMap {
    screeningId: string
    movieTitle: string
    screeningStartTime: string
    auditoriumName: string
    seats: SeatAvailability[]
}