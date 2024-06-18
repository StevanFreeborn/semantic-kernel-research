
using System.Net;

using Codeblaze.SemanticKernel.Connectors.Ollama;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

var builder = Kernel.CreateBuilder();

builder.Services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Information));
builder.Services.ConfigureHttpClientDefaults(
  c => c
    .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromMinutes(20))
    .AddStandardResilienceHandler()
    .Configure(
      o => 
      {
        o.AttemptTimeout = new() { Timeout = TimeSpan.FromMinutes(5) };
        o.TotalRequestTimeout = new() { Timeout = TimeSpan.FromMinutes(15) };
        o.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(10);
        o.Retry.ShouldHandle = args => ValueTask.FromResult(args.Outcome.Result?.StatusCode is HttpStatusCode.Unauthorized);
      }
    )
);

builder.AddOllamaChatCompletion(modelId: "llama3", baseUrl: "http://localhost:11434");

var kernel = builder.Build();

var logger = kernel.LoggerFactory.CreateLogger(typeof(Program));

const string docsPath = @"C:\Users\StevanFreeborn\Repositories\help-center-scrapping\HelpCenterScrapping\docs";

var docs = Directory.GetFiles(docsPath, "*.txt", SearchOption.AllDirectories);

var promptTemplate = """
<|begin_of_text|>
  <|start_header_id|>system<|end_header_id|>
    You are an exceptionally talented and knowledge administrator of the Onspring platform.

    You are also a gifted communicator and educator.

    You will be given an article from the Onspring Help Center.

    Your task is to analyze the article and come up with a list of 20 questions that a user might ask that can
    be answered by the content of the article.

    You should always return the list of questions as a properly formatted JSON array
    with no additional information or formatting. It is crucial that this format
    is followed exactly to ensure that the questions can be processed correctly.
  <|eot_id|>
  <|start_header_id|>user<|end_header_id|>
    Here is the article:
    {{$input}}
  <|eot_id|>
  <|start_header_id|>assistant<|end_header_id|>
<|end_of_text|>
""";

var docQuestions = await GetQuestionsAsync(docs[2], kernel, promptTemplate, CancellationToken.None);

foreach (var question in docQuestions)
{
  Console.WriteLine(question);
}

static async Task<string[]> GetQuestionsAsync(string doc, Kernel kernel, string promptTemplate, CancellationToken token)
{
  var content = File.ReadAllText(doc);
  var docName = Path.GetFileNameWithoutExtension(doc);

  var result = await kernel.InvokePromptAsync(promptTemplate, new KernelArguments { { "input", content } }, cancellationToken: token);
  var questions = result.GetValue<string[]>();

  if (questions is null)
  {
    Console.WriteLine($"No questions found for {docName}");
    return [];
  }

  return questions;
}