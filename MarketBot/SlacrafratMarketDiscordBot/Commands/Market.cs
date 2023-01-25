using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SlacrafratMarketDiscordBot.Commands
{
    public class Market : BaseCommandModule
    {
        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }
    }
}
