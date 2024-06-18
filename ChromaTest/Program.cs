#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0020, SKEXP0050

using Codeblaze.SemanticKernel.Connectors.Ollama;

using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Text;
using System.Text.Json;

// var builder = new MemoryBuilder()
//   .WithHttpClient(new() { Timeout = TimeSpan.FromMinutes(5) })
//   .WithChromaMemoryStore("http://localhost:3111")
//   .WithOllamaTextEmbeddingGeneration(modelId: "nomic-embed-text", baseUrl: "http://localhost:11434");

// var memory = builder.Build();

// const string collectionName = "ChromaTest";
// const string docsPath = @"C:\Users\StevanFreeborn\Repositories\help-center-scrapping\HelpCenterScrapping\docs";

// var docs = Directory.GetFiles(docsPath, "*.txt", SearchOption.AllDirectories);

// await Parallel.ForEachAsync(docs, async (doc, token) =>
// {
//   var content = File.ReadAllText(doc);
//   var docName = Path.GetFileNameWithoutExtension(doc);
  
//   var lines = TextChunker.SplitPlainTextLines(content, 40);
//   var chunks = TextChunker.SplitPlainTextParagraphs(lines, 120);

//   foreach (var (chunk, index) in chunks.Select((chunk, index) => (chunk, index)))
//   {
//     await memory.SaveInformationAsync(
//         collection: collectionName,
//         id: $"{docName}_{index}",
//         description: docName,
//         text: chunk,
//         cancellationToken: token
//       );
//   }

//   Console.WriteLine($"Saved document {docName}");
// });

// var query = @"how, full, user";

// var results = memory.SearchAsync(
//   collection: collectionName,
//   query: query,
//   limit: 10,
//   minRelevanceScore: 0.5
// );

// var i = 0;

// await foreach (var result in results)
// {
//   Console.WriteLine($"Result {++i}:");
//   Console.WriteLine("  URL      : " + result.Metadata.Id);
//   Console.WriteLine("  Relevance: " + result.Relevance);
//   Console.WriteLine("  Text     : " + result.Metadata.Text);
//   Console.WriteLine();
// }

var json = File.ReadAllText("questionsMap.json");
var questionsMap = JsonSerializer.Deserialize<Dictionary<string, string[]>>(json);