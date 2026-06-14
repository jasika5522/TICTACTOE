import { Player } from './game-state.model';

export interface GameStats {
  totalGames: number;
  xWins: number;
  oWins: number;
  draws: number;
  lastWinner: string | null;
  lastWinnerSymbol: Player | null;
}

export const DEFAULT_STATS: GameStats = {
  totalGames: 0,
  xWins: 0,
  oWins: 0,
  draws: 0,
  lastWinner: null,
  lastWinnerSymbol: null
};
