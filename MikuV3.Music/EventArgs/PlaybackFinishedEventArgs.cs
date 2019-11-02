using MikuV3.Database.Entities;
using MikuV3.Music.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MikuV3.Music.EventArgs
{
    public class PlaybackFinishedEventArgs
    {
        public QueueEntryInfo QueueEntry { get; set; }
        public MusicInstance MusicInstance { get; set; }
    }
}
