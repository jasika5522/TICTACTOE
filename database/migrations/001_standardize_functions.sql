-- 001_standardize_functions.sql
-- Removes duplicate PostgreSQL function overloads causing 42725 ambiguity.
-- Canonical signatures use TEXT for all string parameters (Npgsql/Dapper compatible).
-- Does NOT modify .NET code.
--
-- REQUIRES: Run as postgres superuser (functions are owned by postgres).
--   $env:TICTACTOE_DB="Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=<pwd>;"
--   cd database/audit && dotnet run -- apply

BEGIN;

-- ============================================================
-- DROP duplicate overloads (exact signatures required)
-- ============================================================

DROP FUNCTION IF EXISTS public.create_game(uuid, character varying);
DROP FUNCTION IF EXISTS public.create_game(uuid, text);

DROP FUNCTION IF EXISTS public.add_move(uuid, integer, character varying, smallint);
DROP FUNCTION IF EXISTS public.add_move(uuid, integer, character, smallint);

DROP FUNCTION IF EXISTS public.update_game(uuid, character, character, character varying, character, smallint[]);
DROP FUNCTION IF EXISTS public.update_game(uuid, text, character varying, text, character varying, smallint[]);

-- Replace get_game / get_moves so return types also use TEXT consistently
DROP FUNCTION IF EXISTS public.get_game(uuid);
DROP FUNCTION IF EXISTS public.get_moves(uuid);

-- ============================================================
-- CREATE canonical functions (one per operation)
-- ============================================================

CREATE OR REPLACE FUNCTION public.create_game(p_id uuid, p_mode text)
RETURNS uuid
LANGUAGE plpgsql
AS $function$
BEGIN
    INSERT INTO games (id, mode, board, current_player, status)
    VALUES (p_id, p_mode, '_________', 'X', 'InProgress');
    RETURN p_id;
END;
$function$;

CREATE OR REPLACE FUNCTION public.add_move(
    p_game_id uuid,
    p_move_number integer,
    p_player text,
    p_cell_index smallint
)
RETURNS void
LANGUAGE plpgsql
AS $function$
BEGIN
    INSERT INTO moves (game_id, move_number, player, cell_index)
    VALUES (p_game_id, p_move_number, p_player, p_cell_index);
END;
$function$;

CREATE OR REPLACE FUNCTION public.update_game(
    p_id uuid,
    p_board text,
    p_current_player text,
    p_status text,
    p_winner text,
    p_winning_cells smallint[]
)
RETURNS void
LANGUAGE plpgsql
AS $function$
BEGIN
    UPDATE games
    SET board = p_board,
        current_player = p_current_player,
        status = p_status,
        winner = p_winner,
        winning_cells = p_winning_cells
    WHERE id = p_id;
END;
$function$;

CREATE OR REPLACE FUNCTION public.get_game(p_id uuid)
RETURNS TABLE(
    id uuid,
    mode text,
    board text,
    current_player text,
    status text,
    winner text,
    winning_cells smallint[]
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT g.id, g.mode, g.board,
           g.current_player::text, g.status, g.winner::text, g.winning_cells
    FROM games g
    WHERE g.id = p_id;
END;
$function$;

CREATE OR REPLACE FUNCTION public.get_moves(p_game_id uuid)
RETURNS TABLE(
    id integer,
    game_id uuid,
    move_number integer,
    player text,
    cell_index smallint
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT m.id, m.game_id, m.move_number, m.player::text, m.cell_index
    FROM moves m
    WHERE m.game_id = p_game_id
    ORDER BY m.move_number;
END;
$function$;

COMMIT;

-- ============================================================
-- Post-migration verification query (run manually)
-- ============================================================
-- SELECT proname, pg_get_function_identity_arguments(oid)
-- FROM pg_proc p JOIN pg_namespace n ON p.pronamespace = n.oid
-- WHERE n.nspname = 'public'
--   AND proname IN ('create_game','add_move','update_game','get_game','get_moves')
-- ORDER BY 1, 2;
