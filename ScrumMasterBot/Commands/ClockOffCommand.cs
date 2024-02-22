using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace ScrumMasterBot.Commands;

public class ClockOffCommand(ILogger<ClockOffCommand> logger) : ModuleBase<SocketCommandContext>
{
    [Command("clockoff", RunMode = RunMode.Async)]
    public async Task Command()
    {
        if (_responseCache.ContainsKey(Context.User.Id))
        {
            await Context.Message.ReplyAsync($"No retries! your original answer was: {_responseCache[Context.User.Id]}");
            return;
        }

        var timestamp = new DateTime(Context.Message.Timestamp.Ticks);

        var clampedHour = Math.Clamp(timestamp.Hour, DayBegin, DayEnd);
        var percentOfDayComplete = MapRange(clampedHour, DayBegin, DayEnd, 0, 1);
        var tweened = EaseInQuart(percentOfDayComplete);

        var choice = _rng.NextDouble();

        logger.LogDebug("{0} {1} {2}", percentOfDayComplete, tweened, choice);

        var message = choice < tweened ? "I think you should clock off" : GetMessage(percentOfDayComplete);

        _responseCache[Context.User.Id] = message;
        
        await Context.Message.ReplyAsync(message);
    }

    static Random _rng = new Random();
    static Dictionary<ulong, string> _responseCache = new Dictionary<ulong, string>();
    const int DayBegin = 9;
    const int DayEnd = 17;

    static string GetMessage(double percentOfDayComplete) => percentOfDayComplete switch 
    {
        >= 0 and < 0.25 => "If you're unwell then log off, but its too early!",
        >= 0.25 and < 0.5 => "It's still morning... you shouldn't log off yet.",
        >= 0.5 and < 0.75 => "Just take your lunch break.",
        >= 0.75 and < 0.825 => "Only a couple more hours, you can tough it out.",
        >= 0.825 and < 1.0 => "So close! Might as well finish the day off!",
        _ => "Why are you working at this hour?"
    };

    static double MapRange(double from, double fromMin, double fromMax, double toMin,  double toMax)
    {
        var fromAbs = from - fromMin;
        var fromMaxAbs = fromMax - fromMin;      
       
        var normal = fromAbs / fromMaxAbs;
 
        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;
 
        var to = toAbs + toMin;
       
        return to;
    }

    static double EaseInQuart(double x) {
        return x * x * x * x;
    }
}
