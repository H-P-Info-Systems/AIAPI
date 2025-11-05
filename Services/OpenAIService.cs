using OpenAI;
using OpenAI.Chat;
using System.Text.Json;

namespace SmartApi.Services;

public class OpenAIService : IAIService
{
    private readonly OpenAIClient _client;
    private readonly ChatClient _chatClient;
    private readonly string _model;

    public OpenAIService(IConfiguration config)
    {
        var apiKey = config["OpenAI:ApiKey"] ?? "YOUR_OPENAI_API_KEY";
        _client = new OpenAIClient(apiKey);
        _chatClient = new ChatClient("gpt-4.1-mini", apiKey);

        _model = config["OpenAI:Model"] ?? "gpt-4o-mini";
    }

    public async Task<string> ExtractIntent(string question)
    {
        var response = await _client.ChatCompletions.CreateAsync(
            model: _model,
            messages: [
                new ChatMessage(ChatRole.System, "You are a search intent extractor. Return only the minimal keyword(s) or filter logic (no explanation)."),
                new ChatMessage(ChatRole.User, question)
            ]
        );

        return response.Value?.Choices?.FirstOrDefault()?.Message?.Content?[0]?.Text?.Trim() ?? string.Empty;
    }

    public async Task<string> GenerateAnswer(string question, IEnumerable<object> data)
    {
        var json = JsonSerializer.Serialize(data);

        var response = await _client.ChatCompletions.CreateAsync(
            model: _model,
            messages: [
                new ChatMessage(ChatRole.System, "You are an assistant that answers using only the provided data. If data is insufficient, say so."),
                new ChatMessage(ChatRole.User, $"Question: {question}\nData: {json}\nRespond concisely.")
            ]
        );

        return response.Value?.Choices?.FirstOrDefault()?.Message?.Content?[0]?.Text?.Trim() ?? "No answer generated.";
    }
}
