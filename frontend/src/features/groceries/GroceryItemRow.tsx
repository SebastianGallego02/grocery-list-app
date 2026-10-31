import { Button } from '../../components'
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
      <div className={styles.details}>
        <span className={styles.name}>{item.name}</span>
        {/* Strike-through is visual only; say it for screen readers too. */}
        {item.isInBasket && <span className="visually-hidden">(in basket)</span>}
        <span className={styles.quantity}>
          <span className="visually-hidden">Quantity: </span>× {item.quantity}
        </span>
      </div>
      <div className={styles.actions}>
        {/* The hidden product name makes each button's accessible name unique ("Put in basket Milk"). */}
        <Button
          size="small"
          variant={item.isInBasket ? 'secondary' : 'primary'}
          disabled={isPending}
          onClick={() => onToggleBasket(item)}
        >
          {item.isInBasket ? 'Put back' : 'Put in basket'}
          <span className="visually-hidden"> {item.name}</span>
        </Button>
        <Button size="small" variant="danger" disabled={isPending} onClick={() => onRemove(item)}>
          Remove<span className="visually-hidden"> {item.name}</span>
        </Button>
      </div>
    </li>
  )
}
