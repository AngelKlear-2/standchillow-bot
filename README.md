# standchillow-bot

Skeleton for an autonomous local-bot setup for StandChillow (Unity il2cpp, offline / LAN 1v1 test server).

## Layout

```
csharp/StandChillowCheat/   external reader + aim/trigger controller, exposes localhost TCP (127.0.0.1:7777)
python/mcp_bridge/          MCP server (official SDK) that wraps the TCP bridge as tools
python/orchestrator/        while-True loop driving the model via the Anthropic API (Haiku)
```

## Architecture

- C# process reads game memory via OpenProcess / ReadProcessMemory (external, read-only by default).
- C# exposes a newline-delimited JSON protocol on `127.0.0.1:7777`.
- Python MCP server connects to that socket and registers `get_game_state`, `walk_randomly`, `engage_enemy`, `stop_bot`.
- Orchestrator calls the Anthropic API, feeds tool results back in a loop, re-prompts when the model returns text without a tool call.

## Status

- offsets are not in yet: every `off::` value is `FILL` and must be replaced from the dump.
- nothing is wired to live memory yet; the TCP command handlers return explicit `not_implemented` until offsets land.

## Build

```
dotnet build csharp/StandChillowCheat -c Release
pip install -r python/requirements.txt
```
