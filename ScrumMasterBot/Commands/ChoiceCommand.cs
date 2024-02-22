using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ScrumMasterBot.Commands;

public class ChoiceCommand() : ModuleBase<SocketCommandContext>
{
    [Command("choice", RunMode = RunMode.Async)]
    public async Task Command(params string[] selection)
    {
        if (selection.Length == 0)
        {
            await Context.Message.ReplyAsync("No items to choose from!");
            return;
        }    

        var index = _rng.NextInt64(selection.Length);

        await Context.Message.ReplyAsync($"I choose: {selection[index]}");
    }

    static Random _rng = new Random();
}
