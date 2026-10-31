import { BasketIcon, CheckIcon, EmptyState, ErrorBanner, SkeletonList } from '../../components'
import { AddItemForm } from './AddItemForm'
import { GroceryItemRow } from './GroceryItemRow'
import styles from './GroceryListPage.module.scss'
import { countByView, itemsInView, tabId, useViewParam, type GroceryView } from './groceryViews'
import { ProgressSummary } from './ProgressSummary'
import { useGroceryList } from './useGroceryList'
import { ViewTabs } from './ViewTabs'

const PANEL_ID = 'grocery-view-panel'

/** What a filtered view says when it has nothing to show (the list itself is not empty). */
const VIEW_EMPTY: Record<Exclude<GroceryView, 'all'>, { title: string; description: string }> = {
  'to-buy': { title: 'Everything is in your basket.', description: 'Nothing left to buy.' },
  'in-basket': {
    title: 'Nothing in your basket yet.',
    description: 'Tap the circle next to a product when you pick it up.',
  },
}

export function GroceryListPage() {
  const list = useGroceryList()
  const [view, setView] = useViewParam()
  const visibleItems = itemsInView(list, view)

  return (
    <div className={styles.page}>
      <div className={styles.heading}>
        <h1 className={styles.title}>My grocery list</h1>
        {list.status === 'ready' && list.items.length > 0 && (
          <ProgressSummary inBasket={list.inBasket.length} total={list.items.length} />
        )}
      </div>

      <AddItemForm onAdd={list.addItem} />

      {list.actionError && <ErrorBanner message={list.actionError} onDismiss={list.dismissActionError} />}

      {list.status === 'loading' && <SkeletonList label="Loading your list…" />}

      {list.status === 'error' && list.loadError && <ErrorBanner message={list.loadError} onRetry={list.reload} />}

      {list.status === 'ready' && list.items.length === 0 && (
        <EmptyState title="Your list is empty" icon={<BasketIcon />}>
          Add the first product you need above.
        </EmptyState>
      )}

      {list.status === 'ready' && list.items.length > 0 && (
        <section className={styles.section} aria-labelledby="products-heading">
          <h2 id="products-heading" className="visually-hidden">
            Products
          </h2>
          <ViewTabs selected={view} counts={countByView(list)} onSelect={setView} panelId={PANEL_ID} />
          <div id={PANEL_ID} role="tabpanel" aria-labelledby={tabId(view)}>
            {visibleItems.length === 0 && view !== 'all' ? (
              <EmptyState title={VIEW_EMPTY[view].title} icon={view === 'to-buy' ? <CheckIcon /> : <BasketIcon />}>
                {VIEW_EMPTY[view].description}
              </EmptyState>
            ) : (
              // One list for every view, items in the basket last in "All". Rows are keyed by id,
              // so in "All" React moves a toggled row instead of remounting it and focus survives.
              <ul className={styles.items}>
                {visibleItems.map((item) => (
                  <GroceryItemRow
                    key={item.id}
                    item={item}
                    isPending={list.isPending(item.id)}
                    onToggleBasket={list.toggleBasket}
                    onRemove={list.removeItem}
                  />
                ))}
              </ul>
            )}
          </div>
        </section>
      )}
    </div>
  )
}
