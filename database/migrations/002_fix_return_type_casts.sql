-- 002_fix_return_type_casts.sql
-- Fixes 42804: varchar table columns must be cast to text in RETURNS TABLE functions.

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
