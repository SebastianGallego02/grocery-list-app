/**
 * Thin, typed wrapper around fetch for the Grocery List API.
 *
 * - Prefixes paths with VITE_API_BASE_URL and sends/receives JSON.
 * - Adds the Bearer token supplied by the auth feature (see configureHttpClient).
 * - Turns error responses (ProblemDetails) into a typed ApiError.
 * - On 401 for a request that carried a token, notifies the auth feature so it clears the session;
 *   the protected routes then send the user back to the login page.
 *
 * It knows nothing about features: auth plugs in through configureHttpClient, so dependencies
 * point from features to this module and never the other way.
 */

/** The RFC 9457 fields the API returns, plus its machine-readable "code" extension. */
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  code?: string
}

export const NETWORK_ERROR_CODE = 'Network.Unreachable'

export class ApiError extends Error {
  readonly status: number
  readonly code: string

  constructor(status: number, code: string, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = code
  }
}

export interface HttpClientConfig {
  getToken: () => string | null
  onUnauthorized: () => void
}

export interface RequestOptions {
  body?: unknown
  signal?: AbortSignal
}

let config: HttpClientConfig = {
  getToken: () => null,
  onUnauthorized: () => {},
}

export function configureHttpClient(next: HttpClientConfig): void {
  config = next
}

export function isAbortError(error: unknown): boolean {
  return error instanceof DOMException && error.name === 'AbortError'
}

const baseUrl = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/+$/, '')

async function request<T>(method: string, path: string, options: RequestOptions = {}): Promise<T> {
  const { body, signal } = options
  const token = config.getToken()

  const headers: Record<string, string> = { Accept: 'application/json' }
  if (body !== undefined) {
    headers['Content-Type'] = 'application/json'
  }
  if (token) {
    headers.Authorization = `Bearer ${token}`
  }

  let response: Response
  try {
    response = await fetch(`${baseUrl}${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
      signal,
    })
  } catch (error) {
    if (isAbortError(error)) {
      throw error
    }
    throw new ApiError(0, NETWORK_ERROR_CODE, 'Could not reach the server. Check your connection and try again.')
  }

  if (!response.ok) {
    const error = await toApiError(response)
    // Only an authenticated request means "your session is no longer valid";
    // a 401 from the login endpoint just means wrong credentials.
    if (response.status === 401 && token) {
      config.onUnauthorized()
    }
    throw error
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

async function toApiError(response: Response): Promise<ApiError> {
  let problem: ProblemDetails | undefined
  if (response.headers.get('Content-Type')?.includes('json')) {
    try {
      problem = (await response.json()) as ProblemDetails
    } catch {
      // Body is not valid JSON: fall back to the status-based message below.
    }
  }

  return new ApiError(
    response.status,
    problem?.code ?? `Http.${response.status}`,
    problem?.detail ?? problem?.title ?? fallbackMessage(response.status),
  )
}

function fallbackMessage(status: number): string {
  if (status === 401) return 'Your session has expired. Please log in again.'
  if (status >= 500) return 'Something went wrong on our side. Please try again.'
  return 'The request could not be completed.'
}

export const http = {
  get: <T>(path: string, options?: RequestOptions) => request<T>('GET', path, options),
  post: <T>(path: string, options?: RequestOptions) => request<T>('POST', path, options),
  put: <T>(path: string, options?: RequestOptions) => request<T>('PUT', path, options),
  delete: <T>(path: string, options?: RequestOptions) => request<T>('DELETE', path, options),
}
