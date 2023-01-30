using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Newtonsoft.Json;
using SlacrafratMarketDiscordBot.Objects;
using System.IO;
using System.IO.Enumeration;

namespace SlacrafratMarketDiscordBot.Commands
{
    public class Market : BaseCommandModule
    {
        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }

        [Command("search")]
        public async Task Search(CommandContext ctx)
        {
            ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }
    }

    public class MarketSlash : ApplicationCommandModule
    {

        [SlashCommand("ping", "Test")]
        public async Task Ping(InteractionContext ctx)
        {
            ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }
        [SlashCommand("search", "Поиск предметов")]
        public async Task Search(InteractionContext ctx, [Option("Имя", "Название предмета")] string item)
        {
            var ItemList = JsonConvert.DeserializeObject<List<Item>>(Properties.Resources.listing);
            foreach (Item i in ItemList)
            {
                if (i.name.Lines.Ru.Contains(item))
                {
                    var path = Directory.GetCurrentDirectory().ToString().Replace("\\", "/") + "/stalcraftdatabase/global" + i.icon;
                    var filename = Path.GetFileName(path);
                    path = path.Replace("/", @"\");

                    
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        var message = new DiscordMessageBuilder()
                        .AddFile(filename, fs);

                        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                        {
                            Title = i.name.Lines.Ru,
                            Description = "Test",
                            ImageUrl = "attachment://" + filename,
                        };

                        message.Embed = embed;
                        
                        await ctx.Channel.SendMessageAsync(message);
                    }
                }
                else if (i.name.Lines.En.Contains(item))
                {
                    var path = Directory.GetCurrentDirectory().ToString().Replace("\\", "/") + "/stalcraftdatabase/global" + i.icon;
                    var filename = Path.GetFileName(path);
                    path = path.Replace("/", @"\");


                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        var message = new DiscordMessageBuilder()
                        .AddFile(filename, fs);

                        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                        {
                            Title = i.name.Lines.En,
                            Description = "Test",
                            ImageUrl = "attachment://" + filename,
                        };

                        message.Embed = embed;

                        await ctx.Channel.SendMessageAsync(message);
                    }
                }
            }
        }
    }
}
