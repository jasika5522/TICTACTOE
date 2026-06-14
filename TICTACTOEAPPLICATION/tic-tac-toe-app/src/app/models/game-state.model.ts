export type Player = 'X' | 'O';
export type GameMode = 'TwoPlayer' | 'VsComputer';
export type GameStatus = 'InProgress' | 'Won' | 'Draw';

export interface MoveDto {
  moveNumber: number;
  player: Player;
  cellIndex: number;
  row: number;
  column: number;
}

export interface ScoreboardDto {
  xWins: number;
  oWins: number;
  draws: number;
}

export interface GameStateDto {
  gameId: string;
  board: string;
  currentPlayer: Player;
  mode: GameMode;
  status: GameStatus;
  winner: Player | null;
  winningCells: number[] | null;
  moveHistory: MoveDto[];
  scoreboard: ScoreboardDto;
}

export interface CreateGameRequest {
  mode: GameMode;
}

export interface MakeMoveRequest {
  player: Player;
  cellIndex: number;
}
