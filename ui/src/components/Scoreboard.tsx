import type { ScoreEntry } from '../types/types'

interface Props {
  entries: ScoreEntry[]
  onReset: () => void
}

const RESULT_LABELS: Record<string, string> = {
  win: 'Win',
  lose: 'Lose',
  tie: 'Tie',
}

const ICONS: Record<string, string> = {
  rock: '✊',
  paper: '✋',
  scissors: '✌️',
  lizard: '🦎',
  spock: '🖖',
}

export function Scoreboard({ entries, onReset }: Props) {
  return (
    <section className="scoreboard">
      <div className="scoreboard__header">
        <h2>Recent Games</h2>
        {entries.length > 0 && (
          <button className="btn btn--ghost" onClick={onReset}>
            Reset
          </button>
        )}
      </div>
      {entries.length === 0 ? (
        <p className="scoreboard__empty">No games played yet. Make your first move!</p>
      ) : (
        <table className="scoreboard__table">
          <thead>
            <tr>
              <th>Result</th>
              <th>You</th>
              <th>Computer</th>
            </tr>
          </thead>
          <tbody>
            {entries.map(entry => (
              <tr key={entry.id} className={`score-row score-row--${entry.result}`}>
                <td className="score-row__result">{RESULT_LABELS[entry.result]}</td>
                <td>
                  {ICONS[entry.playerName]} {entry.playerName}
                </td>
                <td>
                  {ICONS[entry.computerName]} {entry.computerName}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  )
}
