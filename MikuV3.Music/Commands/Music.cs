using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
using MikuV3.Music.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MikuV3.Music.Commands
{
    public class Music : BaseCommandModule
    {

        //This and the constructor has to be here to make use of the dependency injection (can be left out if not needed)
        Dictionary<ulong, MusicInstance> _mi { get; }

        public Music(Dictionary<ulong, MusicInstance> mi)
        {
            _mi = mi;
        }

        [Command("join")]
        public async Task Join(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);
            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
                throw new InvalidOperationException("You need to be in a voice channel.");

            vnc = await chn.ConnectAsync();
        }

        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);
            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
                throw new InvalidOperationException("You need to be in a voice channel.");

            vnc.Disconnect();
            await ctx.RespondAsync("dc");
        }

        [Command("stop")]
        public async Task Stop(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);
            vnc.Stop();
            await ctx.RespondAsync("stopped");
        }

        [Command("play")]
        [Priority(1)]
        public async Task Play(CommandContext ctx, [RemainingText] string url)
        {
            if (!_mi.Any(x => x.Key == ctx.Guild.Id))
            {
                _mi.Add(ctx.Guild.Id, new MusicInstance(ctx.Guild));
            }
            var g = _mi[ctx.Guild.Id];
            g.UsedChannel = ctx.Channel;
            var vnext = ctx.Client.GetVoiceNext();
            if (g.Vnc == null) await g.ConnectToChannel(ctx.Member.VoiceState.Channel, vnext);
            var q = await g.QueueSong(ctx, url);
            /*
            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);
            var su = new ServiceUtil();
            var it = su.GetService(url);
            var yes = await new YoutubeSingle().GetServiceResult(url);
            var txStream = vnc.GetTransmitStream();
            if (yes.Slow) await ctx.RespondAsync("Slow service, please wait while its being rendered");
            while (yes.Slow && yes.Percentage < 55) await Task.Delay(100);
            while (yes.PCMCache == null) await Task.Delay(100);
            var srgot = yes.PCMCache;
            await srgot.CopyToAsync(txStream);
            await txStream.FlushAsync();*/
        }

        [Command("play")]
        [Priority(0)]
        public async Task Play(CommandContext ctx)
        {
            await ctx.RespondAsync("no");
            //throw new NotImplementedException(ctx.Command.Name);
        }
    }
}
