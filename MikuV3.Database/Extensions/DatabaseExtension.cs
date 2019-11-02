using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using MikuV3.Database;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MikuV3.Music.Database.Extensions
{
    #region Extensions
    public static partial class DatabaseExtensionMethods
    {
        public static DatabaseExtension UseDatabase(this DiscordClient c)
        {
            if (c.GetExtension<DatabaseExtension>() != null)
                throw new Exception("Music module is already enabled for this client!");

            var m = new DatabaseExtension();
            c.AddExtension(m);
            return m;
        }

        public static DatabaseExtension GetMusic(this DiscordClient c)
        {
            return c.GetExtension<DatabaseExtension>();
        }
    }
    #endregion

    /// <summary>
    /// Extension class for DSharpPlus
    /// </summary>
    public class DatabaseExtension : BaseExtension
    {
        protected override void Setup(DiscordClient client)
        {
            this.Client = client;
        }

        public QueueDBContext GetQueueContext()
        {
            return new QueueDBContext();
        }
    }

    public static class DirectQueueDbExtensions
    {
        public static QueueDBContext GetQueueDBContext(this CommandContext ctx)
        {
            var ex = ctx.Client.GetExtension<DatabaseExtension>();
            return ex.GetQueueContext();
        }
    }
}
