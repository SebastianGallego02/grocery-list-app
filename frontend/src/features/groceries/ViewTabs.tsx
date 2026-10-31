import { useRef, type KeyboardEvent } from 'react'
import { GROCERY_VIEWS, VIEW_LABELS, tabId, type GroceryView } from './groceryViews'
import styles from './ViewTabs.module.scss'

interface ViewTabsProps {
  selected: GroceryView
  counts: Record<GroceryView, number>
  onSelect: (view: GroceryView) => void
  /** Id of the tabpanel the tabs control. */
  panelId: string
}

/**
 * WAI-ARIA tabs (not a radio group): the control switches which content panel is shown. Only
 * the selected tab is in the Tab order; arrow keys move between tabs (wrapping) and Home/End jump
 * to the ends. Selection follows focus because switching views is instant.
 */
export function ViewTabs({ selected, counts, onSelect, panelId }: ViewTabsProps) {
  const tabRefs = useRef(new Map<GroceryView, HTMLButtonElement>())

  function handleKeyDown(event: KeyboardEvent<HTMLButtonElement>) {
    const index = GROCERY_VIEWS.indexOf(selected)
    const last = GROCERY_VIEWS.length - 1
    const targets: Partial<Record<string, number>> = {
      ArrowRight: index === last ? 0 : index + 1,
      ArrowLeft: index === 0 ? last : index - 1,
      Home: 0,
      End: last,
    }
    const target = targets[event.key]
    if (target === undefined) {
      return
    }

    event.preventDefault()
    const view = GROCERY_VIEWS[target]
    onSelect(view)
    tabRefs.current.get(view)?.focus()
  }

  return (
    <div role="tablist" aria-label="Filter products" className={styles.tablist}>
      {GROCERY_VIEWS.map((view) => {
        const isSelected = view === selected
        return (
          <button
            key={view}
            ref={(element) => {
              if (element) {
                tabRefs.current.set(view, element)
              } else {
                tabRefs.current.delete(view)
              }
            }}
            type="button"
            role="tab"
            id={tabId(view)}
            aria-selected={isSelected}
            aria-controls={panelId}
            tabIndex={isSelected ? 0 : -1}
            className={styles.tab}
            onClick={() => onSelect(view)}
            onKeyDown={handleKeyDown}
          >
            {VIEW_LABELS[view]} <span className={styles.count}>({counts[view]})</span>
          </button>
        )
      })}
    </div>
  )
}
