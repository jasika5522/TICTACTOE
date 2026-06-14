import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  CreateGameRequest,
  GameMode,
  GameStateDto,
  MakeMoveRequest,
  Player,
  ScoreboardDto
} from '../models/game-state.model';

@Injectable({ providedIn: 'root' })
export class GameService {
  private readonly baseUrl = environment.apiBaseUrl;
  private readonly gameStateSubject = new BehaviorSubject<GameStateDto | null>(null);

  readonly gameState$ = this.gameStateSubject.asObservable();

  constructor(private http: HttpClient) {}

  get currentGame(): GameStateDto | null {
    return this.gameStateSubject.value;
  }

  createGame(mode: GameMode): Observable<GameStateDto> {
    const body: CreateGameRequest = { mode };
    return this.http.post<GameStateDto>(`${this.baseUrl}/games`, body).pipe(
      tap(state => this.gameStateSubject.next(state))
    );
  }

  getGame(id: string): Observable<GameStateDto> {
    return this.http.get<GameStateDto>(`${this.baseUrl}/games/${id}`).pipe(
      tap(state => this.gameStateSubject.next(state))
    );
  }

  makeMove(id: string, player: Player, cellIndex: number): Observable<GameStateDto> {
    const body: MakeMoveRequest = { player, cellIndex };
    return this.http.post<GameStateDto>(`${this.baseUrl}/games/${id}/moves`, body).pipe(
      tap(state => this.gameStateSubject.next(state))
    );
  }

  undo(id: string): Observable<GameStateDto> {
    return this.http.post<GameStateDto>(`${this.baseUrl}/games/${id}/undo`, {}).pipe(
      tap(state => this.gameStateSubject.next(state))
    );
  }

  resetGame(id: string): Observable<GameStateDto> {
    return this.http.post<GameStateDto>(`${this.baseUrl}/games/${id}/reset`, {}).pipe(
      tap(state => this.gameStateSubject.next(state))
    );
  }

  getScoreboard(): Observable<ScoreboardDto> {
    return this.http.get<ScoreboardDto>(`${this.baseUrl}/scoreboard`);
  }

  resetScoreboard(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/scoreboard/reset`, {});
  }
}
