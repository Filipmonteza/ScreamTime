export async function extractErrorMessage(response: Response, fallback: string): Promise<string> {
    const data = await response.json().catch(() => null)
    return data?.detail ?? data?.message ?? data?.errors?.[0]?.description ?? fallback
}