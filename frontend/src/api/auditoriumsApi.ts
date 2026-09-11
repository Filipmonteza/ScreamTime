import type { Auditorium } from '../types/auditorium'
import { getAuthHeader } from './authHeader'

const API_BASE_URL = 'http://localhost:5267'

export async function getAllAuditoriums(includeInactive = false): Promise<Auditorium[]> {
    const response = await fetch(`${API_BASE_URL}/api/auditoriums?includeInactive=${includeInactive}`, {
        headers: { ...getAuthHeader() },
    })
    if (!response.ok) {
        throw new Error(`Failed to fetch auditoriums: ${response.status}`)
    }
    return response.json()
}

export async function createAuditorium(auditorium: { name: string }): Promise<Auditorium> {
    const response = await fetch(`${API_BASE_URL}/api/auditoriums`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
        body: JSON.stringify(auditorium),
    })
    if (!response.ok) {
        throw new Error(`Failed to create auditorium: ${response.status}`)
    }
    return response.json()
}

export async function updateAuditorium(id: string, auditorium: { name: string; isActive: boolean }): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/api/auditoriums/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
        body: JSON.stringify(auditorium),
    })
    if (!response.ok) {
        throw new Error(`Failed to update auditorium: ${response.status}`)
    }
}

export async function deleteAuditorium(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/api/auditoriums/${id}`, {
        method: 'DELETE',
        headers: { ...getAuthHeader() },
    })
    if (!response.ok) {
        throw new Error(`Failed to delete auditorium: ${response.status}`)
    }
}