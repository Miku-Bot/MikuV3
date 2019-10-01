using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using MikuV3.Music.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MikuV3.Music.Entities
{
    public class MusicInstance
    {
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
    }
}
