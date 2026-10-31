import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { ApiError, getErrorMessage, isAbortError } from '../../api/httpClient'
import * as groceriesApi from './groceriesApi'
import type { AddGroceryItemRequest, GroceryItem } from './types'

export type ListStatus = 'loading' | 'error' | 'ready'

export interface GroceryListViewModel {
  status: ListStatus
  /** Why the list could not be loaded (status "error"). */
  loadError: string | null
  items: GroceryItem[]
  toBuy: GroceryItem[]
  inBasket: GroceryItem[]
  /** Why the last remove/tag/un-tag failed; the change has already been rolled back. */
  actionError: string | null
  /** True while a request for this item is in flight; its actions are disabled meanwhile. */
  isPending: (id: string) => boolean
  reload: () => void
  /** Rejects with an ApiError so the form can show it next to the right field. */
  addItem: (request: AddGroceryItemRequest) => Promise<void>
  removeItem: (item: GroceryItem) => Promise<void>
  toggleBasket: (item: GroceryItem) => Promise<void>
  dismissActionError: () => void
}

/**
 * ViewModel for the grocery list page: owns the items, the loading/error state and every action,
 * so the components only render. Tagging is optimistic (the UI flips immediately and rolls back if
 * the API fails), which keeps the most frequent action instant while shopping.
 */
export function useGroceryList(): GroceryListViewModel {
  const [items, setItems] = useState<GroceryItem[]>([])
  const [status, setStatus] = useState<ListStatus>('loading')
  const [loadError, setLoadError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [reloadCount, setReloadCount] = useState(0)

  // The ref is the synchronous guard against double clicks; the state re-renders the buttons.
  const pendingRef = useRef(new Set<string>())
  const [pendingIds, setPendingIds] = useState<ReadonlySet<string>>(() => new Set())

  useEffect(() => {
    const controller = new AbortController()

    groceriesApi
      .getItems(controller.signal)
      .then((loaded) => {
        setItems(loaded)
        setStatus('ready')
      })
      .catch((error: unknown) => {
        if (isAbortError(error)) {
          return
        }
        setLoadError(getErrorMessage(error, 'Could not load your list. Please try again.'))
        setStatus('error')
      })

    return () => controller.abort()
  }, [reloadCount])

  const reload = useCallback(() => {
    setStatus('loading')
    setLoadError(null)
    setReloadCount((count) => count + 1)
  }, [])

  const begin = useCallback((id: string): boolean => {
    if (pendingRef.current.has(id)) {
      return false
    }
    pendingRef.current.add(id)
    setPendingIds(new Set(pendingRef.current))
    return true
  }, [])

  const end = useCallback((id: string) => {
    pendingRef.current.delete(id)
    setPendingIds(new Set(pendingRef.current))
  }, [])

  const setInBasket = (id: string, isInBasket: boolean) =>
    setItems((current) => current.map((item) => (item.id === id ? { ...item, isInBasket } : item)))

  const toggleBasket = useCallback(
    async (item: GroceryItem) => {
      if (!begin(item.id)) {
        return
      }

      const target = !item.isInBasket
      setActionError(null)
      setInBasket(item.id, target)

      try {
        await (target ? groceriesApi.putInBasket(item.id) : groceriesApi.removeFromBasket(item.id))
      } catch (error) {
        setInBasket(item.id, !target)
        const action = target ? `put “${item.name}” in the basket` : `put “${item.name}” back`
        setActionError(`Could not ${action}. ${getErrorMessage(error)}`)
      } finally {
        end(item.id)
      }
    },
    [begin, end],
  )

  const removeItem = useCallback(
    async (item: GroceryItem) => {
      if (!begin(item.id)) {
        return
      }

      setActionError(null)
      try {
        await groceriesApi.removeItem(item.id)
        setItems((current) => current.filter((existing) => existing.id !== item.id))
      } catch (error) {
        if (error instanceof ApiError && error.status === 404) {
          // Already gone (e.g. removed in another tab): the goal is reached, just drop it.
          setItems((current) => current.filter((existing) => existing.id !== item.id))
        } else {
          setActionError(`Could not remove “${item.name}”. ${getErrorMessage(error)}`)
        }
      } finally {
        end(item.id)
      }
    },
    [begin, end],
  )

  const addItem = useCallback(async (request: AddGroceryItemRequest) => {
    setActionError(null)
    const created = await groceriesApi.addItem(request)
    setItems((current) => [...current, created])
  }, [])

  const dismissActionError = useCallback(() => setActionError(null), [])
  const isPending = useCallback((id: string) => pendingIds.has(id), [pendingIds])

  const toBuy = useMemo(() => items.filter((item) => !item.isInBasket), [items])
  const inBasket = useMemo(() => items.filter((item) => item.isInBasket), [items])

  return {
    status,
    loadError,
    items,
    toBuy,
    inBasket,
    actionError,
    isPending,
    reload,
    addItem,
    removeItem,
    toggleBasket,
    dismissActionError,
  }
}
