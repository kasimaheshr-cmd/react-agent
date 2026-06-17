using ReactAgent.Multimodal.Services;

var extractor = new VisionExtractor();
var pdfPipeline = new PdfVisionPipeline();

Console.WriteLine("=== Day 3 — PDF Vision Pipeline ===\n");

// create a simple test PDF first
var testPdfPath = @"C:\Users\mahesh.kasireddy\Desktop\test.pdf";

if (!File.Exists(testPdfPath))
{
    Console.WriteLine("No PDF found at C:\\image\\test.pdf");
    Console.WriteLine("Place any PDF there and rerun.");
    return;
}

var results = await pdfPipeline.ProcessPdfAsync(testPdfPath);

Console.WriteLine($"\n[PDF] Extraction complete. {results.Count} pages processed.\n");

foreach (var result in results)
{
    Console.WriteLine($"Page {result.PageNumber}:");
    Console.WriteLine($"  Method:     {result.Method}");
    Console.WriteLine($"  Confidence: {result.Confidence}");
    Console.WriteLine($"  Content:    {result.RawText[..Math.Min(200, result.RawText.Length)]}...");
    Console.WriteLine();
}