import { useRef, useState, type FormEvent } from 'react'
import { ApiError, getErrorMessage } from '../../api/httpClient'
import { Button, ErrorBanner, TextField } from '../../components'
import type { AddGroceryItemRequest } from './types'
import styles from './AddItemForm.module.scss'
import {
  MIN_QUANTITY,
  NAME_MAX_LENGTH,
  fieldForItemError,
  validateNewItem,
  type ItemField,
  type NewItemValues,
} from './validation'

const EMPTY: NewItemValues = { name: '', quantity: '1' }

interface AddItemFormProps {
  onAdd: (request: AddGroceryItemRequest) => Promise<void>
}

export function AddItemForm({ onAdd }: AddItemFormProps) {
  const [values, setValues] = useState<NewItemValues>(EMPTY)
  const [fieldErrors, setFieldErrors] = useState<Partial<Record<ItemField, string>>>({})
  const [formError, setFormError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const nameRef = useRef<HTMLInputElement>(null)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (isSubmitting) {
      return
    }

    const errors = validateNewItem(values)
    setFieldErrors(errors)
    setFormError(null)
    if (Object.keys(errors).length > 0) {
      return
    }

    setIsSubmitting(true)
    try {
      await onAdd({ name: values.name.trim(), quantity: Number(values.quantity) })
      setValues(EMPTY)
      // Ready for the next product without reaching for the mouse.
      nameRef.current?.focus()
    } catch (error) {
      const field = error instanceof ApiError ? fieldForItemError(error.code) : null
      if (field) {
        setFieldErrors({ [field]: getErrorMessage(error) })
      } else {
        setFormError(getErrorMessage(error))
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form className={styles.form} onSubmit={handleSubmit} noValidate aria-label="Add a product">
      {formError && <ErrorBanner message={formError} />}
      <div className={styles.fields}>
        <TextField
          ref={nameRef}
          className={styles.name}
          label="Product"
          name="name"
          placeholder="e.g. Milk"
          autoComplete="off"
          maxLength={NAME_MAX_LENGTH}
          value={values.name}
          onChange={(event) => setValues((current) => ({ ...current, name: event.target.value }))}
          error={fieldErrors.name}
        />
        <TextField
          className={styles.quantity}
          label="Quantity"
          name="quantity"
          type="number"
          inputMode="numeric"
          min={MIN_QUANTITY}
          step={1}
          value={values.quantity}
          onChange={(event) => setValues((current) => ({ ...current, quantity: event.target.value }))}
          error={fieldErrors.quantity}
        />
        <Button type="submit" className={styles.submit} isLoading={isSubmitting}>
          {isSubmitting ? 'Adding…' : 'Add'}
        </Button>
      </div>
    </form>
  )
}
