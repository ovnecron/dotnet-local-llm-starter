using System.Text;
using Microsoft.Extensions.AI;
using OllamaSharp;

var modelName = Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? "llama3.2:1b";
var rawOllamaEndpoint = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? "http://localhost:11434";
var ollamaEndpoint = rawOllamaEndpoint.EndsWith('/') ? rawOllamaEndpoint : rawOllamaEndpoint + "/";
var systemPrompt = Environment.GetEnvironmentVariable("OLLAMA_SYSTEM_PROMPT");
var maxTurns = int.TryParse(Environment.GetEnvironmentVariable("OLLAMA_MAX_TURNS"), out var parsedMaxTurns) && parsedMaxTurns > 0
    ? parsedMaxTurns
    : 10;

IChatClient client = new OllamaApiClient(new Uri(ollamaEndpoint), modelName);
var chatHistory = new List<ChatMessage>();
if (!string.IsNullOrWhiteSpace(systemPrompt))
{
    chatHistory.Add(new ChatMessage(ChatRole.System, systemPrompt));
}

Console.WriteLine("=== .NET Local AI Chat ===");
Console.WriteLine($"Model: {modelName}");
Console.WriteLine($"Endpoint: {rawOllamaEndpoint}");
Console.WriteLine("Type 'exit' to quit.\n");

while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("User: ");
    Console.ResetColor();

    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("AI: ");

    try
    {
        chatHistory.Add(new ChatMessage(ChatRole.User, input));
        TrimHistory(chatHistory, maxTurns);
        var assistantText = new StringBuilder();

        await foreach (var update in client.GetStreamingResponseAsync(chatHistory))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                assistantText.Append(update.Text);
                Console.Write(update.Text);
            }
        }

        var assistant = assistantText.ToString();
        if (!string.IsNullOrWhiteSpace(assistant))
        {
            chatHistory.Add(new ChatMessage(ChatRole.Assistant, assistant));
            TrimHistory(chatHistory, maxTurns);
        }
        Console.WriteLine("\n");
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("model", StringComparison.OrdinalIgnoreCase)
                                               && ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Model '{modelName}' is not available in Ollama.");
        Console.WriteLine($"Run: ollama pull {modelName}");
        Console.WriteLine("Then start this app again.");
        Console.ResetColor();
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Request failed: {ex.Message}. Ensure Ollama is running and model '{modelName}' exists.");
        Console.ResetColor();
    }

    Console.ResetColor();
}

static void TrimHistory(List<ChatMessage> chatHistory, int maxTurns)
{
    var hasSystemMessage = chatHistory.Count > 0 && chatHistory[0].Role == ChatRole.System;
    var firstRemovableIndex = hasSystemMessage ? 1 : 0;
    var maxMessages = maxTurns * 2 + (hasSystemMessage ? 1 : 0);

    while (chatHistory.Count > maxMessages && chatHistory.Count > firstRemovableIndex)
    {
        // Remove whole turns from the oldest side: User first, then its paired Assistant if present.
        if (chatHistory[firstRemovableIndex].Role == ChatRole.User)
        {
            chatHistory.RemoveAt(firstRemovableIndex);
            if (chatHistory.Count > firstRemovableIndex && chatHistory[firstRemovableIndex].Role == ChatRole.Assistant)
            {
                chatHistory.RemoveAt(firstRemovableIndex);
            }
            continue;
        }

        // If history is malformed (e.g., orphan assistant), drop that entry and continue.
        chatHistory.RemoveAt(firstRemovableIndex);
    }
}
