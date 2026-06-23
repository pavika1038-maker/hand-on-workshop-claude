/**
 * api.ts — fetch wrapper ที่ inject X-Employee-Id header โดยอัตโนมัติ
 * อ่าน employeeId จาก sessionStorage (auth_user) ที่ AuthContext เก็บไว้
 */

function getEmployeeId(): string {
  try {
    const raw = sessionStorage.getItem('auth_user')
    if (!raw) return ''
    const user = JSON.parse(raw) as { employeeId?: string }
    return user.employeeId ?? ''
  } catch {
    return ''
  }
}

function buildHeaders(extra?: HeadersInit): HeadersInit {
  const eid = getEmployeeId()
  return {
    'Content-Type': 'application/json',
    ...(eid ? { 'X-Employee-Id': eid } : {}),
    ...(extra ?? {}),
  }
}

export async function apiFetch(url: string, init?: RequestInit): Promise<Response> {
  return fetch(url, {
    ...init,
    headers: buildHeaders(init?.headers as HeadersInit),
  })
}

export async function apiGet<T>(url: string): Promise<T> {
  const res = await apiFetch(url)
  return res.json() as Promise<T>
}

export async function apiPost<T>(url: string, body: unknown): Promise<T> {
  const res = await apiFetch(url, {
    method: 'POST',
    body: JSON.stringify(body),
  })
  return res.json() as Promise<T>
}

export async function apiPatch<T>(url: string, body?: unknown): Promise<T> {
  const res = await apiFetch(url, {
    method: 'PATCH',
    body: body !== undefined ? JSON.stringify(body) : undefined,
  })
  return res.json() as Promise<T>
}

/** สำหรับ multipart/form-data upload — ไม่ set Content-Type (browser จัดการ boundary เอง) */
export async function apiUploadFile(url: string, form: FormData): Promise<Response> {
  const eid = getEmployeeId()
  return fetch(url, {
    method: 'POST',
    body: form,
    headers: eid ? { 'X-Employee-Id': eid } : undefined,
  })
}
