using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Newtonsoft.Json;
using SlacrafratMarketDiscordBot.Objects;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Enumeration;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using static SlacrafratMarketDiscordBot.Commands.MarketSlash;
using static SlacrafratMarketDiscordBot.Objects.ItemPrice;
using static System.Net.WebRequestMethods;

namespace SlacrafratMarketDiscordBot.Commands
{
    public class MarketSlash : ApplicationCommandModule
    {
        public enum Servers{Europe, Russian};

        [SlashCommand("search", "Поиск предметов")]
        public async Task Search(InteractionContext ctx, [Option("Имя", "Название предмета")] string item, [Option("Сервер", "Выберите сервер")] Servers server)
        {
            var pathListing = GetPathFile("/listing.json", server);
            var ItemList = new List<Item>();
            using (StreamReader sr = new StreamReader(pathListing))
            {
                ItemList = JsonConvert.DeserializeObject<List<Item>>(sr.ReadToEnd());
            }

            var interactivity = ctx.Client.GetInteractivity();
            var FoundedItemList = new List<Item>();

            foreach (Item i in ItemList)
            {
                if (i.name.Lines.Ru.Contains(item) || i.name.Lines.En.Contains(item))
                {
                    FoundedItemList.Add(i);
                }
            }
            ItemList = null;

            var len = FoundedItemList.Count();
            var message = DisplayItemEmbed(ctx, FoundedItemList[0], server, 0, len).GetAwaiter().GetResult();
            var number = 0;

            if (len == 1)
            {
                return;
            }

            var nextEmoji = DiscordEmoji.FromName(ctx.Client, ":arrow_right:");
            var backEmoji = DiscordEmoji.FromName(ctx.Client, ":arrow_left:");

            await message.CreateReactionAsync(backEmoji);
            await message.CreateReactionAsync(nextEmoji);

            var UserContact = true;
            do
            {
                var reactionResult = await interactivity.WaitForReactionAsync(x =>
                    x.Message == message,
                    ctx.Member
                );

                if (reactionResult.TimedOut)
                {
                    UserContact = false;
                }
                else if (reactionResult.Result.Emoji == nextEmoji)
                {
                    if (number == len - 1)
                    {
                        number = 0;
                    }
                    else
                    {
                        number++;
                    }
                    await message.DeleteReactionAsync(nextEmoji, reactionResult.Result.User);
                    await DisplayItemEmbedUpdate(message, FoundedItemList[number], server, number, len);
                    continue;
                }

                if (reactionResult.Result.Emoji == backEmoji)
                {
                    if (number == 0)
                    {
                        number = len - 1;
                    }
                    else
                    {
                        number--;
                    }
                    await message.DeleteReactionAsync(backEmoji, reactionResult.Result.User);
                    await DisplayItemEmbedUpdate(message, FoundedItemList[number], server, number, len);
                    continue;
                }
            }
            while (UserContact);

        }

        public async Task<DiscordMessage> DisplayItemEmbed(InteractionContext ctx, Item i, Servers server, int number, int len)
        {
            DiscordMessage message = null;
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
                var m = new DiscordMessageBuilder()
                            .AddFile(filename, fs);

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = i.name.Lines.Ru,
                    Description = "",
                    ImageUrl = "attachment://" + filename,
                };

                foreach (ItemInfo.InfoBlock e in objectData.InfoBlocks)
                {

                    if (e.Text != null && e.Text.Key != null && e.Text.Key.Contains("description"))

                    {

                        embed.Description = e.Text.Lines.Ru;

                    }

                }
                ChangeColorEmbed(objectData.Color, embed);
                number++;
                embed.WithFooter("Page " + number.ToString() + "/" + len.ToString() + "        " + "Данные предоставлены https://eapi.stalcraft.net/");

                m.Embed = embed;
                message = await ctx.Channel.SendMessageAsync(m);
            }

            return message;
        }

        public async Task DisplayItemEmbedUpdate(DiscordMessage message, Item i, Servers server, int number, int len)
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
                var m = new DiscordMessageBuilder()
                            .AddFile(filename, fs);

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = i.name.Lines.Ru,
                    Description = "",
                    ImageUrl = "attachment://" + filename,
                };

                foreach (ItemInfo.InfoBlock e in objectData.InfoBlocks)
                {

                    if (e.Text != null && e.Text.Key != null && e.Text.Key.Contains("description"))

                    {

                        embed.Description = e.Text.Lines.Ru;

                    }

                }

                ChangeColorEmbed(objectData.Color, embed);
                number++;
                embed.WithFooter("Page " + number.ToString() + "/" + len.ToString() + "        " + "Данные предоставлены https://eapi.stalcraft.net/");

                m.Embed = embed;
                await message.ModifyAsync(m);
            }
        }

        [SlashCommand("price", "Поиск цен по предмету")]
        public async Task Price(InteractionContext ctx, [Option("Имя", "Название предмета")] string item, [Option("Сервер", "Выберите сервер")] Servers server)
        {
            var pathListing = GetPathFile("/listing.json", server);
            var ItemList = new List<Item>();
            using (StreamReader sr = new StreamReader(pathListing))
            {
                ItemList = JsonConvert.DeserializeObject<List<Item>>(sr.ReadToEnd());
            }

            var interactivity = ctx.Client.GetInteractivity();
            var FoundedItemList = new Dictionary<Item, Prices>();

            foreach (Item i in ItemList)
            {
                if (i.name.Lines.Ru.Contains(item) || i.name.Lines.En.Contains(item))
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

                            var minPrice = long.MaxValue;
                            var maxPrice = long.MinValue;

                            foreach (Price p in objectResponse.Prices)
                            {
                                if (minPrice > (p.PricePrice / p.Amount))
                                {
                                    minPrice = p.PricePrice / p.Amount;
                                }
                                if (maxPrice < (p.PricePrice / p.Amount))
                                {
                                    maxPrice = p.PricePrice / p.Amount;
                                }
                            }

                            var ObjectPrices = new Prices();
                            ObjectPrices.MinPrice = minPrice;
                            ObjectPrices.MaxPrice = maxPrice;
                            ObjectPrices.DefPrice = 0;

                            FoundedItemList.Add(i,ObjectPrices);

                        }

                    }
                }
            }

            var len = FoundedItemList.Values.Count();
            var message = await DisplayItemPriceEmbed(ctx, FoundedItemList.ElementAt(0).Key, FoundedItemList.ElementAt(0).Value, server, 1, len);
            var number = 0;

            if (len == 1)
            {
                return;
            }

            var nextEmoji = DiscordEmoji.FromName(ctx.Client, ":arrow_right:");
            var backEmoji = DiscordEmoji.FromName(ctx.Client, ":arrow_left:");

            await message.CreateReactionAsync(backEmoji);
            await message.CreateReactionAsync(nextEmoji);

            var UserContact = true;
            do
            {
                var reactionResult = await interactivity.WaitForReactionAsync(x =>
                    x.Message == message,
                    ctx.Member
                );

                if (reactionResult.TimedOut)
                {
                    UserContact = false;
                }
                else if (reactionResult.Result.Emoji == nextEmoji)
                {
                    if (number == len - 1)
                    {
                        number = 0;
                    }
                    else
                    {
                        number++;
                    }
                    await message.DeleteReactionAsync(nextEmoji, reactionResult.Result.User);
                    await DisplayItemPriceEmbedUpdate(message, FoundedItemList.ElementAt(number).Key, FoundedItemList.ElementAt(number).Value, server, number, len);
                    continue;
                }

                if (reactionResult.Result.Emoji == backEmoji)
                {
                    if (number == 0)
                    {
                        number = len - 1;
                    }
                    else
                    {
                        number--;
                    }
                    await message.DeleteReactionAsync(backEmoji, reactionResult.Result.User);
                    await DisplayItemPriceEmbedUpdate(message, FoundedItemList.ElementAt(number).Key, FoundedItemList.ElementAt(number).Value, server, number, len);
                    continue;
                }
            }
            while (UserContact);
        }

        public async Task<DiscordMessage> DisplayItemPriceEmbed(InteractionContext ctx, Item i, Prices p, Servers server, int number, int len)
        {
            DiscordMessage message = null;
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
                var m = new DiscordMessageBuilder()
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
                if (p.MinPrice != long.MaxValue)
                {
                    mP = p.MinPrice.ToString("#,#", culture);
                }
                if (p.MaxPrice != long.MinValue)
                {
                    maxP = p.MaxPrice.ToString("#,#", culture);
                }
                embed.WithThumbnail("attachment://" + filename);
                embed.AddField("Цена", mP, true);
                embed.AddField("Максимальная цена", maxP, true);
                embed.AddField("Разница %", "Wip", true);
                embed.WithFooter("Page " + number.ToString() + "/" + len.ToString() + "        " + "Данные предоставлены https://eapi.stalcraft.net/");

                ChangeColorEmbed(objectData.Color, embed);

                m.Embed = embed;

                message = await ctx.Channel.SendMessageAsync(m);
            }
            return message;
        }

        public async Task DisplayItemPriceEmbedUpdate(DiscordMessage message, Item i, Prices p, Servers server, int number, int len)
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
                var m = new DiscordMessageBuilder()
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
                if (p.MinPrice != long.MaxValue)
                {
                    mP = p.MinPrice.ToString("#,#", culture);
                }
                if (p.MaxPrice != long.MinValue)
                {
                    maxP = p.MaxPrice.ToString("#,#", culture);
                }
                embed.WithThumbnail("attachment://" + filename);
                embed.AddField("Цена", mP, true);
                embed.AddField("Максимальная цена", maxP, true);
                embed.AddField("Разница %", "Wip", true);
                embed.WithFooter("Page " + number.ToString() + "/" + len.ToString() + "        " + "Данные предоставлены https://eapi.stalcraft.net/");

                ChangeColorEmbed(objectData.Color, embed);

                m.Embed = embed;

                await message.ModifyAsync(m);
            }
        }

        [SlashCommand("prices", "Выводит список вещей с ценами")]
        public async Task Prices(InteractionContext ctx, [Option("Имя", "Название предмета")] string item, [Option("Сервер", "Выберите сервер")] Servers server)
        {
            var pathListing = GetPathFile("/listing.json", server);
            var ItemList = new List<Item>();
            using (StreamReader sr = new StreamReader(pathListing))
            {
                ItemList = JsonConvert.DeserializeObject<List<Item>>(sr.ReadToEnd());
            }

            var interactivity = ctx.Client.GetInteractivity();
            var ItemPriceLists = new Dictionary<Item, ItemPrice>();
            foreach (Item i in ItemList)
            {
                if (i.name.Lines.Ru.Contains(item) || i.name.Lines.En.Contains(item))
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

                            ItemPriceLists.Add(i, objectResponse);
                        }
                    }
                }
            }
            ItemList = null;

            var p = ItemPriceLists.ElementAt(0).Key;
            var k = ItemPriceLists.ElementAt(0).Value;
            var len = 0;
            foreach (Item o in ItemPriceLists.Keys)
            {
                len += ItemPriceLists[o].Prices.Count();
            }
            var message = DisplayItemPricesEmbed(ctx, p, k.Prices[0].PricePrice, k.Prices[0].Amount.ToString(), server, 1, len).GetAwaiter().GetResult();
            var number = 1;
            var number2 = 0;
            var number3 = 0;

            if (len == 1)
            {
                return;
            }

            var nextEmoji = DiscordEmoji.FromName(ctx.Client, ":arrow_right:");
            var backEmoji = DiscordEmoji.FromName(ctx.Client, ":arrow_left:");

            await message.CreateReactionAsync(backEmoji);
            await message.CreateReactionAsync(nextEmoji);

            var UserContact = true;
            do
            {
                var reactionResult = await interactivity.WaitForReactionAsync(x =>
                    x.Message == message,
                    ctx.Member
                );
                if (reactionResult.TimedOut)
                {
                    UserContact = false;
                }
                else if (reactionResult.Result.Emoji == nextEmoji)
                {
                    if (number == len)
                    {
                        number = 0;
                        number3 = -1;
                    }

                    if (number2 == k.Prices.Count() - 1)
                    {
                        number++;
                        number2 = 0;
                        number3++;
                        k = ItemPriceLists.ElementAt(number3).Value;

                    }
                    else
                    {
                        number++;
                        number2++;
                    }

                    await message.DeleteReactionAsync(nextEmoji, reactionResult.Result.User);
                    await DisplayItemPricesEmbedUpdate(message, ItemPriceLists.ElementAt(number3).Key, 
                                                                ItemPriceLists.ElementAt(number3).Value.Prices[number2].PricePrice,
                                                                ItemPriceLists.ElementAt(number3).Value.Prices[number2].Amount.ToString(), 
                                                                server, number, len);
                    continue;
                }

                else if (reactionResult.Result.Emoji == backEmoji)
                {
                    if (number == 1)
                    {
                        number = len + 1;
                        number3 = ItemPriceLists.Count();
                    }

                    if (number2 == 0)
                    {
                        number--;
                        number3--;
                        number2 = ItemPriceLists.ElementAt(number3).Value.Prices.Count() - 1;
                    }
                    else
                    {
                        number--;
                        number2--;
                    }

                    await message.DeleteReactionAsync(backEmoji, reactionResult.Result.User);
                    await DisplayItemPricesEmbedUpdate(message, ItemPriceLists.ElementAt(number3).Key, 
                                                                ItemPriceLists.ElementAt(number3).Value.Prices[number2].PricePrice,
                                                                ItemPriceLists.ElementAt(number3).Value.Prices[number2].Amount.ToString(),
                                                                server, number, len);
                    continue;
                }
            }
            while (UserContact);

        }

        public async Task<DiscordMessage> DisplayItemPricesEmbed(InteractionContext ctx, Item i, long price, string amount, Servers server, int number, int len)
        {
            DiscordMessage message = null;

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
                var m = new DiscordMessageBuilder()
                .AddFile(filename, fs);

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = i.name.Lines.Ru,
                    Description = "",
                };
                var culture = new CultureInfo("ru-RU")
                {
                    NumberFormat =
                            {
                                NumberGroupSeparator = ".",
                            },
                };
                var priceStr = price.ToString("#,#", culture);
                embed.WithThumbnail("attachment://" + filename);
                embed.AddField("Цена", priceStr, true);
                embed.AddField("Кол-во", amount, true);
                embed.WithFooter("Page " + number.ToString() + "/" + len.ToString() + "        " + "Данные предоставлены https://eapi.stalcraft.net/");

                ChangeColorEmbed(objectData.Color, embed);

                m.Embed = embed;

                message = await ctx.Channel.SendMessageAsync(m);

            }

            return message;
        }

        public async Task DisplayItemPricesEmbedUpdate(DiscordMessage message, Item i, long price, string amount, Servers server, int number, int len)
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
                var m = new DiscordMessageBuilder()
                .AddFile(filename, fs);

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = i.name.Lines.Ru,
                    Description = "",
                };
                var culture = new CultureInfo("ru-RU")
                {
                    NumberFormat =
                            {
                                NumberGroupSeparator = ".",
                            },
                };
                var priceStr = price.ToString("#,#", culture);
                embed.WithThumbnail("attachment://" + filename);
                embed.AddField("Цена", priceStr, true);
                embed.AddField("Кол-во", amount, true);
                embed.WithFooter("Page " + number.ToString() + "/" + len.ToString() + "        " + "Данные предоставлены https://eapi.stalcraft.net/");

                ChangeColorEmbed(objectData.Color, embed);

                m.Embed = embed;

                await message.ModifyAsync(m);

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
                    url.Add("ru");
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
