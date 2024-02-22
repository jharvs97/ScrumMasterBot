using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Discord;

namespace ScrumMasterBot.Dailies;

public static class GameParser
{
    public static (Games game, string score) Parse(string input)
    {
        var result = (Games.None, string.Empty);

        if (string.IsNullOrEmpty(input))
            return result;

        var lines = input.Split('\n').ToList();

        try
        {
            foreach (var regex in Reggies)
            {
                var match = regex.Match(input);
                if (match.Success)
                {
                    var name = match.Groups[1].Value;
                    if (Enum.TryParse<Games>(name, true, out var game))
                    {
                        var score = game switch
                        {
                            Games.Wordle => ParseWordle(lines),
                            Games.Bandle => ParseBandle(lines),
                            Games.GuessTheGame => ParseGuessTheGame(lines),
                            Games.Connections => ParseConnections(lines),
                            _ => throw new ArgumentException("wtf")
                        };

                        result = (game, $"{name}: {score}");
                    }
                }
            }
        }
        catch (Exception)
        { }

        return result;
    }

    static readonly Regex WordleBandleRegex = new Regex(@"(Wordle|Bandle) #?[0-9]+ ([0-6]\/[0-6])", RegexOptions.Compiled);
    static readonly Regex GuessTheGameRegex = new Regex(@"#(GuessTheGame) #[0-9]+", RegexOptions.Compiled);
    static readonly Regex ConnectionsRegex = new Regex(@"(Connections)\s?\nPuzzle\s#[0-9]+", RegexOptions.Compiled | RegexOptions.Multiline);

    static ImmutableArray<Regex> Reggies = [
        WordleBandleRegex,
        GuessTheGameRegex,
        ConnectionsRegex,
    ];

    static string ParseWordle(IReadOnlyCollection<string> lines)
    {
        var sb = new StringBuilder();

        foreach (var line in lines.Skip(2))
        {
            int count = 0;
            foreach (var square in line.EnumerateRunes())
            {
                if (square == YellowSquare|| square == GreenSquare)
                {
                    count += 1;
                }
            }
            
            sb.Append(Regex.Unescape($"{count}\\uFE0F\\u20E3"));
        }

        return sb.ToString();
    }

    static string ParseBandle(IReadOnlyCollection<string> lines)
    {
        return lines.Skip(1).First();
    }

    static string ParseGuessTheGame(IReadOnlyCollection<string> lines)
    {
        return lines.Skip(2).First();
    }

    static string ParseConnections(IReadOnlyCollection<string> lines)
    {
        var sb = new StringBuilder();

        foreach (var line in lines.Skip(2))
        {
            var runes = line.EnumerateRunes();
            runes.MoveNext();
            var first = runes.Current;
            var current = first;
            
            while (first == current && runes.MoveNext())
            {
                current = runes.Current;
            }

            if (!runes.MoveNext())
            {
                sb.Append(first);
            }
            else
            {
                sb.Append(X);
            }
        }

        return sb.ToString();
    }

    static readonly Rune YellowSquare = new(0x1F7E8);
    static readonly Rune GreenSquare = new(0x1F7E9);
    static readonly string X = char.ConvertFromUtf32(0x274C);
}
