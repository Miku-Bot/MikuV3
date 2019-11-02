using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MikuV3.Database.Entities
{
    [Table("QueueEntrys")]
    public class QueueEntryInfo
    {
        [Key]
        public int Position { get; set; }
        [Key]
        public ulong GuildId { get; set; }

        [Required]
        public ulong AddedBy { set; get; }
        [Required]
        public DateTimeOffset AdditionTime { get; set; }
        [Required]
        public string DBTrackInfoRaw { get; set; }

        [NotMapped]
        public DBQueueEntryJson DBTrackInfo { get; set; }

        public QueueEntryInfo()
        {
        }

        public QueueEntryInfo(string dbTrackInfo, ulong addedBy, ulong guild, DateTimeOffset additionDate, int position)
        {
            DBTrackInfo = JsonConvert.DeserializeObject<DBQueueEntryJson>(Encoding.UTF8.GetString(Convert.FromBase64String(dbTrackInfo)));
            AdditionTime = additionDate;
            GuildId = guild;
            Position = position;
            AddedBy = addedBy;
        }

        public QueueEntryInfo(DBQueueEntryJson dbTrackInfo, ulong addedBy, ulong guild, DateTimeOffset additionDate, int position)
        {
            DBTrackInfo = dbTrackInfo;
            AdditionTime = additionDate;
            GuildId = guild;
            Position = position;
            AddedBy = addedBy;
        }
    }
}
