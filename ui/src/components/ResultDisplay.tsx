import type { PlayResult, Choice } from '../types/types'

interface Props {
  result: PlayResult
  choices: Choice[]
}

const ICONS: Record<string, string> = {
  rock: '✊',
  paper: '✋',
  scissors: '✌️',
  lizard: '🦎',
  spock: '🖖',
}

const RESULT_CONFIG = {
  win:  { label: 'You Win!',  className: 'result--win'  },
  lose: { label: 'You Lose!', className: 'result--lose' },
  tie:  { label: "It's a Tie!", className: 'result--tie'  },
}

export function ResultDisplay({ result, choices }: Props) {
  const playerChoice  = choices.find(c => c.id === result.player)
  const computerChoice = choices.find(c => c.id === result.computer)
  const config = RESULT_CONFIG[result.results]

  return (
    <div className={`result-display ${config.className}`}>
      <h2 className="result-display__outcome">{config.label}</h2>
      <div className="result-display__matchup">
        <div className="result-display__side">
          <span className="result-display__icon">{ICONS[playerChoice?.name ?? ''] ?? '?'}</span>
          <span className="result-display__name">You: {playerChoice?.name ?? '?'}</span>
        </div>
        <span className="result-display__vs">vs</span>
        <div className="result-display__side">
          <span className="result-display__icon">{ICONS[computerChoice?.name ?? ''] ?? '?'}</span>
          <span className="result-display__name">Computer: {computerChoice?.name ?? '?'}</span>
        </div>
      </div>
    </div>
  )
}
