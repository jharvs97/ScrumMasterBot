using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ScrumMasterBot.Services;

public class DiscordStartupService(DiscordSocketClient client, IConfiguration config, ILogger<DiscordStartupService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        client.Log += (LogMessage message) => {
            logger.LogInformation(message.ToString());
            return Task.CompletedTask;
        };

        await client.LoginAsync(TokenType.Bot, config["token"]);
        await client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await client.LogoutAsync();
        await client.StopAsync();
    }
}
