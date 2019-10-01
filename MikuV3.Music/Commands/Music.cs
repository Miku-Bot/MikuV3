using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
using MikuV3.Music.Entities;
using MikuV3.Music.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
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
            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);
            var su = new ServiceUtil();
            var it = su.GetService(url);
            var psi = new ProcessStartInfo()
            {
                FileName = @"youtube-dl.exe",
                Arguments = $"-i --no-warnings -J --no-playlist \"{url}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var ytdl = Process.Start(psi);
            var json = await ytdl.StandardOutput.ReadToEndAsync();
            string urls = "";
            YTDL.Root got = null;
            if (it.ContentService == Enums.ContentService.Direct)
            {
                urls = url;
            }
            else
            {
                got = JsonConvert.DeserializeObject<YTDL.Root>(json);
                urls = got.formats.OrderByDescending(x => x.abr).First().url;
            }
            var c = new HttpClient();
            c.DefaultRequestHeaders.Add("Referer", url);
            c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:68.0) Gecko/20100101 Firefox/68.0");
            var gott = await c.GetAsync(urls, HttpCompletionOption.ResponseHeadersRead);
            var psi2 = new ProcessStartInfo()
            {
                FileName = @"ffmpeg.exe",
                Arguments = $@"-i - -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            };
            var ffmpeg = Process.Start(psi2);
            var ffout = ffmpeg.StandardOutput.BaseStream;
            var txStream = vnc.GetTransmitStream();
            var inputTask = Task.Run(async () =>
            {
                try
                {
                    if (got?.formats.First().fragments.Length != 0 && got != null)
                    {
                        foreach (var f in got.formats.First().fragments)
                        {
                            gott = await c.GetAsync(got.formats.First().fragment_base_url + f.path, HttpCompletionOption.ResponseHeadersRead);
                            using (var thedata = await gott.Content.ReadAsStreamAsync())
                            {
                                byte[] buffer = new byte[3840];
                                int read;
                                while ((read = thedata.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    ffmpeg.StandardInput.BaseStream.Write(buffer, 0, read);
                                }
                            }
                        }
                    }
                    else
                    {
                        using (var thedata = await gott.Content.ReadAsStreamAsync())
                        {
                            byte[] buffer = new byte[3840];
                            int read;
                            while ((read = thedata.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ffmpeg.StandardInput.BaseStream.Write(buffer, 0, read);
                            }
                        }
                    }
                    ffmpeg.StandardInput.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
            await ffout.CopyToAsync(txStream);
            await txStream.FlushAsync();
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
