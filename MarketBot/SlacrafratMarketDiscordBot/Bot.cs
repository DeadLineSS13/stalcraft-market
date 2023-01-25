using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using SlacrafratMarketDiscordBot.Commands;

namespace SlacrafratMarketDiscordBot
{
    internal class Bot
    {
        public DiscordClient? Client { get; private set; }
        public CommandsNextExtension? Commands { get; private set; }
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

            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] {configJson.Prefix},
                EnableDms = false,
                EnableMentionPrefix = true,
                DmHelp = true,

            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<Market>();

            await Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
