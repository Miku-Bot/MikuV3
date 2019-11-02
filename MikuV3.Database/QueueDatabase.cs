using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using MikuV3.Database.Entities;
using MikuV3.Music.ServiceManager.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MikuV3.Database
{
    public class QueueDBContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
            .UseNpgsql("Host=meek.moe;Database=MikuV3_;Username=;Password=");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QueueEntryInfo>(ctx => {
                ctx.HasKey(key => new { key.Position, key.GuildId });
            });
        }

        public DbSet<QueueEntryInfo> QueueEntries { get; set; }
    }
}
