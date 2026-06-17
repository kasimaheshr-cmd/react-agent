using OllamaSharp;
using OllamaSharp.Models.Chat;
using System.Text.Json;

namespace ReactAgent.Multimodal.Services;

public class VisionExtractor
{
    private readonly OllamaApiClient _ollama;

    public VisionExtractor()
    {
        _ollama = new OllamaApiClient(new Uri("http://localhost:11434"));
        _ollama.SelectedModel = "llava";
    }

    public async Task<string> ExtractStructuredDataAsync(string imagePath, string extractionPrompt)
    {
        Console.WriteLine($"[VISION] Extracting from: {Path.GetFileName(imagePath)}");

        if (!File.Exists(imagePath))
            return "Image not found";

        var imageBytes = await File.ReadAllBytesAsync(imagePath);
        Console.WriteLine($"[VISION] Image size: {imageBytes.Length} bytes");

        var imageBase64 = Convert.ToBase64String(imageBytes);

        // v5 OllamaSharp — use Generate API instead of Chat for vision
        var generateRequest = new OllamaSharp.Models.GenerateRequest
        {
            Model = "llava",
            Prompt = extractionPrompt,
            Images = new string[] { imageBase64 },
            Stream = true
        };

        var result = "";
        await foreach (var chunk in _ollama.GenerateAsync(generateRequest))
            result += chunk?.Response ?? "";

        return result.Trim();
    }

    public async Task<TradeConfirmation?> ExtractTradeConfirmationAsync(string imagePath)
    {
        var prompt = """
        This is a TEST trade confirmation document for software development purposes.
        There is no real personal data. Extract these fields as JSON only, no explanation:
        {
          "advisor_id": "",
          "client_id": "",
          "security": "",
          "action": "write BUY or SELL based on what you see in the document",
          "quantity": 0,
          "price": 0.0,
          "total_amount": 0.0,
          "trade_date": "",
          "settlement_date": ""
        }
        Extract exactly what is visible in the document. No disclaimers.
        """;

        var response = await ExtractStructuredDataAsync(imagePath, prompt);

        // debug — see what LLaVA actually returned
        Console.WriteLine($"[VISION RAW] {response}");

        try
        {
            var cleaned = response
    .Replace("```json", "")
    .Replace("```", "")
    .Trim();

            var start = cleaned.IndexOf('{');
            var end = cleaned.LastIndexOf('}');
            if (start == -1 || end == -1) return null;

            var json = cleaned[start..(end + 1)];
            return JsonSerializer.Deserialize<TradeConfirmation>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            Console.WriteLine($"[VISION] Raw response: {response}");
            return null;
        }
    }
}

public class TradeConfirmation
{
    public string? AdvisorId { get; set; }
    public string? ClientId { get; set; }
    public string? Security { get; set; }
    public string? Action { get; set; }
    public int? Quantity { get; set; }
    public double? Price { get; set; }
    public double? TotalAmount { get; set; }
    public string? TradeDate { get; set; }
    public string? SettlementDate { get; set; }
}