import type { Genre } from '../types/genre'

const API_BASE_URL = 'http://localhost:5267'

export async function getAllGenres(): Promise<Genre[]> {
  const response = await fetch(`${API_BASE_URL}/api/genres`)
  if (!response.ok) {
    throw new Error(`Failed to fetch genres: ${response.status}`)
  }
  return response.json()
}