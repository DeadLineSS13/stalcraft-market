using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Newtonsoft.Json;
using SlacrafratMarketDiscordBot.Objects;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Enumeration;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

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
        public enum Servers{Russian, Europe};

        [SlashCommand("ping", "Test")]
        public async Task Ping(InteractionContext ctx)
        {
            ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }
        [SlashCommand("search", "Поиск предметов")]
        public async Task Search(InteractionContext ctx, [Option("Имя", "Название предмета")] string item, [Option("Сервер", "Выберите сервер")] Servers server)
        {
            var pathListing = GetPathFile("/listing.json", server);
            var ItemList = new List<Item>();
            using (StreamReader sr = new StreamReader(pathListing))
            {
                ItemList = JsonConvert.DeserializeObject<List<Item>>(sr.ReadToEnd());
            }

            foreach (Item i in ItemList)
            {
                if (i.name.Lines.Ru.Contains(item))
                {
                    var path = GetPathFile(i.icon, server);
                    var pathData = GetPathFile(i.data, server);
                    var filename = Path.GetFileName(path);
                    var objectData = new ItemInfo();



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

                        ChangeColorEmbed(objectData.Color, embed);
                        embed.WithFooter("Данные предоставлены https://eapi.stalcraft.net/");

                        message.Embed = embed;
                        
                        await ctx.Channel.SendMessageAsync(message);
                    }
                }
                else if (i.name.Lines.En.Contains(item))
                {
                    var path = GetPathFile(i.icon, server);
                    var pathData = GetPathFile(i.data, server);
                    var filename = Path.GetFileName(path);
                    var objectData = new ItemInfo();

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

                        ChangeColorEmbed(objectData.Color, embed);
                        embed.WithFooter("Data provided by https://eapi.stalcraft.net/");

                        message.Embed = embed;

                        await ctx.Channel.SendMessageAsync(message);
                    }
                }
            }
        }

        [SlashCommand("price", "Поиск цен по предмету")]
        public async Task Price(InteractionContext ctx, [Option("Имя", "Название премдета")] string item, [Option("Сервер", "Выберите сервер")] Servers server)
        {
            var pathListing = GetPathFile("/listing.json", server);
            var ItemList = new List<Item>();
            using (StreamReader sr = new StreamReader(pathListing))
            {
                ItemList = JsonConvert.DeserializeObject<List<Item>>(sr.ReadToEnd());
            }

            foreach (Item i in ItemList)
            {
                if (i.name.Lines.Ru.Contains(item))
                {

                    var name = Path.GetFileNameWithoutExtension(i.icon);

                    using (var httpClient = new HttpClient())

                    {

                        using (var request = new HttpRequestMessage(new HttpMethod("GET"), GetHTTP(server) + name + "/history"))

                        {

                            var configObj = JsonConvert.DeserializeObject<Configuration>(Properties.Resources.config);

                            request.Headers.TryAddWithoutValidation("Client-Id", configObj.ClientId);

                            request.Headers.TryAddWithoutValidation("Client-Secret", configObj.ClientSecret);



                            var response = await httpClient.SendAsync(request);
                            var responseString = await response.Content.ReadAsStringAsync();
                            var objectResponse = JsonConvert.DeserializeObject<ItemPrice>(responseString);

                            var path = GetPathFile(i.icon, server);
                            var pathData = GetPathFile(i.data, server);
                            var filename = Path.GetFileName(path);
                            var objectData = new ItemInfo();

                            var minPrice = long.MaxValue;
                            var maxPrice = long.MinValue;

                            foreach (ItemPrice.Price p in objectResponse.Prices)
                            {
                                if (minPrice > (long)(p.PricePrice / p.Amount))
                                {
                                    minPrice = (long)p.PricePrice / p.Amount;
                                }
                                if (maxPrice < (long)(p.PricePrice / p.Amount))
                                {
                                    maxPrice = (long)p.PricePrice / p.Amount;
                                }
                            }

                            using (FileStream fs = new FileStream(path, FileMode.Open))
                            {
                                var message = new DiscordMessageBuilder()
                                .AddFile(filename, fs);

                                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                                {
                                    Title = i.name.Lines.Ru,
                                    Description = "",
                                };
                                string maxP = "None";
                                string mP = "None";
                                var culture = new CultureInfo("ru-RU")
                                {
                                    NumberFormat =
                                    {
                                        NumberGroupSeparator = ".",
                                    },
                                };
                                if (minPrice != long.MaxValue)
                                {
                                    mP = minPrice.ToString("#,#", culture);
                                }
                                if (maxPrice != long.MinValue)
                                {
                                    maxP = maxPrice.ToString("#,#", culture);
                                }
                                embed.WithThumbnail("attachment://" + filename);
                                embed.AddField("Цена", mP, true);
                                embed.AddField("Максимальная цена", maxP, true);
                                embed.AddField("Разница %", "Wip", true);
                                embed.WithFooter("Данные предоставлены https://eapi.stalcraft.net/");

                                ChangeColorEmbed(objectData.Color, embed);

                                message.Embed = embed;

                                await ctx.Channel.SendMessageAsync(message);
                            }
                        }

                    }

                }
                else if (i.name.Lines.En.Contains(item))
                {

                    var name = Path.GetFileNameWithoutExtension(i.icon);

                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("GET"), GetHTTP(server) + name + "/history"))

                        {

                            var configObj = JsonConvert.DeserializeObject<Configuration>(Properties.Resources.config);

                            request.Headers.TryAddWithoutValidation("Client-Id", configObj.ClientId);

                            request.Headers.TryAddWithoutValidation("Client-Secret", configObj.ClientSecret);



                            var response = await httpClient.SendAsync(request);
                            var responseString = await response.Content.ReadAsStringAsync();
                            var objectResponse = JsonConvert.DeserializeObject<ItemPrice>(responseString);

                            var path = GetPathFile(i.icon, server);
                            var pathData = GetPathFile(i.data, server);
                            var filename = Path.GetFileName(path);
                            var objectData = new ItemInfo();

                            var minPrice = long.MaxValue;
                            var maxPrice = long.MinValue;

                            foreach (ItemPrice.Price p in objectResponse.Prices)
                            {
                                if (minPrice > (long)(p.PricePrice / p.Amount))
                                {
                                    minPrice = (long)p.PricePrice / p.Amount;
                                }
                                if (maxPrice < (long)(p.PricePrice / p.Amount))
                                {
                                    maxPrice = (long)p.PricePrice / p.Amount;
                                }
                            }

                            using (FileStream fs = new FileStream(path, FileMode.Open))
                            {
                                var message = new DiscordMessageBuilder()
                                .AddFile(filename, fs);

                                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                                {
                                    Title = i.name.Lines.Ru,
                                    Description = "",
                                };
                                string maxP = "None";
                                string mP = "None";
                                var culture = new CultureInfo("ru-RU")
                                {
                                    NumberFormat =
                                    {
                                        NumberGroupSeparator = ".",
                                    },
                                };
                                if (minPrice != long.MaxValue)
                                {
                                    mP = minPrice.ToString("#,#", culture);
                                }
                                if (maxPrice != long.MinValue)
                                {
                                    maxP = maxPrice.ToString("#,#", culture);
                                }
                                embed.WithThumbnail("attachment://" + filename);
                                embed.AddField("Price", mP, true);
                                embed.AddField("Max Price", maxP, true);
                                embed.AddField("Price Difference", "Wip", true);
                                embed.WithFooter("Data provided by https://eapi.stalcraft.net/");

                                ChangeColorEmbed(objectData.Color, embed);

                                message.Embed = embed;

                                await ctx.Channel.SendMessageAsync(message);
                            }
                        }
                    }
                }
            }
        }
        
        public string GetPathFile(string file, Servers server)
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                file = file.Replace("/", @"\");
            }
            var subpath = GetSubPath(server);
            return Path.Combine(Environment.CurrentDirectory, subpath[0], subpath[1]) + file;
        }
        public List<string> GetSubPath(Servers server)
        {
            var url = new List<string>();
            url.Add("stalcraftdatabase");
            switch (server)
            {
                case Servers.Europe:
                    url.Add("global");
                    break;
                case Servers.Russian:
                    url.Add("ru");
                    break;
            }
            return url;
        }
        public string GetHTTP(Servers server)
        {
            var url = string.Empty;
            switch (server)
            {
                case Servers.Europe:
                    url = "https://eapi.stalcraft.net/eu/auction/";
                    break;
                case Servers.Russian:
                    url =  "https://eapi.stalcraft.net/ru/auction/";
                    break;
            }
            return url;
        }
        public void ChangeColorEmbed(string level, DiscordEmbedBuilder embed)
        {
            switch (level)
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
        }
    }
}
