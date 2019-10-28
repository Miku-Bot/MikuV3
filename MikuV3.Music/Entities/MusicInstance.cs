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
                if (ShuffleMode == ShuffleMode.Off && queue.Count > 1)
                    NextSong = queue[1];
                else
                    if (queue.Count > 1)
                    NextSong = queue[new Random().Next(0, queue.Count)];
                if (RepeatMode == RepeatMode.All && queue.Count > 1)
                {
                    NextSong = queue[RepeatAllPos];
                }
                if (RepeatMode == RepeatMode.On && queue.Count > 1)
                {
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
                //If the next songs is from a slow Service, it sends an alert that the buffering isnt ready yet
                if (CurrentSong.ServiceResult.Slow && CurrentSong.ServiceResult.CacheStatus == CacheStatus.Rendering)
                {
                    await UsedChannel.SendMessageAsync("Slow service, please wait a bit while we buffer for smooth playback");
                }
                //Start caching if it wasnt already
                if (CurrentSong.ServiceResult.PCMQueue == null && CurrentSong.ServiceResult.FillCacheTask == null)
                {
                    CurrentSong.ServiceResult.StartCaching();
                }
                //Wait until the the slow Serice is ready to Play (PlayReady wau)
                while (CurrentSong.ServiceResult.CacheStatus != CacheStatus.PlayReady && CurrentSong.ServiceResult.Slow) await Task.Delay(100);
                //More of a non slow thing, wait until there is actually something cached
                while (CurrentSong.ServiceResult.PCMQueue == null)await Task.Delay(100);
                var currentPCMCache = CurrentSong.ServiceResult.PCMQueue;
                //The main fun
                while (CurrentSong.ServiceResult.CacheStatus != CacheStatus.Cached || currentPCMCache.Count > 0)
                {
                    //See if there is a packet ready (just see, not take)
                    var hasPacket = currentPCMCache.TryPeek(out var none);
                    //If its paused OR THE PACKET QUEUE INTERNALLY OF DSHARPPLUS or there is no packet ready, we skip a cycle
                    if (!hasPacket || GetPacketQueueCount() > 50 || Playstate == Playstate.Paused)
                        continue;
                    //actually take the first packet now
                    currentPCMCache.TryDequeue(out var packet);
                    //This is to see how far we have advanced into the song yet
                    if (!CurrentSong.ServiceResult.CurrentPosition.IsRunning) CurrentSong.ServiceResult.CurrentPosition.Start();
                    //Write to the VoiceStream, try/catch cause sometimes it can oof, then its better to skip a bit than fail as a whole
                    try
                    {
                        await packet.CopyToAsync(tx, 3840);
                    }
                    catch { Console.WriteLine("wau"); }
                    await packet.DisposeAsync();
                    
                }
                //Get rid of stuff and pause
                CurrentSong.ServiceResult.Dispose();
                CurrentSong.ServiceResult.CurrentPosition.Stop();
                //Clear everything
                await tx.FlushAsync();
                await Vnc.WaitForPlaybackFinishAsync();
                //Logic here to not delete the first song but the one that was played, this might be not the right way
                TempQueue.Remove(CurrentSong);
                Playstate = Playstate.NotPlaying;
                LastSong = CurrentSong;
                CurrentSong = NextSong;
                //If there#s still songs in queue, start the playing process again
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
