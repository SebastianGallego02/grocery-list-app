import { describe, expect, it } from 'vitest'
import { countByView, itemsInView, parseView } from './groceryViews'
import type { GroceryItem } from './types'

function item(name: string, isInBasket: boolean): GroceryItem {
  return { id: `${name}-id`, name, quantity: 1, isInBasket, createdAtUtc: '2026-01-01T12:00:00Z' }
}

const milk = item('Milk', false)
const bread = item('Bread', false)
const eggs = item('Eggs', true)
const split = { toBuy: [milk, bread], inBasket: [eggs] }

describe('groceryViews', () => {
  it.each([
    ['to-buy', 'to-buy'],
    ['in-basket', 'in-basket'],
    ['all', 'all'],
    [null, 'to-buy'],
    ['', 'to-buy'],
    ['ALL', 'to-buy'],
    ['basket', 'to-buy'],
  ])('parses %j as the %s view', (value, expected) => {
    expect(parseView(value)).toBe(expected)
  })

  it('counts the items in every view', () => {
    expect(countByView(split)).toEqual({ 'to-buy': 2, 'in-basket': 1, all: 3 })
  })

  it('filters the items for each view, keeping basket items last in "All"', () => {
    expect(itemsInView(split, 'to-buy')).toEqual([milk, bread])
    expect(itemsInView(split, 'in-basket')).toEqual([eggs])
    expect(itemsInView(split, 'all')).toEqual([milk, bread, eggs])
  })
})
