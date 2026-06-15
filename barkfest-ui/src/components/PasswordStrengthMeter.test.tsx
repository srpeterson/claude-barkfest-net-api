import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { PasswordStrengthMeter } from './PasswordStrengthMeter'

describe('PasswordStrengthMeter', () => {
  it('shows the label for the lowest score', () => {
    render(<PasswordStrengthMeter score={0} />)
    expect(screen.getByText('Very weak')).toBeInTheDocument()
  })

  it('shows the label for the highest score', () => {
    render(<PasswordStrengthMeter score={4} />)
    expect(screen.getByText('Very strong')).toBeInTheDocument()
  })

  it('always renders four strength bars', () => {
    const { container } = render(<PasswordStrengthMeter score={2} />)
    expect(container.querySelectorAll('.flex-1')).toHaveLength(4)
  })
})
