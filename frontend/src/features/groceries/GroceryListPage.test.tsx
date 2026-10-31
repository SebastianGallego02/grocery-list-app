import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { RouterProvider, createMemoryRouter } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import * as groceriesApi from './groceriesApi'
import { GroceryListPage } from './GroceryListPage'
import type { GroceryItem } from './types'

vi.mock('./groceriesApi')
const api = vi.mocked(groceriesApi)

function item(name: string, isInBasket = false): GroceryItem {
  return { id: `${name}-id`, name, quantity: 1, isInBasket, createdAtUtc: '2026-01-01T12:00:00Z' }
}

async function renderPage(items: GroceryItem[], path = '/') {
  api.getItems.mockResolvedValue(items)
  const router = createMemoryRouter([{ path: '/', element: <GroceryListPage /> }], { initialEntries: [path] })
  render(<RouterProvider router={router} />)
  await screen.findByRole('tablist')
  return router
}

/** Product names in the visible panel, read from each row's toggle ("Put in basket Milk"). */
function visibleProducts(): string[] {
  return within(screen.getByRole('tabpanel'))
    .queryAllByRole('button', { name: /^(Put in basket|Put back) / })
    .map((toggle) => toggle.textContent!.replace(/^(Put in basket|Put back) /, ''))
}

function selectedTab(): string {
  return screen.getByRole('tab', { selected: true }).textContent ?? ''
}

const shopping = [item('Milk'), item('Bread'), item('Eggs', true)]

describe('GroceryListPage view filter', () => {
  beforeEach(() => {
    vi.resetAllMocks()
  })

  it('shows live counts in the tabs and opens on "To buy" by default', async () => {
    await renderPage(shopping)

    expect(screen.getAllByRole('tab').map((tab) => tab.textContent)).toEqual(['To buy (2)', 'In basket (1)', 'All (3)'])
    expect(selectedTab()).toBe('To buy (2)')
    expect(visibleProducts()).toEqual(['Milk', 'Bread'])
    expect(screen.getByRole('tabpanel').getAttribute('aria-labelledby')).toBe(
      screen.getByRole('tab', { selected: true }).id,
    )
  })

  it('falls back to "To buy" for an unknown view in the URL', async () => {
    await renderPage(shopping, '/?view=everything')

    expect(selectedTab()).toBe('To buy (2)')
    expect(visibleProducts()).toEqual(['Milk', 'Bread'])
  })

  it('filters per view and keeps basket items last in "All"', async () => {
    await renderPage([item('Eggs', true), item('Milk'), item('Bread')], '/?view=in-basket')
    expect(selectedTab()).toBe('In basket (1)')
    expect(visibleProducts()).toEqual(['Eggs'])

    await userEvent.setup().click(screen.getByRole('tab', { name: 'All (3)' }))

    expect(visibleProducts()).toEqual(['Milk', 'Bread', 'Eggs'])
  })

  it('stores the selected view in the URL, so the back button returns to the previous one', async () => {
    const router = await renderPage(shopping)

    await userEvent.setup().click(screen.getByRole('tab', { name: 'In basket (1)' }))
    expect(router.state.location.search).toBe('?view=in-basket')

    await router.navigate(-1)
    expect(await screen.findByRole('tab', { name: 'To buy (2)', selected: true })).toBeTruthy()
  })

  it('moves between tabs with the arrow keys (wrapping), Home and End', async () => {
    const user = userEvent.setup()
    await renderPage(shopping)

    // Only the selected tab is in the Tab order.
    expect(screen.getAllByRole('tab').map((tab) => tab.tabIndex)).toEqual([0, -1, -1])

    screen.getByRole('tab', { selected: true }).focus()
    await user.keyboard('{ArrowRight}')
    expect(document.activeElement?.textContent).toBe('In basket (1)')
    expect(selectedTab()).toBe('In basket (1)')

    await user.keyboard('{End}')
    expect(selectedTab()).toBe('All (3)')

    await user.keyboard('{ArrowRight}')
    expect(selectedTab()).toBe('To buy (2)')

    await user.keyboard('{ArrowLeft}')
    expect(selectedTab()).toBe('All (3)')

    await user.keyboard('{Home}')
    expect(selectedTab()).toBe('To buy (2)')
    expect(document.activeElement).toBe(screen.getByRole('tab', { selected: true }))
  })

  it('says everything is in the basket when "To buy" is empty', async () => {
    await renderPage([item('Milk', true)])

    expect(screen.getByRole('heading', { name: 'Everything is in your basket.' })).toBeTruthy()
  })

  it('says nothing is in the basket yet when "In basket" is empty', async () => {
    await renderPage([item('Bread')], '/?view=in-basket')

    expect(screen.getByRole('heading', { name: 'Nothing in your basket yet.' })).toBeTruthy()
  })
})
