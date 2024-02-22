using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ScrumMasterBot.Dailies;

public static class GameParser
{
    public static (Games, string) Parse(string input)
    {
        var result = (Games.None, string.Empty);

        if (string.IsNullOrEmpty(input))
            return result;

        var lines = input.Split('\n').ToList();
        var firstLine = lines.First();

        try
        {
            Match? match = null;
            if ((match = Regexes.WordleBandle.Match(firstLine)).Success)
            {
                var name = match.Groups[1].Value;
                if (Enum.TryParse<Games>(name, true, out var game))
                {
                    var score = game switch
                    {
                        Games.Wordle => ParseWordleScore(lines),
                        Games.Bandle => ParseBandleScore(lines),
                        _ => throw new ArgumentException("wtf")
                    };

                    result = (game, $"{name}: {score}");
                }
            }
            else if ((match = Regexes.GuessTheGame.Match(firstLine)).Success)
            {
                result = (Games.GuessTheGame, ParseGuessTheGameScore(lines));
            }
        }
        catch (Exception)
        { }

        return result;
    }

    static string ParseWordleScore(IReadOnlyCollection<string> lines)
    {
        var sb = new StringBuilder();

        foreach (var line in lines.Skip(2))
        {
            int count = 0;
            foreach (var rune in line.EnumerateRunes())
            {
                if (rune == YellowSquare || rune == GreenSquare)
                {
                    count += 1;
                }
            }

            if (Discord.Emoji.TryParse($":{DigitToString[count]}:", out var emoji))
            {
                sb.Append(emoji);
            }

        }

        return sb.ToString();
    }

    static string ParseBandleScore(IReadOnlyCollection<string> lines)
    {
        return lines.Skip(1).First();
    }

    static string ParseGuessTheGameScore(IReadOnlyCollection<string> lines)
    {
        return lines.Skip(2).First();
    }

    static readonly Rune BlackSquare = new(0x2B1B);
    static readonly Rune WhiteSquare = new(0x2B1C);
    static readonly Rune RedSquare = new(0x1F7E5);
    static readonly Rune YellowSquare = new(0x1F7E8);
    static readonly Rune BlueSquare = new(0x1F7E6);
    static readonly Rune GreenSquare = new(0x1F7E9);
    static readonly Rune PurpleSquare = new(0x1F7EA);
    static ImmutableArray<string> DigitToString = [
        "zero",
        "one",
        "two",
        "three",
        "four",
        "five",
        "six",
        "seven",
        "eight",
        "nine",
    ];
}
