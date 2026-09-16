import type { AdminUser } from "../types/admin";
import { getAuthHeader } from "./authHeader";

const API_BASE_URL = "http://localhost:5267";

function jsonHeaders() {
  return { "Content-Type": "application/json", ...getAuthHeader() };
}

export async function getAllAdminUsers(): Promise<AdminUser[]> {
  const response = await fetch(`${API_BASE_URL}/api/admin/users`, {
    headers: jsonHeaders(),
  });
  if (!response.ok) {
    throw new Error(`Failed to load users: ${response.status}`);
  }
  return response.json();
}

export async function createAdminUser(
  email: string,
  password: string,
  roles: string[],
) {
  const response = await fetch(`${API_BASE_URL}/api/admin/users`, {
    method: "POST",
    headers: jsonHeaders(),
    body: JSON.stringify({ email, password, roles }),
  });
  if (!response.ok) {
    const data = await response.json().catch(() => null);
    throw new Error(data?.errors?.[0] ?? data?.error ?? `Failed to create user: ${response.status}`);
  }
}

export async function updateAdminUser(id: string, newEmail?: string, newPassword?: string) {
  const response = await fetch(`${API_BASE_URL}/api/admin/users/${id}`, {
    method: "PUT",
    headers: jsonHeaders(),
    body: JSON.stringify({ newEmail: newEmail ?? null, newPassword: newPassword ?? null }),
  });
  if (!response.ok) {
    const data = await response.json().catch(() => null);
    throw new Error(data?.errors?.[0] ?? data?.error ?? `Failed to update user: ${response.status}`);
  }
}

export async function updateAdminUserRoles(id: string, roles: string[]) {
  const response = await fetch(`${API_BASE_URL}/api/admin/users/${id}/roles`, {
    method: "POST",
    headers: jsonHeaders(),
    body: JSON.stringify({ roles }),
  });
  if (!response.ok) {
    const data = await response.json().catch(() => null);
    throw new Error(data?.errors?.[0] ?? data?.error ?? `Failed to update roles: ${response.status}`);
  }
}

export async function deleteAdminUser(id: string) {
  const response = await fetch(`${API_BASE_URL}/api/admin/users/${id}`, {
    method: "DELETE",
    headers: getAuthHeader(),
  });
  if (!response.ok) {
    throw new Error(`Failed to delete user: ${response.status}`);
  }
}

export async function reactivateAdminUser(id: string) {
  const response = await fetch(`${API_BASE_URL}/api/admin/users/${id}/reactivate`, {
    method: "POST",
    headers: getAuthHeader(),
  });
  if (!response.ok) {
    throw new Error(`Failed to reactivate user: ${response.status}`);
  }
}
