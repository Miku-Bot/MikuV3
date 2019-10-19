using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MikuV3.Music.Entities;
using MikuV3.Music.ServiceManager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MikuV3.Music.Commands
{
    [RequireOwner]
    public class Debug : BaseCommandModule
    {

        //This and the constructor has to be here to make use of the dependency injection (can be left out if not needed)
        public ServiceResolver ServiceResolver { get; }
        public Dictionary<ulong, MusicInstance> Instances { get; set; }

        public Debug(ServiceResolver serviceResolver, Dictionary<ulong, MusicInstance> instances)
        {
            ServiceResolver = serviceResolver;
            Instances = instances;
        }

        [Command("gs")]
        public async Task GetService(CommandContext ctx, [RemainingText] string n)
        {
            var sw = new Stopwatch();
            sw.Start();
            var it = ServiceResolver.GetService(n);
            sw.Stop();
            await ctx.RespondAsync(it.ContentService + " " + it.Playlist + Environment.NewLine + sw.Elapsed.TotalSeconds);
        }

        [Command("ls")]
        public async Task ListTest(CommandContext ctx, string s)
        {
            Instances.Add(ctx.Guild.Id, null);
            await ctx.RespondAsync(JsonConvert.SerializeObject(Instances, Formatting.Indented));
        }
    }
}
