export interface Choice {
    id: number
    name: string
};

export interface PlayResult {
  results: 'win' | 'lose' | 'tie'
  player: number
  computer: number
};

export interface ScoreEntry {
  id: number
  result: 'win' | 'lose' | 'tie'
  player: number
  playerName: string
  computer: number
  computerName: string
  playedAt: string
};