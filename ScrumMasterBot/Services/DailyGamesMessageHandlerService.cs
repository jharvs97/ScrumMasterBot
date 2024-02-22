using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using ScrumMasterBot.Dailies;

namespace ScrumMasterBot.Services;

public class DailyGamesMessageHandlerService(DiscordSocketClient client, Dailies.Manager dailies) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        client.MessageReceived += OnMessageRecievedAsync;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    static ConcurrentDictionary<ulong, ulong> _messageIds = new();

    async Task OnMessageRecievedAsync(SocketMessage message)
    {
        if (message is not SocketUserMessage userMessage)
            return;

        if (userMessage.Author.Id == client.CurrentUser.Id && userMessage.Author.IsBot)
            return;

        var id = new Id(userMessage.Timestamp.ToLocalTime().Date);
        var gameState = dailies.GetOrPutGameState(id);

        if (gameState.TryUpdate(userMessage))
        {
            string gameStateMessage = FormatGameStateString(id, gameState);
            
            if (_messageIds.ContainsKey(message.Channel.Id))
            {
                await message.Channel.ModifyMessageAsync(_messageIds[message.Channel.Id], props => props.Content = gameStateMessage);
            }
            else
            {
                var botMessage = await message.Channel.SendMessageAsync(gameStateMessage);
                _messageIds[message.Channel.Id] = botMessage.Id;
            }

            await userMessage.DeleteAsync();
        }
    }

    string FormatGameStateString(Id id, State state)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"## **Dailies {id.Date}**");
        foreach (var (userId, userState) in state.UserStates)
        {
            var user = client.GetUser(userId);
            sb.AppendLine($"### {user.Username}");
            foreach (var score in userState.Scores)
            {
                sb.AppendLine($"- {score}");
            }
        }

        return sb.ToString();
    }
}
