import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-winner-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="modal-backdrop" *ngIf="visible" @fadeIn (click)="onBackdropClick($event)">
      <div class="modal glass-card" @slideUp>
        <div class="celebration" *ngIf="isWin">🎉</div>
        <div class="celebration" *ngIf="isDraw">🤝</div>
        <h2 class="title">{{ title }}</h2>
        <p class="subtitle" *ngIf="subtitle">{{ subtitle }}</p>
        <div class="actions">
          <button class="btn btn-primary" type="button" (click)="playAgain.emit()">Play Again</button>
          <button class="btn btn-ghost" type="button" (click)="goHome.emit()">Return Home</button>
        </div>
      </div>
    </div>
  `,
  styleUrl: './winner-modal.component.scss',
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('250ms ease-out', style({ opacity: 1 }))
      ])
    ]),
    trigger('slideUp', [
      transition(':enter', [
        style({ transform: 'translateY(30px) scale(0.95)', opacity: 0 }),
        animate('350ms cubic-bezier(0.34, 1.56, 0.64, 1)', style({ transform: 'translateY(0) scale(1)', opacity: 1 }))
      ])
    ])
  ]
})
export class WinnerModalComponent {
  @Input() visible = false;
  @Input() title = '';
  @Input() subtitle = '';
  @Input() isWin = false;
  @Input() isDraw = false;
  @Output() playAgain = new EventEmitter<void>();
  @Output() goHome = new EventEmitter<void>();

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal-backdrop')) {
      // optional close - don't auto close
    }
  }
}
