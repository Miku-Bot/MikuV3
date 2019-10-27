using DSharpPlus;
using DSharpPlus.Entities;
using MikuV3.Music.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MikuV3.Music.Extensions
{
    #region Extensions
    public static partial class MusicExtensionMethods
    {
        public static MusicExtension UseMusic(this DiscordClient c)
        {
            if (c.GetExtension<MusicExtension>() != null)
                throw new Exception("Music module is already enabled for this client!");

            var m = new MusicExtension();
            c.AddExtension(m);
            return m;
        }

        public static MusicExtension GetMusic(this DiscordClient c)
        {
            return c.GetExtension<MusicExtension>();
        }
    }
    #endregion

    /// <summary>
    /// Extension class for DSharpPlus
    /// </summary>
    public class MusicExtension : BaseExtension
    {
        ConcurrentDictionary<ulong, MusicInstance> musicInstances { get; set; }

        protected override void Setup(DiscordClient client)
        {
            this.Client = client;
            musicInstances = new ConcurrentDictionary<ulong, MusicInstance>();
        }

        public MusicInstance GetMusicInstance(DiscordGuild guild)
        {
            return musicInstances.GetOrAdd(guild.Id, key => new MusicInstance(guild));
        }
    }
}
