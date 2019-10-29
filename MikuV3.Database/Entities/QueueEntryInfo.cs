using System;

namespace MikuV3.Database.Entities
{
    public class QueueEntryInfo
    {
        public int Position { get; set; }
        public ulong AddedBy { set; get; }
        public DateTimeOffset AdditionTime { get; set; }
        public DBQueueEntryJson DBTrackInfo { get; set; }
        public QueueEntryInfo(DBQueueEntryJson dbTrackInfo, ulong addedBy, DateTimeOffset additionDate, int position)
        {
            DBTrackInfo = dbTrackInfo;
            AdditionTime = additionDate;
            Position = position;
            AddedBy = addedBy;
        }
    }
}
