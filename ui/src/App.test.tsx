import { render, screen } from '@testing-library/react'
import { configureStore } from '@reduxjs/toolkit'
import { Provider } from 'react-redux'
import App from './App'
import appReducer from './slices/slice'
import type { Choice, PlayResult, ScoreEntry } from './types/types'

function renderWithProviders(
  preloadedState: {
    app: {
      results: PlayResult | null
      choices: Choice[]
      scoreboard: ScoreEntry[] | null
      loading: boolean
      error: string | null
      loadingChoices: boolean
      errorChoices: string | null
      loadingScoreboard: boolean
      errorScoreboard: string | null
    }
  }
) {
  const store = configureStore({
    reducer: { app: appReducer },
    preloadedState,
  })

  return render(
    <Provider store={store}>
      <App />
    </Provider>
  )
}

describe('App', () => {
  const mockChoices: Choice[] = [
    { id: 1, name: 'rock' },
    { id: 2, name: 'paper' },
    { id: 3, name: 'scissors' },
    { id: 4, name: 'lizard' },
    { id: 5, name: 'spock' },
  ]

  const mockScoreboard: ScoreEntry[] = [
    {
      id: 1,
      result: 'win',
      player: 1,
      playerName: 'rock',
      computer: 2,
      computerName: 'paper',
      playedAt: '2024-01-01T00:00:00Z',
    },
  ]

  const mockResult: PlayResult = {
    results: 'win',
    player: 1,
    computer: 2,
  }

  test('renders choices as buttons when choices are loaded', () => {
    renderWithProviders({
      app: {
        results: null,
        choices: mockChoices,
        scoreboard: null,
        loading: false,
        error: null,
        loadingChoices: false,
        errorChoices: null,
        loadingScoreboard: false,
        errorScoreboard: null,
      },
    })

    mockChoices.forEach(choice => {
      expect(screen.getByRole('button', { name: new RegExp(choice.name, 'i') })).toBeInTheDocument()
    })
  })

  test('displays results when there is a play result', () => {
    renderWithProviders({
      app: {
        results: mockResult,
        choices: mockChoices,
        scoreboard: null,
        loading: false,
        error: null,
        loadingChoices: false,
        errorChoices: null,
        loadingScoreboard: false,
        errorScoreboard: null,
      },
    })

    expect(screen.getByText('You Win!')).toBeInTheDocument()
    expect(screen.getByText('You: rock')).toBeInTheDocument()
    expect(screen.getByText('Computer: paper')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /play again/i })).toBeInTheDocument()
  })

  test('renders scoreboard when scoreboard data is available', () => {
    renderWithProviders({
      app: {
        results: null,
        choices: mockChoices,
        scoreboard: mockScoreboard,
        loading: false,
        error: null,
        loadingChoices: false,
        errorChoices: null,
        loadingScoreboard: false,
        errorScoreboard: null,
      },
    })

    expect(screen.getByText('Recent Games')).toBeInTheDocument()
    expect(screen.getByText('Win')).toBeInTheDocument()
    expect(screen.getByText('✊ rock')).toBeInTheDocument()
    expect(screen.getByText('✋ paper')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /reset/i })).toBeInTheDocument()
  })
})