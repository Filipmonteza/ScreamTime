import type { Movie } from '../types/movie'

const API_BASE_URL = 'http://localhost:5267'

export async function getAllMovies(): Promise<Movie[]> {
    const response = await fetch(`${API_BASE_URL}/api/movies`)
    if (!response.ok) {
        throw new Error(`Failed to fetch movies: ${response.status}`)
    }
    return response.json()
}

export async function getMovieById(id: string): Promise<Movie> {
    const response = await fetch(`${API_BASE_URL}/api/movies/${id}`)
    if (!response.ok) {
        throw new Error(`Failed to fetch movie: ${response.status}`)
    }
    return response.json()
}