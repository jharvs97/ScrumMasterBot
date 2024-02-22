using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ScrumMasterBot.Services;

public class CommandHandlerService(DiscordSocketClient client, CommandService commands, ILogger<CommandHandlerService> logger, IServiceProvider services) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await commands.AddModulesAsync(Assembly.GetExecutingAssembly(), services);

        commands.Log += (LogMessage message) => 
        {
            logger.LogInformation(message.ToString());
            return Task.CompletedTask;
        };
        client.MessageReceived += HandleCommandAsync;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    async Task HandleCommandAsync(SocketMessage message)
    {
        if (message is not SocketUserMessage userMessage)
            return;

        if (userMessage.Author.Id == client.CurrentUser.Id && userMessage.Author.IsBot)
            return;

        var context = new SocketCommandContext(client, userMessage);

        var argPos = 0;
        if (userMessage.HasCharPrefix('!', ref argPos))
        {
            await commands.ExecuteAsync(context, argPos, services);
        }
    }
}
