import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScoreboardDto } from '../../models/game-state.model';

@Component({
  selector: 'app-scoreboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="scoreboard glass-card" *ngIf="scoreboard">
      <div class="player-score x-score">
        <span class="label">{{ playerXName }}</span>
        <span class="symbol">X</span>
        <span class="value">{{ scoreboard.xWins }}</span>
      </div>
      <div class="divider">VS</div>
      <div class="player-score o-score">
        <span class="label">{{ playerOName }}</span>
        <span class="symbol">O</span>
        <span class="value">{{ scoreboard.oWins }}</span>
      </div>
      <div class="draws">
        <span class="draw-label">Draws</span>
        <span class="draw-value">{{ scoreboard.draws }}</span>
      </div>
    </div>
  `,
  styleUrl: './scoreboard.component.scss'
})
export class ScoreboardComponent {
  @Input() scoreboard: ScoreboardDto | null = null;
  @Input() playerXName = 'Player X';
  @Input() playerOName = 'Player O';
  @Output() reset = new EventEmitter<void>();
}
