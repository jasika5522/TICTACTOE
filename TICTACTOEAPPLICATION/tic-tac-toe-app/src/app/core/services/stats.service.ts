import { Injectable, signal } from '@angular/core';
import { GameStats, DEFAULT_STATS } from '../../models/stats.model';
import { GameStateDto } from '../../models/game-state.model';
import { GameSession } from '../../models/game-session.model';

@Injectable({ providedIn: 'root' })
export class StatsService {
  private readonly storageKey = 'ttt-stats';
  readonly stats = signal<GameStats>(this.load());

  recordGameEnd(state: GameStateDto, session: GameSession | null): void {
    const current = { ...this.stats() };

    if (state.status === 'Won' && state.winner) {
      current.lastWinnerSymbol = state.winner;
      current.lastWinner = state.winner === 'X'
        ? session?.playerXName ?? 'Player X'
        : session?.playerOName ?? 'Player O';
    } else if (state.status === 'Draw') {
      current.lastWinner = null;
      current.lastWinnerSymbol = null;
    }

    this.save(current);
  }

  syncFromBackend(scoreboard: { xWins: number; oWins: number; draws: number }): void {
    const current = { ...this.stats() };
    current.xWins = scoreboard.xWins;
    current.oWins = scoreboard.oWins;
    current.draws = scoreboard.draws;
    current.totalGames = scoreboard.xWins + scoreboard.oWins + scoreboard.draws;
    this.save(current);
  }

  resetStats(): void {
    this.save({ ...DEFAULT_STATS });
  }

  get winPercentage(): number {
    const s = this.stats();
    if (s.totalGames === 0) return 0;
    const wins = s.xWins + s.oWins;
    return Math.round((wins / s.totalGames) * 100);
  }

  private load(): GameStats {
    try {
      const raw = localStorage.getItem(this.storageKey);
      return raw ? { ...DEFAULT_STATS, ...JSON.parse(raw) } : { ...DEFAULT_STATS };
    } catch {
      return { ...DEFAULT_STATS };
    }
  }

  private save(stats: GameStats): void {
    this.stats.set(stats);
    localStorage.setItem(this.storageKey, JSON.stringify(stats));
  }
}
