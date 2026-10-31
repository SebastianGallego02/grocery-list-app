import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ApiError, NETWORK_ERROR_CODE, configureHttpClient, http } from './httpClient'

const fetchMock = vi.fn<typeof fetch>()

function problemResponse(status: number, problem: object): Response {
  return new Response(JSON.stringify(problem), {
    status,
    headers: { 'Content-Type': 'application/problem+json' },
  })
}

describe('httpClient', () => {
  const onUnauthorized = vi.fn()
  let token: string | null

  beforeEach(() => {
    token = null
    vi.stubGlobal('fetch', fetchMock)
    configureHttpClient({ getToken: () => token, onUnauthorized })
  })

  afterEach(() => {
    vi.unstubAllGlobals()
    fetchMock.mockReset()
    onUnauthorized.mockReset()
  })

  it('sends JSON with the bearer token and returns the parsed body', async () => {
    token = 'abc'
    fetchMock.mockResolvedValue(Response.json({ id: '1' }))

    const result = await http.post<{ id: string }>('/api/grocery-items', { body: { name: 'Milk' } })

    expect(result).toEqual({ id: '1' })
    const [url, init] = fetchMock.mock.calls[0]
    expect(url).toBe('/api/grocery-items')
    expect(init?.body).toBe('{"name":"Milk"}')
    expect(init?.headers).toMatchObject({
      Authorization: 'Bearer abc',
      'Content-Type': 'application/json',
    })
  })

  it('returns undefined for 204 No Content', async () => {
    fetchMock.mockResolvedValue(new Response(null, { status: 204 }))

    await expect(http.delete('/api/grocery-items/1')).resolves.toBeUndefined()
  })

  it('turns ProblemDetails into an ApiError with status, code and detail', async () => {
    fetchMock.mockResolvedValue(
      problemResponse(409, { status: 409, code: 'GroceryItems.DuplicateName', detail: 'Already in your list.' }),
    )

    const error = await http.post('/api/grocery-items', { body: {} }).catch((e: unknown) => e)

    expect(error).toBeInstanceOf(ApiError)
    expect(error).toMatchObject({ status: 409, code: 'GroceryItems.DuplicateName', message: 'Already in your list.' })
  })

  it('notifies onUnauthorized on 401 when a token was sent', async () => {
    token = 'expired'
    fetchMock.mockResolvedValue(problemResponse(401, { status: 401, code: 'Auth.Unauthenticated' }))

    await expect(http.get('/api/grocery-items')).rejects.toBeInstanceOf(ApiError)
    expect(onUnauthorized).toHaveBeenCalledOnce()
  })

  it('does not treat a failed login (401 without token) as an expired session', async () => {
    fetchMock.mockResolvedValue(problemResponse(401, { status: 401, code: 'Auth.InvalidCredentials' }))

    await expect(http.post('/api/auth/login', { body: {} })).rejects.toMatchObject({ code: 'Auth.InvalidCredentials' })
    expect(onUnauthorized).not.toHaveBeenCalled()
  })

  it('reports network failures as an ApiError with status 0', async () => {
    fetchMock.mockRejectedValue(new TypeError('Failed to fetch'))

    await expect(http.get('/api/grocery-items')).rejects.toMatchObject({ status: 0, code: NETWORK_ERROR_CODE })
  })

  it('rethrows aborts unchanged so callers can ignore them', async () => {
    fetchMock.mockRejectedValue(new DOMException('Aborted', 'AbortError'))

    await expect(http.get('/api/grocery-items')).rejects.toMatchObject({ name: 'AbortError' })
  })
})
