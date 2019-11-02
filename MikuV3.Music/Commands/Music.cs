using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
using MikuV3.Music.Entities;
using MikuV3.Music.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MikuV3.Music.Commands
{
    public class Music : BaseCommandModule
    {

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

        [Command("status")]
        public async Task Status(CommandContext ctx)
        {
            var ex = ctx.Client.GetExtension<MusicExtension>();
            var g = ex.GetMusicInstance(ctx.Guild);
            await ctx.RespondAsync(Math.Round(g.CurrentSongServiceResult.CurrentPosition.Elapsed.TotalSeconds,2).ToString() + " of " + Math.Round(g.CurrentSongServiceResult.Length.TotalSeconds, 2).ToString());
        }

        [Command("pause")]
        public async Task Pause(CommandContext ctx)
        {
            var _mi = ctx.GetMusicInstance();
            if (_mi.Playstate == Enums.Playstate.Paused)
            {
                _mi.Playstate = Enums.Playstate.Playing;
            }
            else
            {
                _mi.Playstate = Enums.Playstate.Paused;
                await Task.Delay(1000);
                _mi.CurrentSongServiceResult.CurrentPosition.Stop();
            }
            await ctx.RespondAsync(_mi.Playstate.ToString());
        }

        [Command("play"), Aliases("p")]
        [Priority(1)]
        public async Task Play(CommandContext ctx, [RemainingText] string url)
        {
            var g = ctx.GetMusicInstance();
            g.UsedChannel = ctx.Channel;
            var vnext = ctx.Client.GetVoiceNext();
            if (g.Vnc == null) await g.ConnectToChannel(ctx.Member.VoiceState.Channel, vnext);
            var q = await g.QueueSong(ctx, url);
            await ctx.RespondAsync(JsonConvert.SerializeObject(q, Formatting.Indented));
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
