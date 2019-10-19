using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using MikuV3.Music.Enums;
using MikuV3.Music.ServiceExtractors;
using MikuV3.Music.ServiceManager.Entities;
using MikuV3.Music.ServiceManager.Enums;
using MikuV3.Music.ServiceManager.ServiceExtractors;
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
        public DiscordGuild Guild { get; set; }
        public DiscordChannel UsedChannel { get; set; }
        public DiscordChannel VoiceChannel { get; set; }
        public Playstate Playstate { get; set; }
        public RepeatMode RepeatMode { get; set; }
        public int RepeatAllPos { get; set; }
        public ShuffleMode ShuffleMode { get; set; }
        public DateTime AloneTime { get; set; }
        public CancellationTokenSource AloneCTS { get; set; }
        public VoiceNextConnection Vnc { get; set; }
        public QueueEntry CurrentSong { get; set; }
        public QueueEntry NextSong { get; set; }
        public QueueEntry LastSong { get; set; }
        public Task PlayTask { get; set; }
        public List<QueueEntry> TempQueue = new List<QueueEntry>();

        public MusicInstance(DiscordGuild guild)
        {
            Guild = guild;
            UsedChannel = null;
            Playstate = Playstate.NotPlaying;
            RepeatMode = RepeatMode.Off;
            RepeatAllPos = 0;
            ShuffleMode = ShuffleMode.Off;
        }

        /// <summary>
        /// Connect to a Voicechannel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="voiceNext"></param>
        /// <returns>true when successfuly connected, false if not</returns>
        public async Task<bool> ConnectToChannel(DiscordChannel channel, VoiceNextExtension voiceNext)
        {
            if (channel.Type == DSharpPlus.ChannelType.Voice)
            {
                Vnc = await voiceNext.ConnectAsync(channel);
                return true;
            }
            return false;
        }

        //This will move to the Servicemanager library soon
        public async Task<ServiceResult> QueueSong(CommandContext ctx, string songString, int pos = -1)
        {
            var su = new ServiceManager.ServiceResolver(BotCore.Config.NicoNicoDougaConfig);
            var gs = su.GetService(songString);
            ServiceResult sr = null;
            switch (gs.ContentService)
            {
                case ContentService.Search: break;
                case ContentService.Direct:
                    {
                        var s1 = await new Generic().GetServiceResult(songString);
                        sr = s1[0];
                        break;
                    }
                case ContentService.Youtube:
                    {
                        switch (gs.Playlist)
                        {
                            case Playlist.No:
                                {
                                    var s1 = await new YoutubeSingle().GetServiceResult(songString);
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
                                    var s1 = await new NicoNicoDougaSingle().GetServiceResult(songString);
                                    if (TempQueue.Count == 1) s1[0].FillCacheTask = Task.Run(s1[0].FillCache);
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
                                    var s1 = await new BilibiliSingle().GetServiceResult(songString);
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
                TempQueue.Add(new QueueEntry(sr, ctx.Member.Id, TempQueue.Count));
                if (TempQueue.Count == 2) NextSong = new QueueEntry(sr, ctx.Member.Id, TempQueue.Count);
                if (Vnc.Channel != null && (Playstate == Playstate.NotPlaying || Playstate == Playstate.Stopped)) await PlaySong(); 
            }
            return sr;
        }

        public async Task<QueueEntry> PlaySong()
        {
            try
            {
                Console.WriteLine("here");
                await Task.Delay(0);
                var queue = TempQueue;
                CurrentSong = queue[0];
                var cur = LastSong;
                if (queue.Count != 1 && RepeatMode == RepeatMode.All)
                    RepeatAllPos++;
                if (RepeatAllPos >= queue.Count)
                    RepeatAllPos = 0;
                if (ShuffleMode == ShuffleMode.Off)
                {
                    if (queue.Count > 1)
                        NextSong = queue[1];
                }
                else
                {
                    if (queue.Count > 1)
                        NextSong = queue[new Random().Next(0, queue.Count)];
                }
                if (RepeatMode == RepeatMode.All)
                {
                    if (queue.Count > 1)
                        NextSong = queue[RepeatAllPos];
                }
                if (RepeatMode == RepeatMode.On)
                {
                    if (queue.Count > 1)
                        NextSong = cur;
                }
                Playstate = Playstate.Playing;
                Console.WriteLine("Task Time");
                PlayTask = Task.Run(PlayCur);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return CurrentSong;
        }

        public async Task PlayCur()
        {
            try
            {
                var tx = Vnc.GetTransmitStream();
                if (CurrentSong.ServiceResult.Slow && CurrentSong.ServiceResult.CacheStatus == CacheStatus.Rendering)
                {
                    await UsedChannel.SendMessageAsync("Slow service, please wait a bit while we buffer for smooth playback");
                }
                if (CurrentSong.ServiceResult.PCMCache == null && CurrentSong.ServiceResult.FillCacheTask == null)
                {
                    CurrentSong.ServiceResult.FillCacheTask = Task.Run(CurrentSong.ServiceResult.FillCache);
                }
                while (CurrentSong.ServiceResult.CacheStatus != CacheStatus.PlayReady && CurrentSong.ServiceResult.Slow) await Task.Delay(100);
                while (CurrentSong.ServiceResult.PCMCache == null)await Task.Delay(100);
                var currentPCMCachr = CurrentSong.ServiceResult.PCMCache;
                int read;
                byte[] buffer = new byte[3840];
                while ((read = currentPCMCachr.Read(buffer, 0, buffer.Length)) > 0)
                {
                    tx.Write(buffer, 0, read);
                }
                await tx.FlushAsync();
                await Vnc.WaitForPlaybackFinishAsync();
                TempQueue.RemoveAt(0);
                Playstate = Playstate.NotPlaying;
                LastSong = CurrentSong;
                CurrentSong = NextSong;
                if (TempQueue.Count != 0)await PlaySong();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
