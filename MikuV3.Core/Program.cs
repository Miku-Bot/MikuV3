using MikuV3.Core.Entities;
using Newtonsoft.Json;
using System;
using System.IO;

namespace MikuV3.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            var json = File.ReadAllText(@"config.json");
            var config = JsonConvert.DeserializeObject<BotConfig.Root>(json);
            using (var bot = new BotCore(config))
            {
                bot.RunBot().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
    }
}
