import { Injectable, signal } from '@angular/core';
import { GameSession } from '../../models/game-session.model';
import { GameMode } from '../../models/game-state.model';

@Injectable({ providedIn: 'root' })
export class GameSessionService {
  private readonly storageKey = 'ttt-session';
  readonly session = signal<GameSession | null>(this.load());

  setSession(session: GameSession): void {
    this.session.set(session);
    localStorage.setItem(this.storageKey, JSON.stringify(session));
  }

  clearSession(): void {
    this.session.set(null);
    localStorage.removeItem(this.storageKey);
  }

  getDefaultSession(mode: GameMode = 'TwoPlayer'): GameSession {
    return {
      mode,
      playerXName: 'Player X',
      playerOName: mode === 'VsComputer' ? 'Computer' : 'Player O'
    };
  }

  private load(): GameSession | null {
    try {
      const raw = localStorage.getItem(this.storageKey);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }
}
