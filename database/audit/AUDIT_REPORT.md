# PostgreSQL Function Audit Report

**Date:** 2026-06-14  
**Scope:** TICTACTOEAPI DAL → PostgreSQL stored functions  
**Error:** `42725: function add_move(uuid, integer, text, smallint) is not unique`

---

## 1. Dapper Call Inventory

| Function Name | Repository Method | SQL | C# Parameter Types | Expected PG Signature |
|---|---|---|---|---|
| `create_game` | `GameRepository.CreateGameAsync` | `SELECT create_game(@Id, @Mode)` | `Guid`, `string` | `create_game(uuid, text) → uuid` |
| `add_move` | `GameRepository.AddMoveAsync` | `SELECT add_move(@GameId, @MoveNumber, @Player, @CellIndex)` | `Guid`, `int`, `char` (sent as `text` via `CharTypeHandler`), `short` | `add_move(uuid, integer, text, smallint) → void` |
| `get_game` | `GameRepository.GetGameAsync` | `SELECT * FROM get_game(@Id)` | `Guid` | `get_game(uuid) → TABLE(...)` |
| `undo_move` | **NOT CALLED** | — | — | Undo implemented in BAL via `remove_last_moves` + `update_game` |
| `reset_game` | **NOT CALLED** | — | — | Reset implemented in BAL via `remove_last_moves` + `update_game` |
| `get_scoreboard` | `ScoreboardRepository.GetScoreboardAsync` | `SELECT * FROM get_scoreboard()` | (none) | `get_scoreboard() → TABLE(x_wins int, o_wins int, draws int)` |
| `reset_scoreboard` | `ScoreboardRepository.ResetScoreboardAsync` | `SELECT reset_scoreboard()` | (none) | `reset_scoreboard() → void` |

### Additional DAL functions (also audited — duplicates found)

| Function Name | Repository Method | C# Parameter Types | Expected PG Signature |
|---|---|---|---|
| `update_game` | `GameRepository.UpdateGameAsync` | `Guid`, `string`, `char`→text, `string`, `char?`→text, `short[]?` | `update_game(uuid, text, text, text, text, smallint[]) → void` |
| `get_moves` | `GameRepository.GetMovesAsync` | `Guid` | `get_moves(uuid) → TABLE(...)` |
| `get_next_move_number` | `GameRepository.GetNextMoveNumberAsync` | `Guid` | `get_next_move_number(uuid) → integer` |
| `remove_last_moves` | `GameRepository.RemoveLastMovesAsync` | `Guid`, `int` | `remove_last_moves(uuid, integer) → void` |
| `increment_x_wins` etc. | `ScoreboardRepository.*` | (none) | single overload each ✓ |

---

## 2. Root Cause — Duplicate Overloads

| Function | Duplicate Signatures in DB | Status |
|---|---|---|
| `create_game` | `(uuid, varchar)` + `(uuid, text)` | **AMBIGUOUS** when Npgsql sends `text` |
| `add_move` | `(uuid, int, varchar, smallint)` + `(uuid, int, char, smallint)` | **AMBIGUOUS** — CharTypeHandler sends `DbType.String` → `text` |
| `update_game` | `(uuid, char, char, varchar, char, smallint[])` + `(uuid, text, varchar, text, varchar, smallint[])` | **AMBIGUOUS** — same `text` binding issue |
| `get_game` | single overload | OK |
| `get_scoreboard` | single overload | OK |
| `reset_scoreboard` | single overload | OK |
| `undo_move` | does not exist | N/A — BAL handles undo |
| `reset_game` | does not exist | N/A — BAL handles reset |

---

## 3. Which Overload Does Dapper/Npgsql Actually Use?

Npgsql + `CharTypeHandler` bind all string-like C# values as **`text`**:

```csharp
// CharTypeHandler.SetValue
parameter.Value = value.ToString();
parameter.DbType = DbType.String;  // maps to PostgreSQL text
```

| Call | Runtime PG argument types | Resolves to |
|---|---|---|
| `create_game(@Id, @Mode)` | `uuid, text` | **Cannot pick** — matches both `varchar` and `text` overloads |
| `add_move(..., @Player, ...)` | `uuid, integer, text, smallint` | **Cannot pick** — `text` is castable to both `varchar` and `char` |
| `update_game(...)` | `uuid, text, text, text, text, smallint[]` | **Cannot pick** — mixed overload parameters both accept text coercion |

**Conclusion:** No single overload is reliably selected. PostgreSQL raises `42725` (function is not unique).

---

## 4. Standardization Decision

**Rule:** Exactly one function per operation. All string parameters → `TEXT`.

| Function | Canonical Signature |
|---|---|
| `create_game` | `(uuid, text) → uuid` |
| `add_move` | `(uuid, integer, text, smallint) → void` |
| `update_game` | `(uuid, text, text, text, text, smallint[]) → void` |
| `get_game` | `(uuid) → TABLE(...)` — return `text` for string columns |
| `get_moves` | `(uuid) → TABLE(...)` — return `text` for player |
| All others | unchanged (already unique) |

---

## 5. Migration

| Script | Purpose |
|---|---|
| `database/migrations/001_standardize_functions.sql` | Drop duplicates, create canonical TEXT functions |
| `database/migrations/002_fix_return_type_casts.sql` | Cast `varchar` columns to `text` in `get_game`/`get_moves` RETURNS TABLE |

**Apply (requires `postgres` superuser — functions owned by postgres):**
```powershell
$env:TICTACTOE_DB="Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=<pwd>;"
cd e:\TICTACTOE\database\audit
dotnet run -- apply
dotnet run -- apply-fix
dotnet run -- list      # confirm one overload per function
dotnet run -- verify    # hit all 7 API endpoints
```

**Post-migration function inventory (verified):**
```
create_game(uuid, text)
add_move(uuid, integer, text, smallint)
update_game(uuid, text, text, text, text, smallint[])
get_game(uuid)
get_moves(uuid)
get_scoreboard()
reset_scoreboard()
remove_last_moves(uuid, integer)   # used by undo/reset in BAL
```

**Endpoint verification:** All 7 endpoints passed after migration (including VsComputer dual `add_move`).
