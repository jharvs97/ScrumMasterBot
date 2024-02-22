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

    static ulong? _messageId;

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

            if (_messageId is null)
            {
                var botMessage = await message.Channel.SendMessageAsync(gameStateMessage);
                _messageId = botMessage.Id;
            }
            else
            {
                await message.Channel.ModifyMessageAsync((ulong)_messageId, props => props.Content = gameStateMessage);
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
