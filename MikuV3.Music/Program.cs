using MikuV3.Music.Entities;
using Newtonsoft.Json;
using System;
using System.IO;

namespace MikuV3.Music
{
    class Program
    {
        static void Main(string[] args)
        {
            var json = File.ReadAllText(@"config.json");
            var conf = JsonConvert.DeserializeObject<BotConfig.Root>(json);
            using (var bot = new Bot(conf))
            {
                bot.RunBot().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
    }
}
