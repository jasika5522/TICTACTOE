import { GameMode } from './game-state.model';

export interface GameSession {
  mode: GameMode;
  playerXName: string;
  playerOName: string;
}
