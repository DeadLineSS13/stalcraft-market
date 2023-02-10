using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using SlacrafratMarketDiscordBot.Commands;
using System.Net.Http;
using SlacrafratMarketDiscordBot.Objects;
using System.Reflection;
using DSharpPlus.SlashCommands;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace SlacrafratMarketDiscordBot
{
    internal class Bot
    {
        public DiscordClient? DClient { get; private set; }
        public HttpClient? HClient { get; private set; }
        public CommandsNextExtension? Commands { get; private set; }
        public SlashCommandsExtension? SlashCommands { get; private set; }

        public async Task RunAsync()
        {
            var configJson = JsonConvert.DeserializeObject<Configuration>(Properties.Resources.config);

            var config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
                
            };

            DClient = new DiscordClient(config);
            HClient = new HttpClient();

            DClient.Ready += OnClientReady;

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] {configJson.Prefix},
                EnableDms = false,
                EnableMentionPrefix = true,
                DmHelp = true,

            };

            Commands = DClient.UseCommandsNext(commandsConfig);

            var slashcommandsConfig = new SlashCommandsConfiguration
            {
            };

            SlashCommands = DClient.UseSlashCommands(slashcommandsConfig);

            SlashCommands.RegisterCommands<MarketSlash>();

            var interactivityConfig = new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromMinutes(1)
            };

            DClient.UseInteractivity(interactivityConfig);

            await DClient.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
