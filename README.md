# .NET Local LLM Starter

This is a minimal starter to run a local LLM via Ollama using C# with `Microsoft.Extensions.AI` and `OllamaSharp`.
No API keys, no credit cards, and no prompts leave your machine (when using local Ollama).

Companion repo for the blog post: https://www.lukaswalter.dev/posts/local-llms-in-.net/

## Prerequisites

1. Ollama
   - Download: https://ollama.com/
   - Pull a model:
     ```bash
     ollama pull llama3.2:1b
     ```
   - Verify Ollama is reachable:
     ```bash
     ollama list
     ```
   - If you get a connection error, start it:
     ```bash
     ollama serve
     ```
2. .NET 10 SDK or later

## Dependencies

If you cloned this repo, you can skip this section. The packages are already referenced in `src/LocalLLM.Console/LocalLLM.Console.csproj`.

```bash
dotnet add src/LocalLLM.Console/LocalLLM.Console.csproj package Microsoft.Extensions.AI
dotnet add src/LocalLLM.Console/LocalLLM.Console.csproj package OllamaSharp
```

## Quick Start

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/dotnet-local-llm-starter.git
   cd dotnet-local-llm-starter
   ```
2. Run:
   ```bash
   dotnet run --project src/LocalLLM.Console/LocalLLM.Console.csproj
   ```

Run with a different model (pull it first: `ollama pull llama3.2:3b`):

```bash
OLLAMA_MODEL="llama3.2:3b" dotnet run --project src/LocalLLM.Console/LocalLLM.Console.csproj
```

```powershell
$env:OLLAMA_MODEL="llama3.2:3b"; dotnet run --project src/LocalLLM.Console/LocalLLM.Console.csproj
```

## How It Works

This project uses `OllamaSharp` (`OllamaApiClient`) behind the `IChatClient` interface.
By coding against `IChatClient`, you can swap providers later with minimal app code changes.

```csharp
using Microsoft.Extensions.AI;
using OllamaSharp;

var modelName = Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? "llama3.2:1b";
var rawOllamaEndpoint = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? "http://localhost:11434";
var ollamaEndpoint = rawOllamaEndpoint.EndsWith('/') ? rawOllamaEndpoint : rawOllamaEndpoint + "/";
IChatClient client = new OllamaApiClient(new Uri(ollamaEndpoint), modelName);

// Provider-agnostic usage
var response = await client.GetResponseAsync("Why is C# the best language?");
```

## Configuration

Optional environment variables:

- `OLLAMA_MODEL` (default: `llama3.2:1b`)
- `OLLAMA_ENDPOINT` (default: `http://localhost:11434`)
- `OLLAMA_SYSTEM_PROMPT` (optional)
- `OLLAMA_MAX_TURNS` (default: `10`)

### Example: macOS/Linux

```bash
export OLLAMA_MODEL="llama3.2:3b"
export OLLAMA_ENDPOINT="http://localhost:11434"
dotnet run --project src/LocalLLM.Console/LocalLLM.Console.csproj
```

### Example: Windows (PowerShell)

```powershell
$env:OLLAMA_MODEL="llama3.2:3b"
$env:OLLAMA_ENDPOINT="http://localhost:11434"
dotnet run --project src/LocalLLM.Console/LocalLLM.Console.csproj
```

## Troubleshooting

- **Connection refused / request failed:** start Ollama (`ollama serve`) and check `OLLAMA_ENDPOINT`.
- **Model not found:** `ollama pull <model>` and retry.
- **Slow first response:** normal; model loading/warmup can take a moment.

## Next Steps

- Add a stronger default system prompt for your use case.
- Tune `OLLAMA_MAX_TURNS` for longer vs. cheaper sessions.
- Persist chat history for resumable conversations.

## License

MIT
