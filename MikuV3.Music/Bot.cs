using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.VoiceNext;
using MikuV3.Music.Entities;
using MikuV3.Music.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MikuV3.Music
{
    public class Bot : IDisposable
    {
        public static BotConfig.Root config { get; set; }
        public static DiscordClient _c { get; set; }
        public static CommandsNextExtension _cnext { get; set; }
        public static InteractivityExtension _inext { get; set; }
        public static VoiceNextExtension _vnext { get; set; }
        public static Dictionary<ulong, MusicInstance> _mi { get; set; }

        public Bot(BotConfig.Root conf)
        {
            config = conf;
            _c = new DiscordClient(new DiscordConfiguration {
                Token = config.DiscordToken,
                TokenType = TokenType.Bot,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true,
                ShardCount = 3,
                ShardId = 0
            });
            _cnext = _c.UseCommandsNext(new CommandsNextConfiguration {
                StringPrefixes = new[] {"mm%"},
                EnableDefaultHelp = false
            });
            _inext = _c.UseInteractivity(new InteractivityConfiguration {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                PaginationDeletion = PaginationDeletion.DeleteEmojis,
                PollBehaviour = PollBehaviour.DeleteEmojis,
                Timeout = TimeSpan.FromMinutes(2)
            });
            _vnext = _c.UseVoiceNext(new VoiceNextConfiguration());
            _c.ClientErrored += e =>
            {
                Console.WriteLine(e);
                return Task.CompletedTask;
            };
            _c.SocketErrored += e =>
            {
                Console.WriteLine(e);
                return Task.CompletedTask;
            };
            _c.UnknownEvent += e =>
            {
                Console.WriteLine(e);
                return Task.CompletedTask;
            };
            _cnext.CommandErrored += e =>
            {
                Console.WriteLine(e.Exception);
                return Task.CompletedTask;
            };
            _cnext.RegisterCommands<Commands.Music>();
            _cnext.RegisterCommands<Commands.Debug>();
        }

        public async Task RunBot()
        {
            await _c.ConnectAsync();
            await Task.Delay(-1);
        } 

        public void Dispose()
        {
            _c = null;
        }
    }
}
