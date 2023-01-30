using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Newtonsoft.Json;
using SlacrafratMarketDiscordBot.Objects;
using System.Drawing;
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
                    var pathData = Directory.GetCurrentDirectory().ToString().Replace("\\", "/") + "/stalcraftdatabase/global" + i.data;
                    var filename = Path.GetFileName(path);
                    var objectData = new ItemInfo();
                    path = path.Replace("/", @"\");
                    pathData = pathData.Replace("/", @"\");

                    using (StreamReader sr = new StreamReader(pathData))
                    {
                        objectData = JsonConvert.DeserializeObject<ItemInfo>(sr.ReadToEnd(), ItemInfo.Converter.Settings);
                    }

                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        var message = new DiscordMessageBuilder()
                        .AddFile(filename, fs);

                        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                        {
                            Title = i.name.Lines.Ru,
                            Description = "",
                            ImageUrl = "attachment://" + filename,
                        };

                        foreach(ItemInfo.InfoBlock e in objectData.InfoBlocks)
                        {
                            if(e.Text != null && e.Text.Key != null && e.Text.Key.Contains("description"))
                            {
                                embed.Description = e.Text.Lines.Ru;
                            }
                        }
                        switch(objectData.Color)
                        {
                            case "RANK_MASTER":
                                embed.Color = DiscordColor.Red;
                                break;
                            case "RANK_VETERAN":
                                embed.Color = DiscordColor.Purple;
                                break;
                            case "RANK_STALKER":
                                embed.Color = DiscordColor.Azure;
                                break;
                            case "RANK_NEWBIE":
                                embed.Color = DiscordColor.Green;
                                break;
                        }

                        message.Embed = embed;
                        
                        await ctx.Channel.SendMessageAsync(message);
                    }
                }
                else if (i.name.Lines.En.Contains(item))
                {
                    var path = Directory.GetCurrentDirectory().ToString().Replace("\\", "/") + "/stalcraftdatabase/global" + i.icon;
                    var pathData = Directory.GetCurrentDirectory().ToString().Replace("\\", "/") + "/stalcraftdatabase/global" + i.data;
                    var filename = Path.GetFileName(path);
                    var objectData = new ItemInfo();
                    path = path.Replace("/", @"\");
                    pathData = pathData.Replace("/", @"\");

                    using (StreamReader sr = new StreamReader(pathData)) 
                    {
                        objectData = JsonConvert.DeserializeObject<ItemInfo>(sr.ReadToEnd());
                    }

                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        var message = new DiscordMessageBuilder()
                        .AddFile(filename, fs);

                        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                        {
                            Title = i.name.Lines.En,
                            Description = "",
                            ImageUrl = "attachment://" + filename,
                        };

                        foreach (ItemInfo.InfoBlock e in objectData.InfoBlocks)
                        {
                            if (e.Text != null && e.Text.Key != null && e.Text.Key.Contains("description"))
                            {
                                embed.Description = e.Text.Lines.En;
                            }
                        }

                        switch (objectData.Color)
                        {
                            case "RANK_MASTER":
                                embed.Color = DiscordColor.Red;
                                break;
                            case "RANK_VETERAN":
                                embed.Color = DiscordColor.Purple;
                                break;
                            case "RANK_STALKER":
                                embed.Color = DiscordColor.Azure;
                                break;
                            case "RANK_NEWBIE":
                                embed.Color = DiscordColor.Green;
                                break;
                        }

                        message.Embed = embed;

                        await ctx.Channel.SendMessageAsync(message);
                    }
                }
            }
        }
    }
}
