import { BasketIcon, EmptyState, ErrorBanner, SkeletonList } from '../../components'
import { AddItemForm } from './AddItemForm'
import { GroceryItemRow } from './GroceryItemRow'
import styles from './GroceryListPage.module.scss'
import { ProgressSummary } from './ProgressSummary'
import { useGroceryList } from './useGroceryList'

export function GroceryListPage() {
  const list = useGroceryList()

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
          <h2 id="products-heading" className={styles.sectionTitle}>
            Products <span className={styles.count}>({list.toBuy.length} to buy)</span>
          </h2>
          {/* One list, items in the basket last and struck through. Keeping every row in the same
              list lets React move it instead of remounting it, so keyboard focus survives a toggle. */}
          <ul className={styles.items}>
            {[...list.toBuy, ...list.inBasket].map((item) => (
              <GroceryItemRow
                key={item.id}
                item={item}
                isPending={list.isPending(item.id)}
                onToggleBasket={list.toggleBasket}
                onRemove={list.removeItem}
              />
            ))}
          </ul>
        </section>
      )}
    </div>
  )
}
