import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';
import { Player } from '../../models/game-state.model';

@Component({
  selector: 'app-cell',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button
      class="cell"
      [class.x]="value === 'X'"
      [class.o]="value === 'O'"
      [class.winning]="winning"
      [class.filled]="!!value"
      [disabled]="disabled"
      (click)="cellClick.emit()"
      [@cellPop]="value || 'empty'"
    >
      <span class="mark" *ngIf="value">{{ value }}</span>
    </button>
  `,
  styleUrl: './cell.component.scss',
  animations: [
    trigger('cellPop', [
      transition('* => X, * => O', [
        style({ transform: 'scale(0.3)', opacity: 0 }),
        animate('200ms cubic-bezier(0.34, 1.56, 0.64, 1)', style({ transform: 'scale(1)', opacity: 1 }))
      ])
    ])
  ]
})
export class CellComponent {
  @Input() value: Player | '' = '';
  @Input() disabled = false;
  @Input() winning = false;
  @Output() cellClick = new EventEmitter<void>();
}
