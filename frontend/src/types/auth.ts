export interface AuthResponse {
    token: string
    expiration: string
    userId: string
    email: string
    role: string | null
}