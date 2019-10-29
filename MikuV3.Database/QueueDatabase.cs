using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using MikuV3.Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MikuV3.Database
{
    public class QueueDataBaseContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("Host=mee.moe;Database=MikuV3_;Username=;Password=");
    }

    public class QueueDatabase
    {
        public async Task<List<QueueEntryInfo>> GetQueue(DiscordGuild guild)
        {

            return null;
        }
    }
}
