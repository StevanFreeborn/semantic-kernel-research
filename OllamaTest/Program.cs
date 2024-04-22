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

var promptTemplate = """
Bot: How can I help you today?
User: {{$input}}

------------------------------

Provide the most accurate and honest answer to the user's question:
""";

while (true)
{
  try {
    logger.LogInformation("Bot: How can I help you today?");
    var input = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(input))
    {
      continue;
    }

    logger.LogInformation("Sure thing! Let me think...");
    logger.LogInformation("Bot: {Response}", await kernel.InvokePromptAsync(promptTemplate, new KernelArguments { { "input", input } }));
  }
  catch (Exception ex)
  {
    logger.LogError(ex, "An error occurred while processing the request.");
  }
}