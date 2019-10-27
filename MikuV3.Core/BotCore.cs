using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using MikuV3.Core.Entities;
using MikuV3.Music.Extensions;
using MikuV3.Music.ServiceManager;
using System;
using System.Threading.Tasks;

namespace MikuV3.Core
{
    public sealed class BotCore : IDisposable
    {
        public static BotConfig.Root Config { get; private set; }
        static DiscordClient _c { get; set; }
        static CommandsNextExtension _cnext { get; set; }
        static InteractivityExtension _inext { get; set; }
        static VoiceNextExtension _vnext { get; set; }
        static MusicExtension _me { get; set; }

        public BotCore(BotConfig.Root botConfig)
        {
            Config = botConfig;
            Console.WriteLine(Config.DatabaseConfig.Hostname);
            _c = new DiscordClient(new DiscordConfiguration
            {
                LogLevel = LogLevel.Debug,
                ShardCount = 1,
                ShardId = 0,
                Token = "",
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true
            });

            //Singleton will be the same in every commandclass
            //For the others it dosent, use Transient for DB stuff in the future
            var serviceProvider = new ServiceCollection()
            .AddTransient<ServiceResolver>()
            .BuildServiceProvider();

            _cnext = _c.UseCommandsNext(new CommandsNextConfiguration
            {
                EnableDefaultHelp = false,
                Services = serviceProvider,
                StringPrefixes = new[] { "b!" }
            });
            _cnext.RegisterCommands<Music.Commands.Debug>();
            _cnext.RegisterCommands<Music.Commands.Music>();

            _vnext = _c.UseVoiceNext();
            _me = _c.UseMusic();

            _cnext.CommandErrored += e =>
            {
                Console.WriteLine(e.Exception);
                return Task.CompletedTask;
            };
        }

        public async Task RunBot()
        {
            await _c.ConnectAsync();
            await Task.Delay(-1);
        }

        public void Dispose()
        {
            _c.Dispose();
        }
    }
}
