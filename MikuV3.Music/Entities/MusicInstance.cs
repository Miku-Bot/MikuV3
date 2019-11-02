using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Microsoft.EntityFrameworkCore;
using MikuV3.Database;
using MikuV3.Database.Entities;
using MikuV3.Music.Enums;
using MikuV3.Music.EventArgs;
using MikuV3.Music.Extensions;
using MikuV3.Music.ServiceManager;
using MikuV3.Music.ServiceManager.Entities;
using MikuV3.Music.ServiceManager.Enums;
using MikuV3.Music.ServiceManager.ServiceExtractors;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikuV3.Music.Entities
{
    public class MusicInstance
    {
        public delegate Task playbackFinished(PlaybackFinishedEventArgs e);
        public event playbackFinished PlaybackFinished;

        public delegate Task playbackErrored(PlaybackErroredEventArgs e);
        public event playbackErrored PlaybackErrored;

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

        public QueueEntryInfo CurrentSong { get; set; }
        public ServiceResult CurrentSongServiceResult { get; set; }

        public QueueEntryInfo NextSong { get; set; }
        public ServiceResult NextSongServiceResult { get; set; }

        public QueueEntryInfo LastSong { get; set; }

        public Task PlaybackTask { get; set; }

        public QueueDBContext DbContext { get; set; }

        public MusicInstance(DiscordGuild guild)
        {
            Guild = guild;
            UsedChannel = null;
            Playstate = Playstate.NotPlaying;
            RepeatMode = RepeatMode.Off;
            RepeatAllPos = 0;
            ShuffleMode = ShuffleMode.Off;
            DbContext = new QueueDBContext();
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

        public async Task<ServiceResult> QueueSong(CommandContext ctx, string songString, int pos = -1)
        {
            var serviceResolver = new ServiceManager.ServiceResolver();
            var songService = serviceResolver.GetService(songString);
            if (songService.Playlist != Playlist.No) songService.Playlist = Playlist.No;
            var ServiceResults = await serviceResolver.GetServiceResults(songString, songService);
            if (ServiceResults != null)
            {
                var oldCount = await DbContext.GetGuildQueueCount(Guild.Id);
                if (pos == -1)
                {
                    ServiceResults.Reverse();
                    foreach (var result in ServiceResults)
                    {
                        await DbContext.AddToGuildQueue(Guild.Id, ctx.Member.Id, result);
                    }
                }
                else
                {
                    ServiceResults.Reverse();
                    foreach (var result in ServiceResults)
                    {
                        await DbContext.InsertToGuildQueue(Guild.Id, ctx.Member.Id, result, pos);
                    }
                }
                if (oldCount == 1)
                {
                    NextSong = new QueueEntryInfo(new DBQueueEntryJson(ServiceResults[0]), ctx.Member.Id, Guild.Id, DateTimeOffset.UtcNow, 1);
                    NextSongServiceResult = ServiceResults[0];
                }

                if (Vnc.Channel != null 
                    && (Playstate == Playstate.NotPlaying 
                    || Playstate == Playstate.Stopped)) await PreparePlayback(); 
            }
            return null;
        }

        public async Task<(QueueEntryInfo, ServiceResult)> GetNextSong()
        {
            var queue = await DbContext.GetGuildQueue(Guild.Id);
            int nextSong = 0;
            if (queue.Count != 1 && RepeatMode == RepeatMode.All)
                RepeatAllPos++;
            if (RepeatAllPos >= queue.Count)
                RepeatAllPos = 0;
            if (ShuffleMode == ShuffleMode.Off)
                nextSong = queue[0].Position;
            else
                nextSong = queue[new Random().Next(0, queue.Count)].Position;
            if (RepeatMode == RepeatMode.All)
                nextSong = queue[RepeatAllPos].Position;
            if (RepeatMode == RepeatMode.On)
                nextSong = CurrentSong.Position;
            var serviceManager = new ServiceResolver();
            var result = await serviceManager.GetServiceResults(queue[nextSong].DBTrackInfo.Url, 
                new ContentServiceMatch(queue[nextSong].DBTrackInfo.ContentService,
                Playlist.No));
            return (queue[nextSong], result[0]);
        }

        public async Task<QueueEntryInfo> PreparePlayback()
        {
            try
            {
                if (CurrentSong == null)
                {
                    var first = await GetNextSong();
                    CurrentSong = first.Item1;
                    CurrentSongServiceResult = first.Item2;
                }
                var next = await GetNextSong();
                if (NextSong == null && await DbContext.GetGuildQueueCount(Guild.Id) > 1)
                {
                    NextSong = next.Item1;
                    NextSongServiceResult = next.Item2;
                    if (NextSongServiceResult.Slow) NextSongServiceResult.StartCaching();
                }
                Playstate = Playstate.Playing;
                PlaybackTask = Task.Run(PlayCurrentSong);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return CurrentSong;
        }

        public async Task PlayCurrentSong()
        {
            try
            {
                Console.WriteLine(CurrentSong.Position);
                Console.WriteLine(CurrentSong.DBTrackInfo.Title);
                Console.WriteLine(CurrentSongServiceResult.Title);

                var tx = Vnc.GetTransmitStream();
                //If the next songs is from a slow Service, it sends an alert that the buffering isnt ready yet
                if (CurrentSongServiceResult.Slow 
                    && CurrentSongServiceResult.CacheStatus == CacheStatus.Rendering)
                {
                    await UsedChannel.SendMessageAsync("Slow service, please wait a bit while we buffer for smooth playback");
                }

                //Start caching if it wasnt already
                if (CurrentSongServiceResult.PCMQueue == null 
                    && CurrentSongServiceResult.FillCacheTask == null)
                {
                    CurrentSongServiceResult.StartCaching();
                }

                //Wait until the the slow Serice is ready to Play (PlayReady wau)
                while (CurrentSongServiceResult.CacheStatus != CacheStatus.PlayReady 
                    && CurrentSongServiceResult.Slow) await Task.Delay(100);

                //More of a non slow thing, wait until there is actually something cached
                while (CurrentSongServiceResult.PCMQueue == null)
                {
                    await Task.Delay(100);
                }

                var currentPCMCache = CurrentSongServiceResult.PCMQueue;

                //The main fun
                while (CurrentSongServiceResult.CacheStatus != CacheStatus.Cached 
                    || currentPCMCache.Count > 0)
                {
                    //See if there is a packet ready (just see, not take)
                    var hasPacket = currentPCMCache.TryPeek(out var none);

                    //If its paused OR THE PACKET QUEUE INTERNALLY OF DSHARPPLUS or there is no packet ready, we skip a cycle
                    
                    if (!hasPacket 
                        || GetPacketQueueCount() > 50 
                        || Playstate == Playstate.Paused)
                    {
                        await Task.Delay(3);
                        continue;
                    }

                    //actually take the first packet now
                    currentPCMCache.TryDequeue(out var packet);

                    //This is to see how far we have advanced into the song yet
                    if (!CurrentSongServiceResult.CurrentPosition.IsRunning)
                    {
                        CurrentSongServiceResult.CurrentPosition.Start();
                    }

                    //Write to the VoiceStream, try/catch cause sometimes it can oof, then its better to skip a bit than fail as a whole
                    try
                    {
                        await packet.CopyToAsync(tx, 3840);
                    }
                    catch { Console.WriteLine("wau"); }
                    await packet.DisposeAsync();
                    
                }
                //Get rid of stuff and pause
                CurrentSongServiceResult.Dispose();
                CurrentSongServiceResult.CurrentPosition.Stop();
                //Clear everything
               
                await tx.FlushAsync();
                await Vnc.WaitForPlaybackFinishAsync();
                //Logic here to not delete the first song but the one that was played, this might be not the right way

                await DbContext.DeleteFromGuildQueue(Guild.Id, CurrentSong.Position);

                Playstate = Playstate.NotPlaying;
                LastSong = CurrentSong;
                CurrentSong = NextSong;
                CurrentSongServiceResult = NextSongServiceResult;

                //If there#s still songs in queue, start the playing process again
                if (await DbContext.GetGuildQueueCount(Guild.Id) != 0)
                {
                    await PreparePlayback();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public int GetPacketQueueCount()
        {
            try
            {
                var pq = Vnc.GetType().GetField("<PacketQueue>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Vnc);
                ICollection list = pq as ICollection;
                return list.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 10;
            }
        }
    }
}
