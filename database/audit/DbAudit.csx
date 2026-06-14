#r "nuget: Npgsql, 10.0.3"
using Npgsql;

var connStr = "Host=localhost;Port=5432;Database=postgres;Username=tictactoe_app;Password=TicTacToe#123;";
await using var conn = new NpgsqlConnection(connStr);
await conn.OpenAsync();

const string sql = """
    SELECT p.proname,
           pg_get_function_identity_arguments(p.oid) AS args,
           pg_get_function_result(p.oid) AS result
    FROM pg_proc p
    JOIN pg_namespace n ON p.pronamespace = n.oid
    WHERE n.nspname = 'public'
    ORDER BY p.proname, args;
    """;

await using var cmd = new NpgsqlCommand(sql, conn);
await using var reader = await cmd.ExecuteReaderAsync();
while (await reader.ReadAsync())
    Console.WriteLine($"{reader.GetString(0)}({reader.GetString(1)}) -> {reader.GetString(2)}");
