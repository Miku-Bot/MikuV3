using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MikuV3.Music.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;

namespace MikuV3.Music.Commands
{
    [RequireOwner]
    public class Debug : BaseCommandModule
    {
        [Command("gs")]
        public async Task GetService(CommandContext ctx, [RemainingText] string n)
        {
            var sw = new Stopwatch();
            var su = new ServiceUtil();
            sw.Start();
            //var it = await su.GetService(n);
            var exp = new YoutubeClient();
            var f = YoutubeClient.ParseVideoId(n);
            sw.Stop();
            await ctx.RespondAsync(Environment.NewLine + sw.Elapsed.TotalSeconds);
        }
    }
}
