# API Documentation — Tic Tac Toe

Base URLs:

| Context | URL |
|---------|-----|
| **Gateway (client-facing)** | `https://localhost:7000/api` |
| **Backend (direct / downstream)** | `https://localhost:7241/api` |

Ocelot forwards gateway routes 1:1 to the backend. Request/response bodies are identical at both layers.

**Content-Type:** `application/json`  
**CORS:** Allowed origin `http://localhost:4200` (gateway + API)

---

## Common Schemas

### Player
`"X"` | `"O"`

### GameMode
`"TwoPlayer"` | `"VsComputer"`

### GameStatus
`"InProgress"` | `"Won"` | `"Draw"`

### ScoreboardDto
```json
{
  "xWins": 0,
  "oWins": 0,
  "draws": 0
}
```

### MoveDto
```json
{
  "moveNumber": 1,
  "player": "X",
  "cellIndex": 0,
  "row": 0,
  "column": 0
}
```

| Field | Type | Description |
|-------|------|-------------|
| `moveNumber` | int | Sequential move number (1-based) |
| `player` | string | `"X"` or `"O"` |
| `cellIndex` | int | Board index 0–8 (left→right, top→bottom) |
| `row` | int | 0-based row (`cellIndex / 3`) |
| `column` | int | 0-based column (`cellIndex % 3`) |

### GameStateDto
```json
{
  "gameId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "board": "_________",
  "currentPlayer": "X",
  "mode": "TwoPlayer",
  "status": "InProgress",
  "winner": null,
  "winningCells": null,
  "moveHistory": [],
  "scoreboard": {
    "xWins": 0,
    "oWins": 0,
    "draws": 0
  }
}
```

| Field | Type | Description |
|-------|------|-------------|
| `gameId` | guid | Unique game identifier |
| `board` | string | 9-char string; `_` = empty, `X`/`O` = filled |
| `currentPlayer` | string | `"X"` or `"O"` — whose turn it is |
| `mode` | string | `"TwoPlayer"` or `"VsComputer"` |
| `status` | string | `"InProgress"`, `"Won"`, or `"Draw"` |
| `winner` | string\|null | `"X"`, `"O"`, or `null` |
| `winningCells` | int[]\|null | Three cell indices when `status=Won` |
| `moveHistory` | MoveDto[] | All moves in order |
| `scoreboard` | ScoreboardDto | Inline global scoreboard |

### CreateGameRequest
```json
{
  "mode": "TwoPlayer"
}
```

| Field | Type | Required | Values |
|-------|------|----------|--------|
| `mode` | string | Yes | `"TwoPlayer"` \| `"VsComputer"` |

### MakeMoveRequest
```json
{
  "player": "X",
  "cellIndex": 4
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `player` | string | Yes | `"X"` or `"O"` (single char) |
| `cellIndex` | int | Yes | 0–8, must be empty cell |

### Error Response (400)
```json
{
  "error": "Not this player's turn"
}
```

---

## Endpoints

### 1. Create Game

| | |
|---|---|
| **Method** | `POST` |
| **Gateway route** | `/api/games` |
| **Backend route** | `/api/games` |
| **Request body** | `CreateGameRequest` |
| **Success** | `201 Created` + `GameStateDto` |
| **Errors** | `400` validation error |

**Example request:**
```http
POST https://localhost:7000/api/games
Content-Type: application/json

{ "mode": "VsComputer" }
```

**Example response:**
```json
{
  "gameId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "board": "_________",
  "currentPlayer": "X",
  "mode": "VsComputer",
  "status": "InProgress",
  "winner": null,
  "winningCells": null,
  "moveHistory": [],
  "scoreboard": { "xWins": 0, "oWins": 0, "draws": 0 }
}
```

---

### 2. Get Game

| | |
|---|---|
| **Method** | `GET` |
| **Gateway route** | `/api/games/{id}` |
| **Backend route** | `/api/games/{id}` |
| **Path param** | `id` — GUID |
| **Success** | `200 OK` + `GameStateDto` |
| **Errors** | `404 Not Found` |

---

### 3. Make Move

| | |
|---|---|
| **Method** | `POST` |
| **Gateway route** | `/api/games/{id}/moves` |
| **Backend route** | `/api/games/{id}/moves` |
| **Path param** | `id` — GUID |
| **Request body** | `MakeMoveRequest` |
| **Success** | `200 OK` + `GameStateDto` |
| **Errors** | `400 Bad Request` + `{ "error": "..." }` |

**Possible 400 error messages:**
| Message | Cause |
|---------|-------|
| `"Game not found"` | Invalid game ID |
| `"Game already completed"` | Status is Won or Draw |
| `"Not this player's turn"` | `player` ≠ `currentPlayer` |
| `"Invalid move"` | Cell occupied or index out of range |

**VsComputer behavior:** After a valid human (X) move, the backend automatically plays computer (O) in the same response when the game remains `InProgress`. `moveHistory` may contain two entries after one call.

---

### 4. Undo Move

| | |
|---|---|
| **Method** | `POST` |
| **Gateway route** | `/api/games/{id}/undo` |
| **Backend route** | `/api/games/{id}/undo` |
| **Path param** | `id` — GUID |
| **Request body** | none (empty body) |
| **Success** | `200 OK` + `GameStateDto` |
| **Errors** | `400 Bad Request` + `{ "error": "..." }` |

**Undo rules:**
- **TwoPlayer:** removes the last 1 move
- **VsComputer:** removes the last 2 moves (computer + human)
- **After completion (Option B):** if undoing from a Won/Draw back to InProgress, scoreboard counts are decremented

**Possible 400 errors:**
| Message | Cause |
|---------|-------|
| `"Game not found"` | Invalid game ID |
| `"No moves to undo"` | `moveHistory` is empty |

---

### 5. Reset Game

| | |
|---|---|
| **Method** | `POST` |
| **Gateway route** | `/api/games/{id}/reset` |
| **Backend route** | `/api/games/{id}/reset` |
| **Path param** | `id` — GUID |
| **Request body** | none |
| **Success** | `200 OK` + `GameStateDto` (fresh board, same gameId) |
| **Errors** | `404 Not Found` |

Clears all moves and resets board to `_________`. Does **not** reset the global scoreboard.

---

### 6. Get Scoreboard

| | |
|---|---|
| **Method** | `GET` |
| **Gateway route** | `/api/scoreboard` |
| **Backend route** | `/api/scoreboard` |
| **Success** | `200 OK` + `ScoreboardDto` |

---

### 7. Reset Scoreboard

| | |
|---|---|
| **Method** | `POST` |
| **Gateway route** | `/api/scoreboard/reset` |
| **Backend route** | `/api/scoreboard/reset` |
| **Request body** | none |
| **Success** | `204 No Content` |
| **Errors** | none documented |

---

## Ocelot Route Mapping

From `TICTACTOEGATEWAY/TICTACTOEGATEWAY/TICTACTOEGATEWAY/ocelot.json`:

| Upstream (Gateway) | Downstream (API) | Methods |
|--------------------|------------------|---------|
| `/api/games` | `https://localhost:7241/api/games` | POST, OPTIONS |
| `/api/games/{id}` | `https://localhost:7241/api/games/{id}` | GET, OPTIONS |
| `/api/games/{id}/moves` | `https://localhost:7241/api/games/{id}/moves` | POST, OPTIONS |
| `/api/games/{id}/undo` | `https://localhost:7241/api/games/{id}/undo` | POST, OPTIONS |
| `/api/games/{id}/reset` | `https://localhost:7241/api/games/{id}/reset` | POST, OPTIONS |
| `/api/scoreboard` | `https://localhost:7241/api/scoreboard` | GET, OPTIONS |
| `/api/scoreboard/reset` | `https://localhost:7241/api/scoreboard/reset` | POST, OPTIONS |

**GlobalConfiguration.BaseUrl:** `https://localhost:7000`

---

## Board Index Reference

```
 0 | 1 | 2
---+---+---
 3 | 4 | 5
---+---+---
 6 | 7 | 8
```

Display positions in the UI use **1-based** row/column (`row + 1`, `column + 1`).

---

## PostgreSQL Functions (Backend DAL)

The API does not expose these directly; they are called via Dapper:

| Function | Used by |
|----------|---------|
| `create_game(uuid, text)` | Create game |
| `get_game(uuid)` | Get game |
| `add_move(uuid, int, text, smallint)` | Record move |
| `update_game(...)` | Update board/status |
| `get_moves(uuid)` | Move history |
| `get_next_move_number(uuid)` | Next move # |
| `remove_last_moves(uuid, int)` | Undo / reset |
| `get_scoreboard()` | Scoreboard |
| `reset_scoreboard()` | Reset scores |
| `increment_x_wins()` / `decrement_x_wins()` etc. | Score updates |

See `database/migrations/` for canonical function definitions.
