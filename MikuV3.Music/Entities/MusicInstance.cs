using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using MikuV3.Music.Enums;
using MikuV3.Music.ServiceManager.Entities;
using MikuV3.Music.ServiceManager.Enums;
using MikuV3.Music.ServiceManager.ServiceExtractors;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
            var su = new ServiceManager.ServiceResolver();
            var gs = su.GetService(songString);
            if (gs.Playlist != Playlist.No) gs.Playlist = Playlist.No;
            var srs = await su.GetServiceResults(songString, gs);
            var sr = srs[0];
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
                if (NextSong?.ServiceResult.Slow == true){
                    NextSong.ServiceResult.StartCaching();
                }
                Playstate = Playstate.Playing;
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
                if (CurrentSong.ServiceResult.PCMQueue == null && CurrentSong.ServiceResult.FillCacheTask == null)
                {
                    CurrentSong.ServiceResult.StartCaching();
                }
                while (CurrentSong.ServiceResult.CacheStatus != CacheStatus.PlayReady && CurrentSong.ServiceResult.Slow) await Task.Delay(100);
                while (CurrentSong.ServiceResult.PCMQueue == null)await Task.Delay(100);
                var currentPCMCache = CurrentSong.ServiceResult.PCMQueue;
                while (CurrentSong.ServiceResult.CacheStatus != CacheStatus.Cached || currentPCMCache.Count > 0)
                {
                    var hasPacket = currentPCMCache.TryPeek(out var none);
                    if (!hasPacket || GetPacketQueueCount() > 50 || Playstate == Playstate.Paused)
                        continue;
                    currentPCMCache.TryDequeue(out var packet);
                    if (!CurrentSong.ServiceResult.CurrentPosition.IsRunning) CurrentSong.ServiceResult.CurrentPosition.Start();
                    await packet.CopyToAsync(tx, 3840);
                    await packet.DisposeAsync();
                    
                }
                CurrentSong.ServiceResult.Dispose();
                CurrentSong.ServiceResult.CurrentPosition.Stop();
                Console.WriteLine(CurrentSong.ServiceResult.Length.TotalSeconds);
                Console.WriteLine(CurrentSong.ServiceResult.CurrentPosition.Elapsed.TotalSeconds +1);
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
