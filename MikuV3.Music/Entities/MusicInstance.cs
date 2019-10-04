using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using MikuV3.Music.Enums;
using MikuV3.Music.Utilities;
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
        public QueueEntry lastSong { get; set; }

        public MusicInstance()
        {
            usedChannel = null;
            playstate = Playstate.NotPlaying;
            repeatMode = RepeatMode.Off;
            repeatAllPos = 0;
            shuffleMode = ShuffleMode.Off;
        }

        public async Task ConnectToChannel(DiscordChannel chn)
        {
            vnc = await Bot._vnext.ConnectAsync(chn);
        }

        public async Task<ServiceResult> GetSong(CommandContext ctx, string s)
        {
            var su = new ServiceUtil();
            var sr = su.GetService(s);
            switch (sr.ContentService)
            {
                case ContentService.Search: break;
                case ContentService.Direct: break;
                case ContentService.Youtube:
                    {
                        switch (sr.Playlist)
                        {
                            case Playlist.No: break;
                            case Playlist.Yes: break;
                            case Playlist.Only: break;
                        }
                        break;
                    }
            }
            return null;
        }
    }
}
