import { act, renderHook, waitFor } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ApiError } from '../../api/httpClient'
import * as groceriesApi from './groceriesApi'
import type { GroceryItem } from './types'
import { useGroceryList } from './useGroceryList'

vi.mock('./groceriesApi')
const api = vi.mocked(groceriesApi)

function item(overrides: Partial<GroceryItem> = {}): GroceryItem {
  return {
    id: 'milk-id',
    name: 'Milk',
    quantity: 1,
    isInBasket: false,
    createdAtUtc: '2026-01-01T12:00:00Z',
    ...overrides,
  }
}

/** A promise the test resolves or rejects by hand, to observe the state while a request is pending. */
function deferred<T = void>() {
  let resolve!: (value: T) => void
  let reject!: (reason: unknown) => void
  const promise = new Promise<T>((res, rej) => {
    resolve = res
    reject = rej
  })
  return { promise, resolve, reject }
}

async function renderLoadedList(items: GroceryItem[]) {
  api.getItems.mockResolvedValue(items)
  const hook = renderHook(() => useGroceryList())
  await waitFor(() => expect(hook.result.current.status).toBe('ready'))
  return hook
}

const serverError = new ApiError(500, 'Server.UnexpectedError', 'Something went wrong on our side.')

describe('useGroceryList', () => {
  beforeEach(() => {
    vi.resetAllMocks()
  })

  it('puts an item in the basket optimistically, before the API answers', async () => {
    const milk = item()
    const { result } = await renderLoadedList([milk])
    const request = deferred()
    api.putInBasket.mockReturnValue(request.promise)

    let toggle!: Promise<void>
    act(() => {
      toggle = result.current.toggleBasket(milk)
    })

    // Flipped immediately and marked as pending.
    expect(result.current.items[0].isInBasket).toBe(true)
    expect(result.current.inBasket).toHaveLength(1)
    expect(result.current.isPending(milk.id)).toBe(true)

    await act(async () => {
      request.resolve()
      await toggle
    })

    expect(api.putInBasket).toHaveBeenCalledWith(milk.id)
    expect(result.current.items[0].isInBasket).toBe(true)
    expect(result.current.isPending(milk.id)).toBe(false)
    expect(result.current.actionError).toBeNull()
  })

  it('rolls back and reports an error when putting in the basket fails', async () => {
    const milk = item()
    const { result } = await renderLoadedList([milk])
    api.putInBasket.mockRejectedValue(serverError)

    await act(() => result.current.toggleBasket(milk))

    expect(result.current.items[0].isInBasket).toBe(false)
    expect(result.current.isPending(milk.id)).toBe(false)
    expect(result.current.actionError).toBe('Could not put “Milk” in the basket. Something went wrong on our side.')
  })

  it('rolls back an un-tag when the API fails', async () => {
    const milk = item({ isInBasket: true })
    const { result } = await renderLoadedList([milk])
    api.removeFromBasket.mockRejectedValue(serverError)

    await act(() => result.current.toggleBasket(milk))

    expect(api.removeFromBasket).toHaveBeenCalledWith(milk.id)
    expect(result.current.items[0].isInBasket).toBe(true)
    expect(result.current.actionError).toContain('Could not put “Milk” back.')
  })

  it('ignores a second toggle while the first request is still pending', async () => {
    const milk = item()
    const { result } = await renderLoadedList([milk])
    const request = deferred()
    api.putInBasket.mockReturnValue(request.promise)

    let first!: Promise<void>
    act(() => {
      first = result.current.toggleBasket(milk)
    })
    await act(() => result.current.toggleBasket({ ...milk, isInBasket: true }))
    await act(async () => {
      request.resolve()
      await first
    })

    expect(api.putInBasket).toHaveBeenCalledOnce()
    expect(api.removeFromBasket).not.toHaveBeenCalled()
    expect(result.current.items[0].isInBasket).toBe(true)
  })

  it('shows a load error and loads again on retry', async () => {
    api.getItems.mockRejectedValueOnce(serverError).mockResolvedValueOnce([item()])
    const { result } = renderHook(() => useGroceryList())

    await waitFor(() => expect(result.current.status).toBe('error'))
    expect(result.current.loadError).toBe('Something went wrong on our side.')

    act(() => result.current.reload())

    await waitFor(() => expect(result.current.status).toBe('ready'))
    expect(result.current.items).toHaveLength(1)
    expect(result.current.loadError).toBeNull()
  })

  it('appends added items and keeps API errors for the form', async () => {
    const { result } = await renderLoadedList([])
    api.addItem.mockResolvedValueOnce(item({ id: 'bread-id', name: 'Bread' }))
    api.addItem.mockRejectedValueOnce(new ApiError(409, 'GroceryItems.DuplicateName', 'Already in your list.'))

    await act(() => result.current.addItem({ name: 'Bread', quantity: 1 }))
    await expect(result.current.addItem({ name: 'bread', quantity: 1 })).rejects.toMatchObject({
      code: 'GroceryItems.DuplicateName',
    })

    expect(result.current.items.map((i) => i.name)).toEqual(['Bread'])
  })

  it('removes an item, and also drops it when the API says it is already gone', async () => {
    const milk = item()
    const bread = item({ id: 'bread-id', name: 'Bread' })
    const { result } = await renderLoadedList([milk, bread])
    api.removeItem.mockResolvedValueOnce(undefined)
    api.removeItem.mockRejectedValueOnce(new ApiError(404, 'GroceryItems.NotFound', 'Not found.'))

    await act(() => result.current.removeItem(milk))
    await act(() => result.current.removeItem(bread))

    expect(result.current.items).toEqual([])
    expect(result.current.actionError).toBeNull()
  })
})
