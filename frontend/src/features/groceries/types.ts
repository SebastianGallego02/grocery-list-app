/** Mirrors GroceryList.Application.GroceryItems.GroceryItemResponse. */
export interface GroceryItem {
  id: string
  name: string
  quantity: number
  /** True when the product has been picked up (tagged). */
  isInBasket: boolean
  createdAtUtc: string
}

/** Mirrors GroceryList.Application.GroceryItems.AddGroceryItemRequest. */
export interface AddGroceryItemRequest {
  name: string
  quantity: number
}
