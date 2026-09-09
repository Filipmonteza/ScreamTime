import type { Movie } from '../types/movie'
import { getAuthHeader } from './authHeader'

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

export async function createMovie(movie: {
    title: string
    ageRating: number
    runtimeMinutes: number
    description: string
    genreIds: string[]
}): Promise<Movie> {
    const response = await fetch(`${API_BASE_URL}/api/movies`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
        body: JSON.stringify(movie),
    })
    if (!response.ok) {
        throw new Error(`Failed to create movie: ${response.status}`)
    }
    return response.json()
}

export async function updateMovie(id: string, movie: {
    title: string
    ageRating: number
    runtimeMinutes: number
    description: string
    isActive: boolean
    genreIds: string[]
}): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/api/movies/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
        body: JSON.stringify(movie),
    })
    if (!response.ok) {
        throw new Error(`Failed to update movie: ${response.status}`)
    }
}