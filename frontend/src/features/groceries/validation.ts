// Mirrors GroceryItem.NameMaxLength and GroceryItem.MinQuantity in the backend Domain.
export const NAME_MAX_LENGTH = 100
export const MIN_QUANTITY = 1

export type ItemField = 'name' | 'quantity'

export interface NewItemValues {
  name: string
  /** Raw input value; the number input can be empty or partial while typing. */
  quantity: string
}

export function validateNewItem(values: NewItemValues): Partial<Record<ItemField, string>> {
  const errors: Partial<Record<ItemField, string>> = {}

  const name = values.name.trim()
  if (!name) {
    errors.name = 'Name is required.'
  } else if (name.length > NAME_MAX_LENGTH) {
    errors.name = `Name must be at most ${NAME_MAX_LENGTH} characters.`
  }

  const quantity = Number(values.quantity)
  if (!Number.isInteger(quantity) || quantity < MIN_QUANTITY) {
    errors.quantity = `Quantity must be a whole number of at least ${MIN_QUANTITY}.`
  }

  return errors
}

/** Which form field a backend error code belongs to (e.g. GroceryItems.DuplicateName → name). */
export function fieldForItemError(code: string): ItemField | null {
  if (code.startsWith('GroceryItems.Name') || code === 'GroceryItems.DuplicateName') return 'name'
  if (code.startsWith('GroceryItems.Quantity')) return 'quantity'
  return null
}
