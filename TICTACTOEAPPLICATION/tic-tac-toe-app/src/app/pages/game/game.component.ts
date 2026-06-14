import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { trigger, transition, style, animate } from '@angular/animations';
import { GameService } from '../../services/game.service';
import { GameSessionService } from '../../core/services/game-session.service';
import { StatsService } from '../../core/services/stats.service';
import { GameStateDto } from '../../models/game-state.model';
import { BoardComponent } from '../../components/board/board.component';
import { ScoreboardComponent } from '../../components/scoreboard/scoreboard.component';
import { GameControlsComponent } from '../../components/game-controls/game-controls.component';
import { WinnerModalComponent } from '../../components/winner-modal/winner-modal.component';
import { StatisticsComponent } from '../../components/statistics/statistics.component';
import { ThemeToggleComponent } from '../../components/theme-toggle/theme-toggle.component';
import { MoveHistoryComponent } from '../../components/move-history/move-history.component';

@Component({
  selector: 'app-game',
  standalone: true,
  imports: [
    CommonModule,
    BoardComponent,
    ScoreboardComponent,
    GameControlsComponent,
    WinnerModalComponent,
    StatisticsComponent,
    ThemeToggleComponent,
    MoveHistoryComponent
  ],
  templateUrl: './game.component.html',
  styleUrl: './game.component.scss',
  animations: [
    trigger('pageEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(16px)' }),
        animate('400ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ]
})
export class GameComponent implements OnInit, OnDestroy {
  gameState: GameStateDto | null = null;
  errorMessage = '';
  loading = false;
  showModal = false;
  private sub?: Subscription;
  private lastRecordedGameId: string | null = null;

  constructor(
    private gameService: GameService,
    public sessionService: GameSessionService,
    private statsService: StatsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const session = this.sessionService.session();
    if (!session) {
      this.router.navigate(['/']);
      return;
    }

    this.sub = this.gameService.gameState$.subscribe(state => {
      if (!state) return;
      this.handleStateUpdate(state);
      this.gameState = state;
    });

    this.startNewGame();
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  get session() {
    return this.sessionService.session();
  }

  get canUndo(): boolean {
    return (this.gameState?.moveHistory.length ?? 0) > 0;
  }

  get turnLabel(): string {
    if (!this.gameState || this.gameState.status !== 'InProgress') return '';
    const name = this.gameState.currentPlayer === 'X'
      ? this.session?.playerXName ?? 'X'
      : this.session?.playerOName ?? 'O';
    return `${name}'s turn (${this.gameState.currentPlayer})`;
  }

  get modalTitle(): string {
    if (!this.gameState) return '';
    if (this.gameState.status === 'Won') {
      const name = this.gameState.winner === 'X'
        ? this.session?.playerXName ?? 'Player X'
        : this.session?.playerOName ?? 'Player O';
      return `${name} Wins!`;
    }
    return "It's a Draw!";
  }

  onCellClick(cellIndex: number): void {
    if (!this.gameState || this.gameState.status !== 'InProgress' || this.loading) return;
    this.errorMessage = '';
    this.loading = true;
    this.gameService
      .makeMove(this.gameState.gameId, this.gameState.currentPlayer, cellIndex)
      .subscribe({
        next: () => { this.loading = false; },
        error: err => {
          this.loading = false;
          this.errorMessage = err.error?.error ?? 'Move failed';
        }
      });
  }

  onNewGame(): void {
    this.showModal = false;
    this.startNewGame();
  }

  onResetGame(): void {
    if (!this.gameState) return;
    this.showModal = false;
    this.errorMessage = '';
    this.loading = true;
    this.gameService.resetGame(this.gameState.gameId).subscribe({
      next: () => { this.loading = false; },
      error: err => {
        this.loading = false;
        this.errorMessage = err.error?.error ?? 'Reset failed';
      }
    });
  }

  onUndo(): void {
    if (!this.gameState || !this.canUndo) return;
    this.showModal = false;
    this.errorMessage = '';
    this.loading = true;
    this.gameService.undo(this.gameState.gameId).subscribe({
      next: () => { this.loading = false; },
      error: err => {
        this.loading = false;
        this.errorMessage = err.error?.error ?? 'Undo failed';
      }
    });
  }

  onResetScore(): void {
    this.errorMessage = '';
    this.gameService.resetScoreboard().subscribe({
      next: () => {
        this.statsService.resetStats();
        if (this.gameState) {
          this.gameService.getGame(this.gameState.gameId).subscribe();
        }
      },
      error: () => this.errorMessage = 'Failed to reset scoreboard'
    });
  }

  onGoHome(): void {
    this.showModal = false;
    this.router.navigate(['/']);
  }

  onPlayAgain(): void {
    this.onNewGame();
  }

  private startNewGame(): void {
    const mode = this.session?.mode ?? 'TwoPlayer';
    this.lastRecordedGameId = null;
    this.errorMessage = '';
    this.loading = true;
    this.gameService.createGame(mode).subscribe({
      next: () => { this.loading = false; },
      error: err => {
        this.loading = false;
        this.errorMessage = err.error?.error ?? 'Failed to create game';
      }
    });
  }

  private handleStateUpdate(state: GameStateDto): void {
    this.statsService.syncFromBackend(state.scoreboard);

    if (state.status === 'InProgress') {
      this.showModal = false;
      this.lastRecordedGameId = null;
      return;
    }

    if ((state.status === 'Won' || state.status === 'Draw') &&
        this.lastRecordedGameId !== state.gameId) {
      this.lastRecordedGameId = state.gameId;
      this.statsService.recordGameEnd(state, this.sessionService.session());
      this.showModal = true;
    }
  }
}
