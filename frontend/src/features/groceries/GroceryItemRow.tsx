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
          action plus the product name ("Put in basket Milk"), so each one is unique. */}
      <button type="button" className={styles.toggle} disabled={isPending} onClick={() => onToggleBasket(item)}>
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
      <Button size="icon" variant="danger" disabled={isPending} onClick={() => onRemove(item)}>
        <TrashIcon />
        <span className="visually-hidden">Remove {item.name}</span>
      </Button>
    </li>
  )
}
