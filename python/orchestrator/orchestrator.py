"""Infinite loop: model -> MCP tool -> tool result -> model. Re-prompts on text-only replies.

Run the MCP server first (python -m mcp_bridge.server or via the MCP client config),
then run this file with ANTHROPIC_API_KEY set.
"""

import asyncio
import os
import sys

import anthropic
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

MODEL = "claude-haiku-5-5"
START_TASK = "Играй и убивай"
NUDGE = "Ты ответил текстом без вызова инструмента. Вызови инструмент сейчас."
MAX_TOKENS = 512


async def run() -> None:
    client = anthropic.AsyncAnthropic(api_key=os.environ["ANTHROPIC_API_KEY"])
    server = StdioServerParameters(
        command=sys.executable,
        args=["-m", "mcp_bridge.server"],
        cwd=os.path.join(os.path.dirname(__file__), ".."),
    )

    async with stdio_client(server) as (read, write):
        async with ClientSession(read, write) as session:
            await session.initialize()
            listed = await session.list_tools()
            tools = [
                {"name": t.name, "description": t.description or "", "input_schema": t.inputSchema}
                for t in listed.tools
            ]

            messages: list[dict] = [{"role": "user", "content": START_TASK}]

            while True:
                resp = await client.messages.create(
                    model=MODEL,
                    max_tokens=MAX_TOKENS,
                    tools=tools,
                    messages=messages,
                )
                messages.append({"role": "assistant", "content": resp.content})

                tool_uses = [b for b in resp.content if b.type == "tool_use"]
                if not tool_uses:
                    messages.append({"role": "user", "content": NUDGE})
                    continue

                results = []
                for block in tool_uses:
                    out = await session.call_tool(block.name, block.input)
                    text = "".join(c.text for c in out.content if getattr(c, "type", "") == "text")
                    results.append({"type": "tool_result", "tool_use_id": block.id, "content": text})

                messages.append({"role": "user", "content": results})


if __name__ == "__main__":
    asyncio.run(run())
