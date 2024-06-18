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

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
builder.AddOpenAIChatCompletion(
  modelId: "llama3-70b-8192",
  endpoint: new Uri("https://api.groq.com/openai/v1/chat/completions"),
  apiKey: ""
);
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

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
    Console.WriteLine("      Bot: How can I help you today?");
    Console.Write("      User: ");
    var input = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(input))
    {
      continue;
    }

    Console.WriteLine("      Bot: Sure thing! Let me think...");
    Console.WriteLine("      Bot: {0}", await kernel.InvokePromptAsync(promptTemplate, new KernelArguments { { "input", input } }));
  }
  catch (Exception ex)
  {
    logger.LogError(ex, "An error occurred while processing the request.");
  }
}