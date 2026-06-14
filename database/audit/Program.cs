using System.Net.Http.Json;
using System.Text.Json;
using Npgsql;

var connStr = Environment.GetEnvironmentVariable("TICTACTOE_DB")
    ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres;";
const string apiBase = "https://localhost:7241/api/";

async Task ApplyMigration(string file)
{
    var sql = await File.ReadAllTextAsync(file);
    await using var conn = new NpgsqlConnection(connStr);
    await conn.OpenAsync();
    await using var cmd = new NpgsqlCommand(sql, conn);
    await cmd.ExecuteNonQueryAsync();
    Console.WriteLine($"Migration applied: {Path.GetFileName(file)}");
}

if (args.Length > 0 && args[0] == "apply")
{
    await ApplyMigration(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "migrations", "001_standardize_functions.sql"));
    return;
}

if (args.Length > 0 && args[0] == "apply-fix")
{
    await ApplyMigration(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "migrations", "002_fix_return_type_casts.sql"));
    return;
}

if (args.Length > 0 && args[0] == "list")
{
    await using var conn = new NpgsqlConnection(connStr);
    await conn.OpenAsync();
    const string sql = """
        SELECT proname, pg_get_function_identity_arguments(p.oid)
        FROM pg_proc p JOIN pg_namespace n ON p.pronamespace = n.oid
        WHERE n.nspname = 'public'
          AND proname IN ('create_game','add_move','update_game','get_game','get_moves',
                          'get_scoreboard','reset_scoreboard','remove_last_moves',
                          'get_next_move_number','undo_move','reset_game')
        ORDER BY 1, 2;
        """;
    await using var cmd = new NpgsqlCommand(sql, conn);
    await using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
        Console.WriteLine($"{reader.GetString(0)}({reader.GetString(1)})");
    return;
}

if (args.Length > 0 && args[0] == "draw-test")
{
    using var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
    using var http = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7000/api/") };

    async Task<JsonElement> Post(string path, object body)
    {
        var resp = await http.PostAsJsonAsync(path, body);
        var text = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode) throw new Exception($"{path} -> {(int)resp.StatusCode}: {text}");
        return JsonDocument.Parse(text).RootElement;
    }

    var game = await Post("games", new { mode = "TwoPlayer" });
    var id = game.GetProperty("gameId").GetGuid();
    var seq = new[] { (0,'X'),(1,'O'),(2,'X'),(4,'O'),(3,'X'),(5,'O'),(7,'X'),(6,'O'),(8,'X') };
    JsonElement state = default;
    foreach (var (cell, player) in seq)
        state = await Post($"games/{id}/moves", new { player = player.ToString(), cellIndex = cell });

    Console.WriteLine(state.GetProperty("status").GetString() == "Draw" ? "DRAW OK via gateway" : "DRAW FAIL");
    return;
}

if (args.Length > 0 && args[0] == "verify")
{
    using var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
    using var http = new HttpClient(handler) { BaseAddress = new Uri(apiBase) };

    async Task<JsonElement> Post(string path, object? body = null)
    {
        var resp = body is null
            ? await http.PostAsync(path, null)
            : await http.PostAsJsonAsync(path, body);
        var text = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            throw new Exception($"POST {path} -> {(int)resp.StatusCode}: {text}");
        return JsonDocument.Parse(text).RootElement;
    }

    async Task<JsonElement> Get(string path)
    {
        var resp = await http.GetAsync(path);
        var text = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            throw new Exception($"GET {path} -> {(int)resp.StatusCode}: {text}");
        return JsonDocument.Parse(text).RootElement;
    }

    var created = await Post("games", new { mode = "TwoPlayer" });
    var gameId = created.GetProperty("gameId").GetGuid();
    Console.WriteLine($"OK POST /games -> {gameId}");

    await Get($"games/{gameId}");
    Console.WriteLine("OK GET /games/{{id}}");

    await Post($"games/{gameId}/moves", new { player = "X", cellIndex = 0 });
    Console.WriteLine("OK POST /games/{{id}}/moves");

    await Post($"games/{gameId}/undo");
    Console.WriteLine("OK POST /games/{{id}}/undo");

    await Post($"games/{gameId}/reset");
    Console.WriteLine("OK POST /games/{{id}}/reset");

    await Get("scoreboard");
    Console.WriteLine("OK GET /scoreboard");

    var resetResp = await http.PostAsync("scoreboard/reset", null);
    if ((int)resetResp.StatusCode != 204)
        throw new Exception($"POST /scoreboard/reset -> {(int)resetResp.StatusCode}");
    Console.WriteLine("OK POST /scoreboard/reset -> 204");

    var vs = await Post("games", new { mode = "VsComputer" });
    var vsId = vs.GetProperty("gameId").GetGuid();
    await Post($"games/{vsId}/moves", new { player = "X", cellIndex = 4 });
    Console.WriteLine("OK VsComputer move (add_move x2)");

    Console.WriteLine("\nAll 7 endpoints verified successfully.");
    return;
}

Console.WriteLine("Usage: dotnet run -- [apply|apply-fix|list|verify]");
