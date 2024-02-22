using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Discord;

namespace ScrumMasterBot.Dailies;

using ParseResult = (Games, string);

public static class ParserUtils
{
    public static IResult<ParseResult> Parse(string input)
    {
        if (string.IsNullOrEmpty(input))
            return new Error<ParseResult>("Input is empty");

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
                        var score = GetParser(game).Parse([.. lines]);
                        if (score is IError err)
                        {
                            return new Error<ParseResult>(err.Reason);
                        }
                        else
                        {
                            return new Result<ParseResult>((game, $"{name}: {score}"));
                        }
                    }
                }
            }
        }
        catch (Exception)
        { }

        return new Error<ParseResult>("No match found");
    }

    static readonly Regex WordleBandleRegex = new Regex(@"(Wordle|Bandle) #?[0-9]+ ([0-6]\/[0-6])", RegexOptions.Compiled);
    static readonly Regex GuessTheGameRegex = new Regex(@"#(GuessTheGame) #[0-9]+", RegexOptions.Compiled);
    static readonly Regex ConnectionsRegex = new Regex(@"(Connections)\s?\nPuzzle\s#[0-9]+", RegexOptions.Compiled | RegexOptions.Multiline);

    static ImmutableArray<Regex> Reggies = [
        WordleBandleRegex,
        GuessTheGameRegex,
        ConnectionsRegex,
    ];

    static IGameParser GetParser(Games game)
    {
        return game switch
        {
            Games.Wordle => new WordleParser(),
            Games.Bandle => new BandleParser(),
            Games.GuessTheGame => new GuessTheGameParser(),
            Games.Connections => new ConnectionsParser(),
            _ => throw new ArgumentException("wtf"),
        };
    }

    static readonly Rune YellowSquare = new(0x1F7E8);
    static readonly Rune GreenSquare = new(0x1F7E9);
    static readonly string X = char.ConvertFromUtf32(0x274C);

    interface IGameParser
    {
        IResult<string> Parse(ImmutableArray<string> lines);
    }

    class WordleParser : IGameParser
    {
        public IResult<string> Parse(ImmutableArray<string> lines)
        {
            var sb = new StringBuilder();

            if (lines.Count() <= 2 || lines.Count() > 8)
            {
                return new Error<string>("Invalid input");
            }

            foreach (var line in lines.Skip(2))
            {
                int count = 0;
                foreach (var square in line.EnumerateRunes())
                {
                    if (square == YellowSquare || square == GreenSquare)
                    {
                        count += 1;
                    }
                }

                sb.Append(Regex.Unescape($"{count}\\uFE0F\\u20E3"));
            }

            return new Result<string>(sb.ToString());
        }
    }

    class BandleParser : IGameParser
    {
        public IResult<string> Parse(ImmutableArray<string> lines)
        {
            if (lines.Count() != 2)
            {
                return new Error<string>("Invalid input");
            }

            return new Result<string>(lines.Skip(1).First());
        }
    }

    class GuessTheGameParser : IGameParser
    {
        public IResult<string> Parse(ImmutableArray<string> lines)
        {
            if (lines.Count() != 3)
            {
                return new Error<string>("Invalid input");
            }

            return new Result<string>(lines.Skip(2).First());
        }
    }

    class ConnectionsParser : IGameParser
    {
        public IResult<string> Parse(ImmutableArray<string> lines)
        {
            if (lines.Count() <= 2 || lines.Count() > 9)
            {
                return new Error<string>("Invalid input");
            }

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

            return new Result<string>(sb.ToString());
        }
    }
}


