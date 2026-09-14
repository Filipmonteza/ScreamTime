export interface Screening {
    screeningId: string
    movieId: string
    movieTitle: string
    auditoriumId: string
    auditoriumName: string
    startTime: string
    endTime: string
    isCancelled: boolean
    capacity: number
    bookedSeats: number
}