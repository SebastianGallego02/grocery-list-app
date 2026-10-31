import { http } from '../../api/httpClient'
import type { AddGroceryItemRequest, GroceryItem } from './types'

const BASE = '/api/grocery-items'

export function getItems(signal?: AbortSignal): Promise<GroceryItem[]> {
  return http.get<GroceryItem[]>(BASE, { signal })
}

export function addItem(request: AddGroceryItemRequest, signal?: AbortSignal): Promise<GroceryItem> {
  return http.post<GroceryItem>(BASE, { body: request, signal })
}

export function removeItem(id: string, signal?: AbortSignal): Promise<void> {
  return http.delete<void>(`${BASE}/${encodeURIComponent(id)}`, { signal })
}

export function putInBasket(id: string, signal?: AbortSignal): Promise<void> {
  return http.put<void>(`${BASE}/${encodeURIComponent(id)}/basket`, { signal })
}

export function removeFromBasket(id: string, signal?: AbortSignal): Promise<void> {
  return http.delete<void>(`${BASE}/${encodeURIComponent(id)}/basket`, { signal })
}
