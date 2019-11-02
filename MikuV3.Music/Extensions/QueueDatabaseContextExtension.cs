using Microsoft.EntityFrameworkCore;
using MikuV3.Database;
using MikuV3.Database.Entities;
using MikuV3.Music.Entities;
using MikuV3.Music.Enums;
using MikuV3.Music.ServiceManager.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuV3.Music.Extensions
{
    public static class QueueDatabaseContextExtension
    {

        public static async Task<List<QueueEntryInfo>> GetGuildQueue(this QueueDBContext ctx, ulong guildId)
        {
            var items = await ctx.QueueEntries.Where(x => x.GuildId == guildId).ToListAsync();
            var ordered = items.OrderBy(x => x.Position).ToList();
            foreach (var item in ordered)
            {
                //Console.WriteLine(item.DBTrackInfoRaw);
                var rawBytes = Convert.FromBase64String(item.DBTrackInfoRaw);
                var rawJson = Encoding.UTF8.GetString(rawBytes);
                item.DBTrackInfo = JsonConvert.DeserializeObject<DBQueueEntryJson>(rawJson);
            }
            return ordered;
        }

        public static async Task<int> GetGuildQueueCount(this QueueDBContext ctx, ulong guildId)
        {
            var items = await ctx.QueueEntries.Where(x => x.GuildId == guildId).CountAsync();
            return items;
        }

        public static async Task<QueueEntryInfo> GetNextSong(this QueueDBContext ctx, ulong guildId, int position)
        {
            var queue = await GetGuildQueue(ctx, guildId);
            return queue[position];
        }

        public static async Task ClearGuildQueue(this QueueDBContext ctx, ulong guildId)
        {
            var them = await GetGuildQueue(ctx, guildId);
            ctx.QueueEntries.RemoveRange(them);
            await ctx.SaveChangesAsync();
        }

        private static async Task ReorderQueue(QueueDBContext ctx, ulong guildId, List<QueueEntryInfo> queueEntries)
        {
            queueEntries.Reverse();
            await ClearGuildQueue(ctx, guildId);
            for (int i = 0; i < queueEntries.Count; i++)
                queueEntries[i].Position = i;
            await ctx.AddRangeAsync(queueEntries);
            await ctx.SaveChangesAsync();
        }

        public static async Task<QueueEntryInfo> AddToGuildQueue(this QueueDBContext ctx, ulong guildId, ulong userId, ServiceResult queueEntry)
        {
            var baseDB = new DBQueueEntryJson(queueEntry);
            var baseDBJson = JsonConvert.SerializeObject(baseDB);
            var baseBytes = Encoding.UTF8.GetBytes(baseDBJson);
            var queueCount = await ctx.QueueEntries.Where(x => x.GuildId == guildId).CountAsync();
            var items = await ctx.QueueEntries.AddAsync(new QueueEntryInfo
            {
                AddedBy = userId,
                AdditionTime = DateTime.Now,
                DBTrackInfoRaw = Convert.ToBase64String(baseBytes),
                GuildId = guildId,
                Position = queueCount
            });
            await ctx.SaveChangesAsync();
            items.Entity.DBTrackInfo = new DBQueueEntryJson(queueEntry);
            return items.Entity;
        }

        public static async Task<QueueEntryInfo> InsertToGuildQueue(this QueueDBContext ctx, ulong guildId, ulong userId, ServiceResult queueEntry, int position)
        {
            var queue = await GetGuildQueue(ctx, guildId);
            var baseDB = new DBQueueEntryJson(queueEntry);
            var baseDBJson = JsonConvert.SerializeObject(baseDB);
            var baseBytes = Encoding.UTF8.GetBytes(baseDBJson);
            var item = new QueueEntryInfo
            {
                AddedBy = userId,
                AdditionTime = DateTime.Now,
                DBTrackInfoRaw = Convert.ToBase64String(baseBytes),
                GuildId = guildId,
                Position = -1
            };
            queue.Insert(position, item);
            await ReorderQueue(ctx, guildId, queue);
            await ctx.SaveChangesAsync();
            item.DBTrackInfo = new DBQueueEntryJson(queueEntry);
            return item;
        }

        public static async Task<QueueEntryInfo> DeleteFromGuildQueue(this QueueDBContext ctx, ulong guildId, int position)
        {
            var queue = await GetGuildQueue(ctx, guildId);
            var deletedEntry = queue[0];
            queue.RemoveAt(0);
            await ReorderQueue(ctx, guildId, queue);
            await ctx.SaveChangesAsync();
            return deletedEntry;
        }

        public static async Task<QueueEntryInfo> MoveFromToGuildQueue(this QueueDBContext ctx, ulong guildId, int oldPosition, int newPosition)
        {
            var queue = await GetGuildQueue(ctx, guildId);
            var temp = queue[newPosition];
            queue[newPosition] = queue[oldPosition];
            queue[oldPosition] = temp;
            await ReorderQueue(ctx, guildId, queue);
            return queue[newPosition];
        }
    }
}
