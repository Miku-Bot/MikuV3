using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using MikuV3.Music.Enums;
using MikuV3.Music.ServiceExtractors;
using MikuV3.Music.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikuV3.Music.Entities
{
    public class MusicInstance
    {
        public DiscordGuild guild { get; set; }
        public DiscordChannel usedChannel { get; set; }
        public DiscordChannel voiceChannel { get; set; }
        public Playstate playstate { get; set; }
        public RepeatMode repeatMode { get; set; }
        public int repeatAllPos { get; set; }
        public ShuffleMode shuffleMode { get; set; }
        public DateTime aloneTime { get; set; }
        public CancellationTokenSource aloneCTS { get; set; }
        public VoiceNextConnection vnc { get; set; }
        public QueueEntry currentSong { get; set; }
        public QueueEntry nextSong { get; set; }
        public QueueEntry lastSong { get; set; }
        public Task PlayTask { get; set; }
        public List<QueueEntry> tempQueue = new List<QueueEntry>();

        public MusicInstance(DiscordGuild g)
        {
            guild = g;
            usedChannel = null;
            playstate = Playstate.NotPlaying;
            repeatMode = RepeatMode.Off;
            repeatAllPos = 0;
            shuffleMode = ShuffleMode.Off;
        }

        public async Task<bool> ConnectToChannel(DiscordChannel chn)
        {
            if (chn.Type == DSharpPlus.ChannelType.Voice)
            {
                vnc = await Bot._vnext.ConnectAsync(chn);
                return true;
            }
            return false;
        }

        public async Task<ServiceResult> QueueSong(CommandContext ctx, string s, int pos = -1)
        {
            var su = new ServiceUtil();
            var gs = su.GetService(s);
            ServiceResult sr = null;
            switch (gs.ContentService)
            {
                case ContentService.Search: break;
                case ContentService.Direct: break;
                case ContentService.Youtube:
                    {
                        switch (gs.Playlist)
                        {
                            case Playlist.No:
                                {
                                    var s1 = await new YoutubeSingle().GetServiceResult(s);
                                    sr = s1[0];
                                    break;
                                }
                            case Playlist.Yes: break;
                            case Playlist.Only: break;
                        }
                        break;
                    }
                case ContentService.Soundcloud:
                    {
                        switch (gs.Playlist)
                        {
                            case Playlist.No: break;
                            case Playlist.Only: break;
                        }
                        break;
                    }
                case ContentService.NicoNicoDouga:
                    {
                        switch (gs.Playlist)
                        {
                            case Playlist.No: {
                                    var s1 = await new NicoNicoDougaSingle().GetServiceResult(s);
                                    if (tempQueue.Count == 1) s1[0].FillCacheTask = Task.Run(s1[0].FillCache);
                                    sr = s1[0];
                                    break;
                                }
                            case Playlist.Only: break;
                        }
                        break;
                    }
                case ContentService.BiliBili:
                    {
                        switch (gs.Playlist)
                        {
                            case Playlist.No:
                                {
                                    var s1 = await new BilibiliSingle().GetServiceResult(s);
                                    sr = s1[0];
                                    break;
                                }
                            case Playlist.Only:
                                {
                                    break;
                                }
                        }
                        break;
                    }
            }
            if (sr != null)
            {
                Console.WriteLine(sr.Artist + " - " + sr.Title);
                var plainJson = JsonConvert.SerializeObject(new DBQueueEntryJson(sr));
                var plainBytes = Encoding.UTF8.GetBytes(plainJson);
                var encodedJson = Convert.ToBase64String(plainBytes);
                //Database.AddToQueue(ctx.Guild, ctx.Member.Id, encodedJson);
                tempQueue.Add(new QueueEntry(sr, ctx.Member.Id, tempQueue.Count));
                if (tempQueue.Count == 2) nextSong = new QueueEntry(sr, ctx.Member.Id, tempQueue.Count);
                if (vnc.Channel != null && (playstate == Playstate.NotPlaying || playstate == Playstate.Stopped)) await PlaySong(); 
            }
            return sr;
        }

        public async Task<QueueEntry> PlaySong()
        {
            try
            {
                Console.WriteLine("here");
                await Task.Delay(0);
                var queue = tempQueue;
                if (currentSong == null) currentSong = queue[0];
                var cur = lastSong;
                if (queue.Count != 1 && repeatMode == RepeatMode.All)
                    repeatAllPos++;
                if (repeatAllPos >= queue.Count)
                    repeatAllPos = 0;
                if (shuffleMode == ShuffleMode.Off)
                {
                    if (queue.Count > 1)
                        nextSong = queue[1];
                }
                else
                {
                    if (queue.Count > 1)
                        nextSong = queue[new Random().Next(0, queue.Count)];
                }
                if (repeatMode == RepeatMode.All)
                {
                    if (queue.Count > 1)
                        nextSong = queue[repeatAllPos];
                }
                if (repeatMode == RepeatMode.On)
                {
                    if (queue.Count > 1)
                        nextSong = cur;
                }
                playstate = Playstate.Playing;
                Console.WriteLine("Task Time");
                PlayTask = Task.Run(PlayCur);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return currentSong;
        }

        public async Task PlayCur()
        {
            try
            {
                //Console.WriteLine("Getting Stream");
                var tx = vnc.GetTransmitStream();
                //Console.WriteLine(currentSong.ServiceResult.Title);
                if (currentSong.ServiceResult.Slow && currentSong.ServiceResult.CacheStatus == CacheStatus.Rendering)
                {
                    //Console.WriteLine("Yes it slow");
                    await usedChannel.SendMessageAsync("Slow service, please wait a bit while we buffer for smooth playback");
                }
                if (currentSong.ServiceResult.PCMCache == null && currentSong.ServiceResult.FillCacheTask == null)
                {
                    currentSong.ServiceResult.FillCacheTask = Task.Run(currentSong.ServiceResult.FillCache);
                }
                while (currentSong.ServiceResult.Percentage < 65 && currentSong.ServiceResult.Slow) await Task.Delay(100);
                //Console.WriteLine("Hmmmm");
                while (currentSong.ServiceResult.PCMCache == null)await Task.Delay(100);
                //Console.WriteLine("stream exists");
                var sr = currentSong.ServiceResult.PCMCache;
                int read;
                byte[] buffer = new byte[3840];
                //Console.WriteLine("Write time");
                while ((read = sr.Read(buffer, 0, buffer.Length)) > 0)
                {
                    tx.Write(buffer, 0, read);
                }
                await tx.FlushAsync();
                await vnc.WaitForPlaybackFinishAsync();
                tempQueue.RemoveAt(0);
                playstate = Playstate.NotPlaying;
                lastSong = currentSong;
                currentSong = nextSong;
                await PlaySong();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
