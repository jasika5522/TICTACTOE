import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-game-controls',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="controls">
      <button class="btn btn-secondary" type="button" (click)="newGame.emit()" [disabled]="loading">
        New Game
      </button>
      <button class="btn btn-secondary" type="button" (click)="resetGame.emit()" [disabled]="loading">
        Reset Board
      </button>
      <button class="btn btn-outline" type="button" (click)="resetScore.emit()" [disabled]="loading">
        Reset Score
      </button>
      <button class="btn btn-ghost" type="button" (click)="undo.emit()" [disabled]="!canUndo || loading">
        Undo
      </button>
      <button class="btn btn-ghost" type="button" (click)="goHome.emit()">
        ← Home
      </button>
    </div>
  `,
  styleUrl: './game-controls.component.scss'
})
export class GameControlsComponent {
  @Input() canUndo = false;
  @Input() loading = false;
  @Output() newGame = new EventEmitter<void>();
  @Output() resetGame = new EventEmitter<void>();
  @Output() resetScore = new EventEmitter<void>();
  @Output() undo = new EventEmitter<void>();
  @Output() goHome = new EventEmitter<void>();
}
