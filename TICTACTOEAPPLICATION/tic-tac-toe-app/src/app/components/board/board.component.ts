import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameStateDto } from '../../models/game-state.model';
import { CellComponent } from '../cell/cell.component';

@Component({
  selector: 'app-board',
  standalone: true,
  imports: [CommonModule, CellComponent],
  template: `
    <div class="board-wrapper">
      <div class="board">
        <app-cell
          *ngFor="let i of cells"
          [value]="getCellChar(i)"
          [winning]="isWinningCell(i)"
          [disabled]="!isClickable(i)"
          (cellClick)="cellClick.emit(i)"
        />
      </div>
    </div>
  `,
  styleUrl: './board.component.scss'
})
export class BoardComponent {
  @Input() gameState: GameStateDto | null = null;
  @Output() cellClick = new EventEmitter<number>();

  readonly cells = [0, 1, 2, 3, 4, 5, 6, 7, 8];

  getCellChar(index: number): 'X' | 'O' | '' {
    if (!this.gameState) return '';
    const ch = this.gameState.board[index];
    return ch === '_' ? '' : ch as 'X' | 'O';
  }

  isWinningCell(index: number): boolean {
    return this.gameState?.status === 'Won' &&
      (this.gameState.winningCells?.includes(index) ?? false);
  }

  isClickable(index: number): boolean {
    if (!this.gameState || this.gameState.status !== 'InProgress') return false;
    return this.gameState.board[index] === '_';
  }
}
