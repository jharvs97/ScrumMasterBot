using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScrumMasterBot.Services;

namespace ScrumMasterBot;

internal static class Program 
{
    public static async Task Main(string[] args) 
    {
        var discordConfig = new DiscordSocketConfig() 
        { 
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent 
        };

        using IHost host = Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(config => 
            {
                config.AddJsonFile("config.json", false);
            })
            .ConfigureServices(services => 
            {
                services.AddLogging(builder => builder.AddConsole());

                services.AddSingleton(new DiscordSocketClient(discordConfig));
                services.AddSingleton<CommandService>();
                services.AddSingleton<Dailies.Manager>();

                services.AddHostedService<DiscordStartupService>();
                services.AddHostedService<CommandHandlerService>();
                services.AddHostedService<DailyGamesMessageHandlerService>();
            })
            .Build();

        await host.RunAsync();
    }
}
