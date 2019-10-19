using MikuV3.Music.ServiceManager.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MikuV3.Music.Entities
{
    public class QueueEntry : Entry
    {
        public int Position { get; set; }
        public ulong AddedBy { set; get; }
        public QueueEntry(ServiceResult serviceResult, ulong addedBy, int pos) : base(serviceResult)
        {
            Position = pos;
            AddedBy = addedBy;
        }
    }
}
