using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ScrumMasterBot.Dailies;

public class Manager
{
    public IReadOnlyCollection<Id> GameIds => _games.Keys.ToImmutableList();

    public State GetOrPutGameState(Id id)
    {
        if (!_games.TryGetValue(id, out State? gamesState))
        {
            gamesState = _games[id] = new State();
        }
        
        return gamesState!;
    }

    ConcurrentDictionary<Id, State> _games = new();
}