using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace StandChillowCheat;

// entry point. newline-delimited JSON over 127.0.0.1:7777.
// request:  {"cmd":"get_game_state"} | {"cmd":"walk_randomly"} | {"cmd":"engage_enemy"} | {"cmd":"stop_bot"}
// response: {"ok":true,...} or {"ok":false,"error":"..."}
internal static class Program
{
    private const int Port = 7777;

    private static async Task Main()
    {
        var proc = Process.GetProcessesByName(off.ProcessName).FirstOrDefault();
        if (proc is null)
        {
            Console.Error.WriteLine($"{off.ProcessName} not running");
            return;
        }

        using var mem = new ProcessMemory(proc.Id);
        mem.ResolveModuleBase(off.ModuleName);
        var bot = new BotController(mem);

        var listener = new TcpListener(IPAddress.Loopback, Port);
        listener.Start();
        Console.WriteLine($"bridge listening on 127.0.0.1:{Port}");

        while (true)
        {
            // client is owned by Serve, do not dispose it here
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => Serve(client, bot));
        }
    }

    private static async Task Serve(TcpClient client, BotController bot)
    {
        using var owned = client;
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        // UTF8Encoding(false): Encoding.UTF8 writes a BOM that breaks the Python json parser
        using var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };

        while (await reader.ReadLineAsync() is { } line)
        {
            string reply;
            try
            {
                using var doc = JsonDocument.Parse(line);
                string cmd = doc.RootElement.GetProperty("cmd").GetString() ?? "";
                reply = await Dispatch(cmd, bot);
            }
            catch (Exception ex)
            {
                reply = JsonSerializer.Serialize(new { ok = false, error = ex.Message });
            }
            await writer.WriteLineAsync(reply);
        }
    }

    private static async Task<string> Dispatch(string cmd, BotController bot)
    {
        object result = cmd switch
        {
            "get_game_state" => bot.GetGameState(),
            "walk_randomly" => bot.WalkRandomly(),
            "engage_enemy" => await bot.EngageEnemyAsync(),
            "stop_bot" => bot.Stop(),
            _ => new { ok = false, error = $"unknown cmd {cmd}" }
        };
        return JsonSerializer.Serialize(result);
    }
}
