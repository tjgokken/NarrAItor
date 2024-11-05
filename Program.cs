using Blazored.LocalStorage;
using Blazored.Toast;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NarrAItor;
using NarrAItor.Services;
using NarrAItor.Services.Commands;
using NarrAItor.Services.Commands.Suggestions;
using NarrAItor.Services.Strategies;
using NarrAItor.Shared.Interfaces;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

#if DEBUG
// Disable Browser Link in development
builder.Logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.None);
builder.Logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.None);
#endif

builder.Services.AddScoped<IGameService, GameClientService>();
builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7200")
});
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddLogging();
builder.Services.AddGameCommands().AddGameCommandDocumentation();
// Add Toast Service
builder.Services.AddBlazoredToast();

await builder.Build().RunAsync();

public static class CommandHandlerExtensions
{
    public static IServiceCollection AddGameCommands(this IServiceCollection services)
    {
        services.AddScoped<CommandHandler>();
        services.AddScoped<ICommand, LookCommand>();
        services.AddScoped<ICommand, InventoryCommand>();
        services.AddScoped<ICommand, TakeCommand>();
        services.AddScoped<ICommand, MovementCommand>();
        services.AddScoped<ICommand, DropCommand>();
        services.AddScoped<ICommand, HelpCommand>();
        services.AddScoped<ICommand, TalkCommand>();
        services.AddScoped<ICommand, UseCommand>();
        services.AddScoped<ICommandSuggestionService, CommandSuggestionService>();
        services.AddScoped<ICommand, SuggestCommand>();


        return services;
    }
}