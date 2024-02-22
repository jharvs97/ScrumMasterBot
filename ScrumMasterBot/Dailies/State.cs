using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.WebSocket;
using Microsoft.VisualBasic;

namespace ScrumMasterBot.Dailies;

public class State
{
    public IReadOnlyDictionary<ulong, UserState> UserStates => _gameScores.ToImmutableDictionary();

    public IError? TryUpdate(SocketUserMessage message)
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
    public IError? TryAddScore(string input)
    {
        var result = ParserUtils.Parse(input);
        if (result is not IError err)
        {
            var (game, score) = result.Value;
            _scores[game] = score;
            return null;
        }

        return new Error<string>(err.Reason);
    }
    ConcurrentDictionary<Games, string> _scores = new();
}