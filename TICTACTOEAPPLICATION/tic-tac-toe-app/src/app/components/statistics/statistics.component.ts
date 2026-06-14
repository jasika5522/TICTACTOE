import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StatsService } from '../../core/services/stats.service';

@Component({
  selector: 'app-statistics',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="stats-card glass-card">
      <h3 class="stats-title">Statistics</h3>
      <div class="stats-grid">
        <div class="stat-item">
          <span class="stat-value">{{ statsService.stats().totalGames }}</span>
          <span class="stat-label">Total Games</span>
        </div>
        <div class="stat-item x">
          <span class="stat-value">{{ statsService.stats().xWins }}</span>
          <span class="stat-label">X Wins</span>
        </div>
        <div class="stat-item o">
          <span class="stat-value">{{ statsService.stats().oWins }}</span>
          <span class="stat-label">O Wins</span>
        </div>
        <div class="stat-item">
          <span class="stat-value">{{ statsService.stats().draws }}</span>
          <span class="stat-label">Draws</span>
        </div>
        <div class="stat-item highlight">
          <span class="stat-value">{{ statsService.winPercentage }}%</span>
          <span class="stat-label">Win Rate</span>
        </div>
      </div>
      <p class="last-winner" *ngIf="statsService.stats().lastWinner">
        Last winner: <strong>{{ statsService.stats().lastWinner }}</strong>
      </p>
    </div>
  `,
  styleUrl: './statistics.component.scss'
})
export class StatisticsComponent {
  constructor(public statsService: StatsService) {}
}
