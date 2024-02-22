using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.WebSocket;

namespace ScrumMasterBot.Dailies;

public class State
{
    public IReadOnlyDictionary<ulong, UserState> UserStates => _gameScores.ToImmutableDictionary();

    public bool TryUpdate(SocketUserMessage message)
    {
        if (!_gameScores.TryGetValue(message.Author.Id, out var userState))
        {
            userState = _gameScores[message.Author.Id] = new UserState();
        }

        return userState.TryAddScore(message.Content);
    }

    ConcurrentDictionary<ulong, UserState> _gameScores = new();
}

public class UserState
{
    public IReadOnlyCollection<string> Scores => _scores.Select(kvp => kvp.Value).ToImmutableArray();
    public bool TryAddScore(string input)
    {
        var (game, result) = GameParser.Parse(input);
        if (game is not Games.None && !string.IsNullOrEmpty(result) && !_scores.ContainsKey(game))
        {
            _scores[game] = result;
            return true;
        }
        return false;
    }

    ConcurrentDictionary<Games, string> _scores = new();
}