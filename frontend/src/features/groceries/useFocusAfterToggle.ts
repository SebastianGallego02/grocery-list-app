import { useCallback, useLayoutEffect, useRef } from 'react'
import type { GroceryView } from './groceryViews'
import type { GroceryItem } from './types'

type FocusTarget = { kind: 'item'; id: string } | { kind: 'empty' }

/**
 * Toggling an item takes its focused button away: in a filtered view the row disappears, and in
 * "All" React moves the row's DOM node, which blurs it. Before the toggle, `prepare` picks where
 * focus should go; after the re-render, focus moves there (rows mark their toggle with
 * data-toggle-for):
 * - "All": the same item's toggle, at its new position;
 * - filtered views: the next row's toggle, the previous one if it was the last row, or the empty
 *   state's heading if the view becomes empty.
 */
export function useFocusAfterToggle(visibleItems: GroceryItem[], view: GroceryView) {
  const listRef = useRef<HTMLUListElement>(null)
  const emptyHeadingRef = useRef<HTMLHeadingElement>(null)
  const target = useRef<FocusTarget | null>(null)

  const prepare = useCallback(
    (item: GroceryItem) => {
      const index = visibleItems.findIndex((visible) => visible.id === item.id)
      if (index === -1) {
        return
      }
      if (view === 'all') {
        target.current = { kind: 'item', id: item.id }
        return
      }
      const neighbour = visibleItems[index + 1] ?? visibleItems[index - 1]
      target.current = neighbour ? { kind: 'item', id: neighbour.id } : { kind: 'empty' }
    },
    [visibleItems, view],
  )

  // No dependency list on purpose: it runs after every render and acts only when a target is set.
  useLayoutEffect(() => {
    const next = target.current
    if (!next) {
      return
    }
    target.current = null
    if (next.kind === 'empty') {
      emptyHeadingRef.current?.focus()
    } else {
      listRef.current?.querySelector<HTMLElement>(`[data-toggle-for="${next.id}"]`)?.focus()
    }
  })

  return { listRef, emptyHeadingRef, prepare }
}
