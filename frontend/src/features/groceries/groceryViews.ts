import { useCallback } from 'react'
import { useSearchParams } from 'react-router-dom'
import type { GroceryItem } from './types'

/** Filtered views of the one grocery list. They are derived from the items, never stored separately. */
export const GROCERY_VIEWS = ['to-buy', 'in-basket', 'all'] as const
export type GroceryView = (typeof GROCERY_VIEWS)[number]

export const DEFAULT_VIEW: GroceryView = 'to-buy'
export const VIEW_PARAM = 'view'

export const VIEW_LABELS: Record<GroceryView, string> = {
  'to-buy': 'To buy',
  'in-basket': 'In basket',
  all: 'All',
}

/** Id of a view's tab, so the tabpanel can point at the selected one (aria-labelledby). */
export function tabId(view: GroceryView): string {
  return `grocery-view-tab-${view}`
}

interface SplitItems {
  toBuy: GroceryItem[]
  inBasket: GroceryItem[]
}

/** Unknown or missing values (e.g. a hand-edited URL) fall back to the default view. */
export function parseView(value: string | null): GroceryView {
  return GROCERY_VIEWS.find((view) => view === value) ?? DEFAULT_VIEW
}

export function countByView({ toBuy, inBasket }: SplitItems): Record<GroceryView, number> {
  return { 'to-buy': toBuy.length, 'in-basket': inBasket.length, all: toBuy.length + inBasket.length }
}

/** "All" keeps the items in the basket last, as the unfiltered list always did. */
export function itemsInView({ toBuy, inBasket }: SplitItems, view: GroceryView): GroceryItem[] {
  switch (view) {
    case 'to-buy':
      return toBuy
    case 'in-basket':
      return inBasket
    case 'all':
      return [...toBuy, ...inBasket]
  }
}

/**
 * The selected view lives in the query string (?view=in-basket) so it survives a reload. Every
 * change pushes a history entry, so the back button returns to the previous view.
 */
export function useViewParam(): [GroceryView, (view: GroceryView) => void] {
  const [searchParams, setSearchParams] = useSearchParams()
  const view = parseView(searchParams.get(VIEW_PARAM))

  const setView = useCallback(
    (next: GroceryView) =>
      setSearchParams((current) => {
        const params = new URLSearchParams(current)
        params.set(VIEW_PARAM, next)
        return params
      }),
    [setSearchParams],
  )

  return [view, setView]
}
