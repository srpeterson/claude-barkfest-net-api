import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ForgotPasswordModal } from './ForgotPasswordModal'

describe('ForgotPasswordModal', () => {
  it('renders the heading and a mailto support link', () => {
    render(<ForgotPasswordModal onClose={() => {}} />)
    expect(screen.getByText('Forgot your password?')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'srpeterson@outlook.com' }))
      .toHaveAttribute('href', 'mailto:srpeterson@outlook.com')
  })

  it('calls onClose when the Close button is clicked', async () => {
    const onClose = vi.fn()
    render(<ForgotPasswordModal onClose={onClose} />)
    await userEvent.click(screen.getByRole('button', { name: 'Close' }))
    expect(onClose).toHaveBeenCalledTimes(1)
  })

  it('does not close when clicking inside the dialog card', async () => {
    const onClose = vi.fn()
    render(<ForgotPasswordModal onClose={onClose} />)
    // The card stops propagation, so a click on its content must not reach the backdrop.
    await userEvent.click(screen.getByText('Forgot your password?'))
    expect(onClose).not.toHaveBeenCalled()
  })
})
