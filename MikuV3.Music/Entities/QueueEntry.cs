using System;
using System.Collections.Generic;
using System.Text;

namespace MikuV3.Music.Entities
{
    public class QueueEntry : Entry
    {
        public int position { get; set; }
        public ulong addedBy { set; get; }
        public QueueEntry(ServiceResult sr, ulong ab, int pos) : base(sr)
        {
            additionTime = DateTimeOffset.UtcNow;
            position = pos;
            addedBy = ab;
        }
    }
}
