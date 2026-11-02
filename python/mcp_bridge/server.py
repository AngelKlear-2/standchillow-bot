"""MCP server bridging the model to the C# controller over 127.0.0.1:7777 (newline-delimited JSON)."""

import asyncio
import json

from mcp.server.fastmcp import FastMCP

BRIDGE_HOST = "127.0.0.1"
BRIDGE_PORT = 7777

mcp = FastMCP("standchillow-bot")


async def _call(cmd: str) -> dict:
    reader, writer = await asyncio.open_connection(BRIDGE_HOST, BRIDGE_PORT)
    try:
        writer.write((json.dumps({"cmd": cmd}) + "\n").encode("utf-8"))
        await writer.drain()
        line = await reader.readline()
        return json.loads(line.decode("utf-8"))
    finally:
        writer.close()
        await writer.wait_closed()


@mcp.tool()
async def get_game_state() -> str:
    """Return JSON with the local player position and the enemy position."""
    return json.dumps(await _call("get_game_state"))


@mcp.tool()
async def walk_randomly() -> str:
    """Tell the controller to move randomly (forward / strafe)."""
    return json.dumps(await _call("walk_randomly"))


@mcp.tool()
async def engage_enemy() -> str:
    """Aim at the enemy (70% head / 30% chest) and fire when on target."""
    return json.dumps(await _call("engage_enemy"))


@mcp.tool()
async def stop_bot() -> str:
    """Emergency stop: halt all movement and aim input."""
    return json.dumps(await _call("stop_bot"))


if __name__ == "__main__":
    mcp.run()
