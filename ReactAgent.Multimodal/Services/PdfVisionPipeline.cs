using OllamaSharp;
using OllamaSharp.Models.Chat;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System.Drawing;
using System.Drawing.Imaging;

namespace ReactAgent.Multimodal.Services;

public class PdfVisionPipeline
{
    private readonly OllamaApiClient _ollama;
    private readonly VisionExtractor _extractor;

    public PdfVisionPipeline()
    {
        _ollama = new OllamaApiClient(new Uri("http://localhost:11434"));
        _ollama.SelectedModel = "llava";
        _extractor = new VisionExtractor();
    }

    public async Task<List<PageExtractionResult>> ProcessPdfAsync(string pdfPath)
    {
        Console.WriteLine($"[PDF] Processing: {Path.GetFileName(pdfPath)}");
        var results = new List<PageExtractionResult>();

        using var pdf = PdfDocument.Open(pdfPath);
        Console.WriteLine($"[PDF] Pages found: {pdf.NumberOfPages}");

        foreach (var page in pdf.GetPages())
        {
            Console.WriteLine($"[PDF] Processing page {page.Number}...");

            // try text extraction first — fast path
            var text = string.Join(" ", page.GetWords().Select(w => w.Text));

            if (text.Length > 100)
            {
                // clean text PDF — use direct extraction
                Console.WriteLine($"[PDF] Page {page.Number} → text path ({text.Length} chars)");
                results.Add(new PageExtractionResult
                {
                    PageNumber = page.Number,
                    Method = "text",
                    RawText = text,
                    Confidence = "high"
                });
            }
            else
            {
                // scanned page — render to image and use vision
                Console.WriteLine($"[PDF] Page {page.Number} → vision path (sparse text)");
                var imagePath = await RenderPageToImageAsync(page, pdfPath);
                var extracted = await _extractor.ExtractStructuredDataAsync(
                    imagePath,
                    "Extract all text, numbers, dates and amounts visible in this document page. Be precise."
                );

                results.Add(new PageExtractionResult
                {
                    PageNumber = page.Number,
                    Method = "vision",
                    RawText = extracted,
                    Confidence = extracted.Length > 50 ? "medium" : "low"
                });

                // cleanup temp image
                if (File.Exists(imagePath)) File.Delete(imagePath);
            }
        }

        return results;
    }

    private async Task<string> RenderPageToImageAsync(Page page, string pdfPath)
    {
        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"page_{page.Number}_{Guid.NewGuid()}.png"
        );

        // always use trade.png for local dev demo
        // in production use Docnet.Core or iText7 for proper page rendering
        File.Copy(@"C:\Users\mahesh.kasireddy\Pictures\Screenshots\trade.png", outputPath, overwrite: true);
        Console.WriteLine($"[PDF] Using trade.png as page {page.Number} image");

        return outputPath;
    }
}

public class PageExtractionResult
{
    public int PageNumber { get; set; }
    public string Method { get; set; } = string.Empty;  // "text" or "vision"
    public string RawText { get; set; } = string.Empty;
    public string Confidence { get; set; } = string.Empty;  // "high" | "medium" | "low"
}