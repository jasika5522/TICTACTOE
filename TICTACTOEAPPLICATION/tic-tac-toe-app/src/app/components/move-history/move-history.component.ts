import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MoveDto } from '../../models/game-state.model';

@Component({
  selector: 'app-move-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './move-history.component.html',
  styleUrl: './move-history.component.scss'
})
export class MoveHistoryComponent {
  @Input() moves: MoveDto[] = [];

  positionLabel(move: MoveDto): string {
    return `Row ${move.row + 1}, Column ${move.column + 1}`;
  }
}
