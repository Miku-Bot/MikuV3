using MikuV3.Database.Entities;
using MikuV3.Music.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MikuV3.Music.EventArgs
{
    public class PlaybackErroredEventArgs
    {
        public QueueEntryInfo QueueEntry { get; set; }
        public MusicInstance MusicInstance { get; set; }
        public Exception Exception { get; set; }
    }
}
