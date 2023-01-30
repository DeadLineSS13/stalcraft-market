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

namespace SlacrafratMarketDiscordBot
{
    internal class Bot
    {
        public DiscordClient? DClient { get; private set; }
        public HttpClient? HClient { get; private set; }
        public CommandsNextExtension? Commands { get; private set; }
        public SlashCommandsExtension? SlashCommands { get; private set; }

        public List<Item> items_list { get; set; }

        public async Task RunAsync()
        {
            items_list = JsonConvert.DeserializeObject<List<Item>>(Properties.Resources.listing);
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

            Commands.RegisterCommands<Market>();

            var slashcommandsConfig = new SlashCommandsConfiguration
            {
            };

            SlashCommands = DClient.UseSlashCommands(slashcommandsConfig);

            SlashCommands.RegisterCommands<MarketSlash>();

            await DClient.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
