import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';
import { GameMode } from '../../models/game-state.model';
import { GameSessionService } from '../../core/services/game-session.service';
import { ThemeToggleComponent } from '../../components/theme-toggle/theme-toggle.component';
import { StatisticsComponent } from '../../components/statistics/statistics.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, ThemeToggleComponent, StatisticsComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
  animations: [
    trigger('pageEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('500ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ]
})
export class HomeComponent {
  mode: GameMode = 'TwoPlayer';
  playerXName = 'Player X';
  playerOName = 'Player O';

  constructor(
    private router: Router,
    private sessionService: GameSessionService
  ) {}

  selectMode(mode: GameMode): void {
    this.mode = mode;
    if (mode === 'VsComputer') {
      this.playerOName = 'Computer';
    } else if (this.playerOName === 'Computer') {
      this.playerOName = 'Player O';
    }
  }

  play(): void {
    this.sessionService.setSession({
      mode: this.mode,
      playerXName: this.playerXName.trim() || 'Player X',
      playerOName: this.playerOName.trim() || (this.mode === 'VsComputer' ? 'Computer' : 'Player O')
    });
    this.router.navigate(['/game']);
  }
}
