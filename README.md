# Tic Tac Toe — Full-Stack Application

A full-stack Tic Tac Toe game built with **Angular**, **.NET 9 Web API**, **Ocelot API Gateway**, and **PostgreSQL**. Players can compete in two-player or vs-computer mode, undo moves, track scoreboard statistics, and view move history — all through a modern responsive UI.

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Angular 17 (standalone components, TypeScript, SCSS) |
| API Gateway | Ocelot 24.x on .NET 10 |
| Backend | .NET 10 Web API, Dapper ORM |
| Database | PostgreSQL (stored functions for all DB operations) |
| Architecture | DAL → BAL → Controller (backend), reverse proxy (gateway) |

---

## Features Implemented

- **3×3 game board** with turn-based X/O play
- **Win detection** (rows, columns, diagonals) with winning-cell highlight
- **Draw detection** when the board is full
- **Two game modes:** `TwoPlayer` and `VsComputer` (human = X, computer = O)
- **Computer AI** priority: win → block → center → corner → any available cell
- **Undo move** — TwoPlayer removes 1 move; VsComputer removes 2 (human + computer)
- **Undo after game completion (Option B)** — decrements scoreboard when undoing a finished game
- **Scoreboard** — persistent X wins, O wins, draws (PostgreSQL)
- **Reset game** — clears board/moves for current game, keeps scoreboard
- **Reset scoreboard** — zeroes all scores
- **Move history** — move number, player, cell position per move
- **Dark / light theme** with localStorage persistence
- **Local statistics** — total games, win rate, last winner (localStorage + API sync)
- **Home landing page** — player names, mode selection, play button

---

## Architecture

```
┌─────────────────┐     HTTPS      ┌──────────────────┐     HTTPS      ┌─────────────────┐     SQL       ┌────────────┐
│  Angular App    │ ──────────────► │  Ocelot Gateway  │ ─────────────► │  TICTACTOE API  │ ────────────► │ PostgreSQL │
│  localhost:4200 │   /api/*        │  localhost:7000  │   /api/*       │  localhost:7241 │  functions   │            │
└─────────────────┘                 └──────────────────┘                └─────────────────┘              └────────────┘
```

**Request flow:** The Angular app calls only the gateway (`https://localhost:7000/api`). Ocelot forwards matching routes to the backend API (`https://localhost:7241`). The API uses Dapper to call PostgreSQL stored functions. CORS is enabled on both gateway and API for `http://localhost:4200`.

**Backend layers:**
- **Controllers** — HTTP endpoints, request/response DTOs
- **BAL** — game rules, undo logic, computer moves, scoreboard updates
- **DAL** — Dapper repositories calling PostgreSQL functions
- **DB** — `games`, `moves`, `scoreboard` tables + stored functions

---

## Repository Structure

```
TICTACTOE/
├── TICTACTOEAPI/              # .NET Web API
│   └── TICTACTOEAPI/
│       ├── Controllers/
│       ├── BAL/
│       ├── DAL/
│       └── Models/
├── TICTACTOEGATEWAY/          # Ocelot reverse proxy
│   └── TICTACTOEGATEWAY/
│       ├── ocelot.json
│       └── Program.cs
├── TICTACTOEAPPLICATION/      # Angular frontend
│   └── tic-tac-toe-app/
│       └── src/app/
│           ├── pages/         # Home, Game
│           ├── components/    # Board, Cell, Scoreboard, etc.
│           ├── services/      # GameService (API client)
│           └── core/services/ # Theme, Stats, Session
├── database/
│   ├── migrations/            # PostgreSQL function scripts
│   └── audit/                 # DB audit/migration helper tool
├── README.md
└── API_DOCUMENTATION.md
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/) and npm
- [PostgreSQL 14+](https://www.postgresql.org/)
- [Angular CLI](https://angular.io/cli): `npm install -g @angular/cli`
- Trust dev HTTPS certs: `dotnet dev-certs https --trust`

---

## Database Setup

1. Create a PostgreSQL user and database.
2. Create tables (`games`, `moves`, `scoreboard`) and stored functions (see your schema setup).
3. Apply migrations (requires postgres superuser for function ownership):

```powershell
$env:TICTACTOE_DB="Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;"
cd database/audit
dotnet run -- apply
dotnet run -- apply-fix
```

4. Configure the API connection string (see below).

---

## Configuration (Secrets)

**Do not commit real passwords.** Copy the example file:

```powershell
cd TICTACTOEAPI/TICTACTOEAPI
copy appsettings.Development.json.example appsettings.Development.json
# Edit appsettings.Development.json with your real credentials
```

`appsettings.Development.json` is gitignored. The committed `appsettings.json` uses placeholders only.

**Angular API URL** — `TICTACTOEAPPLICATION/tic-tac-toe-app/src/environments/environment.development.ts`:

```typescript
apiBaseUrl: 'https://localhost:7000/api'  // Gateway, not direct API
```

---

## How to Run Locally

Start services in this order (4 terminals):

### 1. PostgreSQL
Ensure PostgreSQL is running with your schema and connection string configured.

### 2. Backend API (port 7241)

```powershell
cd TICTACTOEAPI/TICTACTOEAPI
dotnet restore
dotnet run
```

- HTTPS: `https://localhost:7241`
- HTTP: `http://localhost:5223`
- Swagger: `https://localhost:7241/swagger`

### 3. Ocelot Gateway (port 7000)

```powershell
cd TICTACTOEGATEWAY/TICTACTOEGATEWAY/TICTACTOEGATEWAY
dotnet restore
dotnet run
```

- Gateway: `https://localhost:7000`
- Routes defined in `ocelot.json` → downstream `https://localhost:7241`
- CORS enabled for `http://localhost:4200`

> **Build tip:** Stop running API/Gateway processes before rebuilding in Visual Studio (`Stop-Process -Name TICTACTOEAPI,TICTACTOEGATEWAY -Force`).

### 4. Angular Frontend (port 4200)

```powershell
cd TICTACTOEAPPLICATION/tic-tac-toe-app
npm install
ng serve
```

Open **http://localhost:4200**

---

## API Endpoint Summary

All client requests go through the **gateway** (`https://localhost:7000`). Ocelot forwards to the same path on the **backend** (`https://localhost:7241`).

| Method | Gateway & Backend Route | Description |
|--------|-------------------------|-------------|
| `POST` | `/api/games` | Create new game |
| `GET`  | `/api/games/{id}` | Get game state |
| `POST` | `/api/games/{id}/moves` | Make a move |
| `POST` | `/api/games/{id}/undo` | Undo last move(s) |
| `POST` | `/api/games/{id}/reset` | Reset board (keep scoreboard) |
| `GET`  | `/api/scoreboard` | Get global scoreboard |
| `POST` | `/api/scoreboard/reset` | Reset scoreboard to zero |

See [API_DOCUMENTATION.md](./API_DOCUMENTATION.md) for full request/response schemas.

---

## Frontend API Integration

| Service / Component | API Calls |
|---------------------|-----------|
| `GameService` | All 7 endpoints above |
| `GameComponent` | Orchestrates game via `GameService` |
| `StatsService` | Syncs scoreboard from API; stores last winner in localStorage |
| `GameSessionService` | Player names/mode in localStorage (UI only, not sent to API) |
| `ThemeService` | Theme preference in localStorage |

---

## Running Tests

### Frontend (Angular + Karma/Jasmine)

```powershell
cd TICTACTOEAPPLICATION/tic-tac-toe-app
ng test
```

Includes `app.component.spec.ts` (smoke test). Run once with `--watch=false --browsers=ChromeHeadless` for CI-style execution.

### Backend

No dedicated xUnit/NUnit test project is included. Manual verification options:

```powershell
# Via gateway (API + Gateway must be running)
cd database/audit
dotnet run -- verify

# Swagger UI
# https://localhost:7241/swagger
```

### Database audit tool

```powershell
cd database/audit
dotnet run -- list        # List PostgreSQL functions
dotnet run -- draw-test   # Draw scenario via gateway
```

---

## Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Undo after completion | **Option B** | Undoing a finished game decrements scoreboard counts |
| VsComputer undo | Remove **2 moves** | One API call adds human + computer move together |
| Storage | **PostgreSQL** (not in-memory/SQLite) | Persistent games, moves, scoreboard via stored procs |
| DB access | **Dapper + stored functions** | No ORM; all CRUD via PostgreSQL functions |
| API entry point | **Ocelot gateway** | Single URL for frontend; CORS/ssl termination at gateway |
| Player names | **Frontend only** | Backend tracks X/O symbols, not display names |
| Computer AI | Win > Block > Center > Corner > Any | Implemented in `GameLogicService.GetComputerMove` |
| Cell indexing | 0–8 left-to-right, top-to-bottom | `row = index / 3`, `col = index % 3` (0-based internally; UI shows 1-based) |

---

## Clarifications & Assumptions

- Human is always **X** in VsComputer mode; computer is **O**
- Board string uses `_` for empty, `X`/`O` for filled cells (9 characters)
- `CreateGame` returns HTTP **201 Created** with `GameStateDto` body
- Scoreboard is **global** (single row, id=1), not per-session
- Gateway uses `BypassSslValidationHandler` for local dev HTTPS to downstream API
- .NET target framework is **net10.0** (SDK 10)

---

## Known Limitations

- No user authentication or multi-user sessions
- No automated backend unit/integration test suite
- Gateway and API use self-signed HTTPS certs (dev only)
- Scoreboard is global — all games share one score record
- Player names are not persisted server-side
- Ocelot routes are configured for localhost development only

---

## Future Improvements

- Docker Compose for API + Gateway + PostgreSQL + Angular
- Backend xUnit tests for `GameLogicService` and `GameService`
- Environment-based Ocelot downstream hosts (dev/staging/prod)
- Per-user scoreboards with authentication
- WebSocket real-time updates for multiplayer across browsers
- E2E tests (Playwright/Cypress) through full stack
- CI/CD pipeline (GitHub Actions)

---

## AI Tools & Prompt Summary

<!-- Fill in manually before submission -->

| Tool | Purpose | Notes |
|------|---------|-------|
| _e.g. Cursor / GitHub Copilot_ | _Code generation, refactoring_ | _Describe usage_ |
| _e.g. ChatGPT_ | _Architecture planning_ | _Describe prompts used_ |

**Prompt summary:** _[Add your own summary of AI-assisted development here]_

---

## License

_Add your license here (e.g. MIT) before submission._
