using DockerUpdater.Api.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace DockerUpdater.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        string modelId = "command-a-03-2025";
        var endpoint = "https://api.cohere.ai/compatibility/v1";
        var apiKey = "7cxVrb4DyCDhsqZ6cenKXAkGD8ZElgjTrWIcVFaw";
        var builder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId, new Uri(endpoint), apiKey);

        builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace)); 
        
        Kernel kernel = builder.Build();
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        kernel.Plugins.AddFromType<DockerVersionFinderPlugin>("DockerVersionFinder");

        
        // Enable planning
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

// Create a history store the conversation
        var history = new ChatHistory();

// Initiate a back-and-forth chat
        string? userInput;
        do
        {
            // Collect user input
            Console.Write("User > ");
            userInput = Console.ReadLine();

            // Add user input
            history.AddUserMessage(userInput);

            // Get the response from the AI
            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel);

            // Print the results
            Console.WriteLine("Assistant > " + result);

            // Add the message from the agent to the chat history
            history.AddMessage(result.Role, result.Content ?? string.Empty);
        } while (userInput is not null);

        // TODO: activate for api 
        //var builder = WebApplication.CreateBuilder(args);
        //
        // // Add services to the container.
        // builder.Services.AddAuthorization();
        //
        // // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        // builder.Services.AddOpenApi();
        //
        // var app = builder.Build();
        //
        // // Configure the HTTP request pipeline.
        // if (app.Environment.IsDevelopment())
        // {
        //     app.MapOpenApi();
        // }
        //
        // app.UseHttpsRedirection();
        //
        // app.UseAuthorization();
        //
        //
        //
        // app.Run();
    }
}