import { Button, CheckIcon, TrashIcon } from '../../components'
import type { GroceryItem } from './types'
import styles from './GroceryItemRow.module.scss'

interface GroceryItemRowProps {
  item: GroceryItem
  isPending: boolean
  onToggleBasket: (item: GroceryItem) => void
  onRemove: (item: GroceryItem) => void
}

export function GroceryItemRow({ item, isPending, onToggleBasket, onRemove }: GroceryItemRowProps) {
  return (
    <li className={[styles.row, item.isInBasket ? styles.inBasket : ''].filter(Boolean).join(' ')} aria-busy={isPending || undefined}>
      {/* Large round toggle, the most frequent action while shopping. Its accessible name is the
          action plus the product name ("Put in basket Milk"), so each one is unique.
          While a request is pending the buttons are aria-disabled rather than disabled: a disabled
          button drops keyboard focus, and the ViewModel already ignores the extra clicks. */}
      <button
        type="button"
        className={styles.toggle}
        aria-disabled={isPending || undefined}
        onClick={() => onToggleBasket(item)}
        data-toggle-for={item.id}
      >
        <span className={styles.check}>
          <CheckIcon />
        </span>
        <span className="visually-hidden">
          {item.isInBasket ? 'Put back' : 'Put in basket'} {item.name}
        </span>
      </button>
      <div className={styles.details}>
        <span className={styles.name}>{item.name}</span>
        {/* Strike-through is visual only; say it for screen readers too. */}
        {item.isInBasket && <span className="visually-hidden">(in basket)</span>}
        <span className={styles.quantity}>
          <span className="visually-hidden">Quantity: </span>× {item.quantity}
        </span>
      </div>
      <Button size="icon" variant="danger" aria-disabled={isPending || undefined} onClick={() => onRemove(item)}>
        <TrashIcon />
        <span className="visually-hidden">Remove {item.name}</span>
      </Button>
    </li>
  )
}
