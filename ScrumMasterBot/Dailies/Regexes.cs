using System.Text.RegularExpressions;

namespace ScrumMasterBot.Dailies;

public static class Regexes
{
    public static Regex WordleBandle => new Regex(@"(Wordle|Bandle) #?[0-9]+ ([0-6]\/[0-6])", RegexOptions.Compiled);
    public static Regex GuessTheGame => new Regex(@"#GuessTheGame #[0-9]+", RegexOptions.Compiled);
}
