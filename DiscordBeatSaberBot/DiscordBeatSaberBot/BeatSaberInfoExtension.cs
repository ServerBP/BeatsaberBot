﻿using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    internal static class BeatSaberInfoExtension
    {
        public static async Task<List<List<string>>> GetPlayers()
        {
            string url = "https://scoresaber.com/global";
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']");
                return table.Descendants("tr").Skip(1).Select(tr => tr.Descendants("td").Select(td => WebUtility.HtmlDecode(td.InnerText)).ToList()).ToList();
            }
        }

        public static async Task<EmbedBuilder> GetTop10Players()
        {
            var topInfo = "";
            var counter = 0;
            var top10 = await GetPlayers();
            foreach (var player in top10)
            {
                var infoToTell = "";
                foreach (var result in player)
                {
                    var item = result.Replace(@"\r\n", " ").Trim();
                    if (!string.IsNullOrEmpty(item)) { infoToTell += item + " "; }
                }

                topInfo += infoToTell + "\n";
                counter++;
                if (counter >= 10) { break; }
            }

            var builder = new EmbedBuilder();
            builder.WithTitle("Top 10 Beatsaber Players");
            builder.WithDescription("Top 10 best beatsaber players");
            builder.AddInlineField("Players", topInfo);

            builder.WithColor(Color.Red);
            return builder;
        }

        public static async Task<List<EmbedBuilder>> GetPlayer(string playerName)
        {
            var playerId = await GetPlayerId(playerName);

            if (playerId.Count == 0)
            {
                var embedList = new List<EmbedBuilder>
                {
                    EmbedBuilderExtension.NullEmbed("No Results", "I am sorry, I could not find any person named: " + playerName, null, null)
                };
                return embedList;
            }

            var players = await GetPlayerInfo(playerName);

            var builders = new List<EmbedBuilder>();

            foreach (var player in players)
            {
                var builder = new EmbedBuilder();
                string countryNameSmall = player.countryName;

                var ppNext = "-";
                var ppBefore = "-";
                var playerNextName = "Not Found o.o";
                try
                {
                    var ppNextDouble = Math.Round(double.Parse(player.Next.pp) - double.Parse(player.pp), 0);
                    var ppBeforeDouble = Math.Round(double.Parse(player.pp) - double.Parse(player.Before.pp), 0);
                    ppNext = ppNextDouble.ToString();
                    ppBefore = ppBeforeDouble.ToString();
                    playerNextName = player.Next.name;
                }
                catch
                {

                }

                if (player.steamLink != "#")

                {
                    builder.ThumbnailUrl = player.imgLink;
                    
                    builder.AddInlineField(playerName, "Global Ranking: #" + player.rank + "\n\n" + "Country Ranking: #" + player.countryRank + "\n\n" + "Country: " + player.countryName + " :flag_" + countryNameSmall.ToLower() + ":" + "\n\n" + "Play Count: " + player.playCount + "\n\n" + "Total Score: " + player.totalScore + "\n\n" + "Performance Points: " + player.pp + "\n\n" + "Steam: " + player.steamLink + "\n\n" + "Player above: " + playerNextName + "\n\n" + "PP till UpRank: " + ppNext + "\n\n" + "PP till DeRank: " + ppBefore);
                }
                else
                {
                    builder.AddInlineField(playerName, "Global Ranking: #" + player.rank + "\n\n" + "Country Ranking: #" + player.countryRank + "\n\n" + "Country: " + player.countryName + " :flag_" + countryNameSmall.ToLower() + ":" + "\n\n" + "Play Count: " + player.playCount + "\n\n" + "Total Score: " + player.totalScore + "\n\n" + "Performance Points: " + player.pp + "\n\n" + "Oculus user" + "\n\n" + "Player above: " + playerNextName + "\n\n" + "PP till UpRank: " + ppNext + "\n\n" + "PP till DeRank: " + ppBefore);
                }

                builder.Timestamp = DateTimeOffset.Now;
                //var rankValue = player.rank.Split('#')[1].Replace(",", "");
                var rankInt = player.rank;
                var rankColor = Rank.GetRankColor(rankInt);
                builder.WithColor(await rankColor);
                builders.Add(builder);
            }


            return builders;
        }

        public static async Task<List<EmbedBuilder>> GetSongs(string search)
        {
            var songs = (List<List<string>>)null;
            var pictureUrl = new List<List<string>>();
            var url = "https://beatsaver.com/search/all/" + search.Replace(" ", "%20");

            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//div[@class='row']");
                songs = table.Descendants("tr").Skip(1).Select(tr => tr.Descendants("td").Select(td => WebUtility.HtmlDecode(td.InnerHtml)).ToList()).ToList();

                pictureUrl = table.Descendants("tr").Select(tr => tr.Descendants("img").Select(img => WebUtility.HtmlDecode(img.GetAttributeValue("src", ""))).ToList()).ToList();
            }

            if (songs.Count <= 0)
            {
                var returnList = new List<EmbedBuilder>
                {
                    EmbedBuilderExtension.NullEmbed("No songs found", "Sorry, I did not find any songs with the term: " + search, null, null)
                };
                return returnList;
            }

            var songNameList = new List<string>();
            var downloadLinkList = new List<string>();
            var authorList = new List<string>();
            var difficultiesList = new List<string>();
            foreach (var songInfo in songs)
            {
                foreach (var info in songInfo)
                {
                    if (info.Contains("Song")) { songNameList.Add(info); }

                    if (info.Contains("Author")) { authorList.Add(info); }

                    if (info.Contains("Difficulties")) { difficultiesList.Add(info); }

                    if (info.Contains("href"))
                    {
                        var addon = info.Split('"').Where(x => x.Contains("beatsaver"));
                        downloadLinkList.Add(addon.First());
                    }
                }
            }

            var pictureFilter = new List<string>();
            foreach (var pictureList in pictureUrl)
            {
                foreach (var picture in pictureList)
                {
                    if (!string.IsNullOrEmpty(picture)) { pictureFilter.Add(picture); }
                }
            }

            var builderList = new List<EmbedBuilder>();
            for (int i = 0; i < pictureFilter.Count; i++)
            {
                var builder = new EmbedBuilder();
                builder.WithTitle("Song Info");
                builder.WithThumbnailUrl(pictureFilter[i]);
                builder.WithDescription("All songs that contains " + search);
                builder.AddInlineField(songNameList[i], "\n" + difficultiesList[i] + "\n" + authorList[i] + "\n" + "\nDownload Link\n" + downloadLinkList[i] + "\n \n ");
                builder.WithColor(Color.Red);
                builderList.Add(builder);
            }


            return builderList;
        }

        public static async Task<EmbedBuilder> GetTopSongList(string search)
        {
            if (search != "!bs topsong")
            {
                var idList = await GetPlayerId(search);
                var build = await GetBestSongWithId(idList.First());
                build.WithTitle("Top song from " + search);
                return build;
            }

            var TopSongInfo = new List<string>();
            var TopSongUrl = new List<List<string>>();
            var topSongUrlString = "";
            var topSongImg = new List<List<string>>();
            var topSongImgString = "";
            var topList = new List<List<string>>();
            var url = "https://scoresaber.com/top";
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='table table-bordered']");
                TopSongInfo = table.Descendants("tr").Skip(1).Select(tr => tr.Descendants("td").Select(td => WebUtility.HtmlDecode(td.InnerText)).ToList()).First().ToList();

                topSongImg = table.Descendants("tr").Skip(1).Select(tr => tr.Descendants("img").Select(img => WebUtility.HtmlDecode(img.GetAttributeValue("src", ""))).ToList()).ToList();

                TopSongUrl = table.Descendants("tr").Skip(1).Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList()).ToList();
                topSongUrlString = TopSongUrl.First().First();
                topSongImgString = "https://scoresaber.com" + topSongImg.First().First();
            }

            using (var client = new HttpClient())
            {
                url = "https://scoresaber.com" + topSongUrlString;
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='table table-bordered ']");
                topList = table.Descendants("tr").Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()).ToList();
            }

            var songName = TopSongInfo[0].Replace("\r\n", "").Trim();
            var songDifficulty = TopSongInfo[2];
            var songStar = TopSongInfo[4];
            var songPlays = TopSongInfo[3];


            var builder = new EmbedBuilder();
            builder.WithTitle(songName.ToString());
            builder.WithDescription("Difficulty: " + songDifficulty + "\n" + "Star rate: " + songStar + "\n" + "Plays last 24H: " + songPlays);
            builder.WithThumbnailUrl(topSongImgString);
            var output = "";
            for (var x = 1; x <= 10; x++) { output += "#" + x + topList[x].First() + "\n"; }

            builder.AddInlineField("Top Players", output);

            builder.WithColor(Color.Red);
            return builder;
        }

        public static async Task<List<EmbedBuilder>> GetRanks()
        {
            var builderList = new List<EmbedBuilder>();

            var builder = new EmbedBuilder();
            builder.WithTitle("Top " + Rank.rankMaster.ToString());
            builder.WithColor(await Rank.GetRankColor(Rank.rankMaster));
            builderList.Add(builder);

            builder = new EmbedBuilder();
            builder.WithTitle("<" + Rank.rankChallenger.ToString());
            builder.WithColor(await Rank.GetRankColor(Rank.rankChallenger));
            builderList.Add(builder);

            builder = new EmbedBuilder();
            builder.WithTitle("<" + Rank.rankDiamond.ToString());
            builder.WithColor(await Rank.GetRankColor(Rank.rankDiamond));
            builderList.Add(builder);

            builder = new EmbedBuilder();
            builder.WithTitle("<" + Rank.rankPlatinum.ToString());
            builder.WithColor(await Rank.GetRankColor(Rank.rankPlatinum));
            builderList.Add(builder);

            builder = new EmbedBuilder();
            builder.WithTitle("<" + Rank.rankGold.ToString());
            builder.WithColor(await Rank.GetRankColor(Rank.rankGold));
            builderList.Add(builder);

            builder = new EmbedBuilder();
            builder.WithTitle("<" + Rank.rankSilver.ToString());
            builder.WithColor(await Rank.GetRankColor(Rank.rankSilver));
            builderList.Add(builder);

            builder = new EmbedBuilder();
            builder.WithTitle("+" + Rank.rankSilver);
            builder.WithColor(await Rank.GetRankColor(9999999));
            builderList.Add(builder);

            return builderList;
        }

        public static async Task<EmbedBuilder> GetInviteLink()
        {
            var builder = new EmbedBuilder();
            builder.WithTitle("Invitation Link");
            builder.WithDescription(Configuration.inviteLink);
            return builder;
        }

        public static async Task<string> GetImageUrlFromId(string playerId)
        {
            var playerImgUrl = "";
            using (var client = new HttpClient())
            {
                var url = "https://scoresaber.com" + playerId;
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                playerImgUrl = doc.DocumentNode.SelectSingleNode("//img[@class='image is-96x96']").GetAttributeValue("src", "");
            }

            return playerImgUrl;
        }

        public static async Task<List<string>> GetPlayerId(string search)
        {
            var player = new Player(search);
            return await player.GetPlayerId();
        }

        public static async Task<EmbedBuilder> GetBestSongWithId(string playerId)
        {
            var playerTopSongImg = "";
            var playerTopSongLink = "";
            var playerTopSongName = "";
            var playerTopSongPP = "";
            var playerTopSongAcc = "";
            var songName = "";
            var songDifficulty = "";
            var songAuthor = "";

            var url = "https://scoresaber.com" + playerId;
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking songs']");
                playerTopSongImg = "https://scoresaber.com" + table.Descendants("tbody").Select(tr => tr.Descendants("img").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("src", ""))).ToList()).ToList().First().First();
                playerTopSongLink = table.Descendants("tbody").Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList()).ToList().First().First();
                playerTopSongName = table.Descendants("tbody").Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()).ToList().First().First();
                playerTopSongPP = doc.DocumentNode.SelectSingleNode("//span[@class='scoreTop ppValue']").InnerText;
                playerTopSongAcc = doc.DocumentNode.SelectSingleNode("//span[@class='scoreBottom']").InnerText;
                songName = doc.DocumentNode.SelectSingleNode("//span[@class='songTop pp']").InnerText;
                songDifficulty = doc.DocumentNode.SelectSingleNode("//span[@class='songTop pp']").Descendants("span").First().InnerText;
                songAuthor = doc.DocumentNode.SelectSingleNode("//span[@class='songTop mapper']").InnerText;
            }

            var builder = new EmbedBuilder();
            builder.AddInlineField("Song", "name: " + songName + "\n" + "difficulty: " + songDifficulty + "\n" + "Author: " + songAuthor + "\n" + "pp from this song: " + playerTopSongPP + "\n" + playerTopSongAcc + "\n" + "https://scoresaber.com" + playerTopSongLink + "\n");
            builder.WithImageUrl(playerTopSongImg);
            try
            {
                builder.WithThumbnailUrl(await GetImageUrlFromId(playerId));
            }
            catch
            {

            }
            return builder;
        }

        public static async Task<EmbedBuilder> GetRecentSongWithId(string playerId)
        {
            var playerTopSongImg = "";
            var playerTopSongLink = "";
            var playerTopSongName = "";
            var playerTopSongPP = "";
            var playerTopSongAcc = "";
            var songName = "";
            var songDifficulty = "";
            var songAuthor = "";
            var playerName = "";

            var url = "https://scoresaber.com" + playerId + "&sort=2";
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking songs']");
                playerTopSongImg = "https://scoresaber.com" + table.Descendants("tbody").Select(tr => tr.Descendants("img").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("src", ""))).ToList()).ToList().First().First();
                playerTopSongLink = table.Descendants("tbody").Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList()).ToList().First().First();
                playerTopSongName = table.Descendants("tbody").Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()).ToList().First().First();
                playerTopSongPP = doc.DocumentNode.SelectSingleNode("//span[@class='scoreTop ppValue']").InnerText;
                playerTopSongAcc = doc.DocumentNode.SelectSingleNode("//span[@class='scoreBottom']").InnerText;
                songName = doc.DocumentNode.SelectSingleNode("//span[@class='songTop pp']").InnerText;
                songDifficulty = doc.DocumentNode.SelectSingleNode("//span[@class='songTop pp']").Descendants("span").First().InnerText;
                songAuthor = doc.DocumentNode.SelectSingleNode("//span[@class='songTop mapper']").InnerText;
                playerName = doc.DocumentNode.SelectSingleNode("//h5[@class='title is-5']").InnerText;
            }

            var songDetails = playerTopSongName;


            var builder = new EmbedBuilder();
            builder.AddInlineField(playerName, "name: " + songName + "\n" + "difficulty: " + songDifficulty + "\n" + "Author: " + songAuthor + "\n" + "pp from this song: " + playerTopSongPP + "\n" + playerTopSongAcc + "\n" + "https://scoresaber.com" + playerTopSongLink + "\n");
            builder.WithImageUrl(playerTopSongImg);
            try
            {
                builder.WithThumbnailUrl(await GetImageUrlFromId(playerId));
            }
            catch
            {

            }
            
            return builder;
        }

        public static async Task<(string, string)> GetRecentSongInfoWithId(string playerId)
        {
            var playerTopSongImg = "";
            var playerTopSongLink = "";
            var playerTopSongName = "";
            var playerTopSongPP = "";
            var playerTopSongAcc = "";
            var songName = "";
            var songDifficulty = "";
            var songAuthor = "";
            var playerName = "";

            var url = "https://scoresaber.com" + playerId + "&sort=2";
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking songs']");
                playerTopSongImg = "https://scoresaber.com" + table.Descendants("tbody").Select(tr => tr.Descendants("img").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("src", ""))).ToList()).ToList().First().First();
                playerTopSongLink = table.Descendants("tbody").Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList()).ToList().First().First();
                playerTopSongName = table.Descendants("tbody").Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()).ToList().First().First();
                playerTopSongPP = doc.DocumentNode.SelectSingleNode("//span[@class='scoreTop ppValue']").InnerText;
                playerTopSongAcc = doc.DocumentNode.SelectSingleNode("//span[@class='scoreBottom']").InnerText;
                songName = doc.DocumentNode.SelectSingleNode("//span[@class='songTop pp']").InnerText;
                songDifficulty = doc.DocumentNode.SelectSingleNode("//span[@class='songTop pp']").Descendants("span").First().InnerText;
                songAuthor = doc.DocumentNode.SelectSingleNode("//span[@class='songTop mapper']").InnerText;
                playerName = doc.DocumentNode.SelectSingleNode("//h5[@class='title is-5']").InnerText;
            }

            var songDetails = playerTopSongName;

            return (playerName, songName);
        }

        public static async Task<EmbedBuilder> GetTopxCountry(string search)
        {
            var input = search.Split(" ");
            if (input.Count() == 1)
            {
                var name = search;
                var player = await GetPlayerInfo(name);
                var countryrank = player.First().countryRank;
                var countryName = player.First().countryName;
                return await GetTopCountryWithName(countryrank, countryName, search);
            }

            if (!input[1].Any(char.IsDigit))
            {
                var name = search;
                var player = await GetPlayerInfo(name);
                var countryrank = player.First().countryRank;
                var countryName = player.First().countryName;
                return await GetTopCountryWithName(countryrank, countryName, search);
            }

            if (int.Parse(input[1]) > 50) { return EmbedBuilderExtension.NullEmbed("Sorry", "Search amount is too big", null, null); }

            decimal t = int.Parse(input[1]) / 50;
            var tab = Math.Ceiling(t) + 1;
            var count = int.Parse(input[1]);
            var Names = new List<string>();
            var ids = new List<string>();

            for (var x = 1; x <= tab; x++)
            {
                var url = "https://scoresaber.com/global/" + x + "&country=" + input[0];
                using (var client = new HttpClient())
                {
                    var html = await client.GetStringAsync(url);
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']");
                    Names.AddRange(table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList());

                    table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']");
                    ids = table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList();
                }
            }

            var topx = new List<string>();
            for (var x = 0; x < int.Parse(input[1]); x++)
            {
                var add = Names[x].Replace("\r\n", " ").Replace("&nbsp&nbsp", "");
                add = add.Trim();
                topx.Add(add);
            }

            var builder = new EmbedBuilder();
            var output = "";
            var counter = 1;
            foreach (var rank in topx)
            {
                output += "#" + counter + " " + rank + "\n";

                counter += 1;
            }

            builder.AddInlineField("Top " + input[1] + " " + input[0].ToUpper() + " :flag_" + input[0] + ":", output);
            return builder;
        }

        public static async Task<EmbedBuilder> GetTopCountryWithName(int Rank,
            string country,
            string playerName)
        {
            var countryName = country;
            double countryRank = Rank;
            double t = countryRank / 50;
            var tab = Math.Ceiling(t);
            var rankOnTab = countryRank % 50;
            var count = countryRank;
            var Names = new List<string>();
            var namesTop = new List<string>();
            var namesBottom = new List<string>();

            var url = "https://scoresaber.com/global/" + tab + "&country=" + country;
            await GetNames(url);

            async Task GetNames(string infoUrl,
                int otherPage = 0)
            {
                using (var client = new HttpClient())
                {
                    var html = await client.GetStringAsync(infoUrl);
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']");
                    if (otherPage == 1) { namesBottom.AddRange(table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()); }
                    else if (otherPage == 2) { namesTop.AddRange(table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()); }
                    else { Names.AddRange(table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()); }
                }
            }

            if (rankOnTab < 4 && rankOnTab != 0 && tab != 1)
            {
                url = "https://scoresaber.com/global/" + (tab - 1) + "&country=" + country;
                await GetNames(url, 1);
            }

            if (rankOnTab > 47 || rankOnTab == 0)
            {
                url = "https://scoresaber.com/global/" + (tab + 1) + "&country=" + country;
                await GetNames(url, 2);
            }

            var topx = new List<string>();
            for (var x = 0; x < 50; x++)
            {
                if (x < (rankOnTab + 3) && x > (rankOnTab - 3))
                {
                    var add = Names[x].Replace("\r\n", " ").Replace("&nbsp&nbsp", "");
                    add = add.Trim();
                    topx.Add(add);
                }
            }

            var outputList = new List<string>();
            outputList.AddRange(namesBottom);
            outputList.AddRange(Names);
            outputList.AddRange(namesTop);

            var builder = new EmbedBuilder();
            var output = "";
            var counter = 1;
            foreach (var rank in outputList)
            {
                if (counter > Rank - 4 && counter < Rank + 4)
                {
                    if (counter == Rank) { output += "#" + counter + " " + rank.Replace("\r\n", " ").Replace("&nbsp&nbsp", "").Trim() + "\n"; }
                    else { output += "#" + counter + " " + rank.Replace("\r\n", " ").Replace("&nbsp&nbsp", "").Trim() + "\n"; }
                }

                counter += 1;
            }

            builder.AddInlineField("RankList from " + playerName + " \nCountry: " + country + " " + " :flag_" + country.ToLower() + ":", output);
            return builder;
        }

        public static async Task<(string, string)> RankedNeighbours(string playerName, int recursionLoop = 0)
        {
            var playerInfo = await GetPlayerInfo(playerName, recursionLoop);
            double Rank = playerInfo.First().rank;
            double t = Rank / 50;
            var tab = Math.Ceiling(t);
            var rankOnTab = Rank % 50;
            var count = Rank;
            var Names = new List<string>();
            var namesTop = new List<string>();
            var namesBottom = new List<string>();

            var url = "https://scoresaber.com/global/" + tab;
            await GetNames(url);

            async Task GetNames(string infoUrl,
                int otherPage = 0)
            {
                using (var client = new HttpClient())
                {
                    var html = await client.GetStringAsync(infoUrl);
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    var table = doc.DocumentNode.SelectSingleNode("//table[@class='ranking global']");
                    if (otherPage == 1) { namesBottom.AddRange(table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()); }
                    else if (otherPage == 2) { namesTop.AddRange(table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()); }
                    else { Names.AddRange(table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()); }
                }
            }

            if (rankOnTab < 4 && rankOnTab != 0 && tab != 1)
            {
                url = "https://scoresaber.com/global/" + (tab - 1);
                await GetNames(url, 1);
            }

            if (rankOnTab > 47 || rankOnTab == 0)
            {
                url = "https://scoresaber.com/global/" + (tab + 1);
                await GetNames(url, 2);
            }

            var topx = new List<string>();
            for (var x = 0; x < 50; x++)
            {
                if (x < (rankOnTab + 3) && x > (rankOnTab - 3))
                {
                    var add = Names[x].Replace("\r\n", " ").Replace("&nbsp&nbsp", "");
                    add = add.Trim();
                    topx.Add(add);
                }
            }

            var outputList = new List<string>();
            outputList.AddRange(namesBottom);
            outputList.AddRange(Names);
            outputList.AddRange(namesTop);

            var builder = new EmbedBuilder();
            var output = new List<string>();
            var counter = 1;

            if (outputList.Count >= 100) { rankOnTab += 50; }

            foreach (var rank in outputList)
            {
                if (counter > rankOnTab - 2 && counter < rankOnTab + 2)
                {
                    if (rank.ToLower().Contains(playerName.ToLower())) { output.Add(rank.Replace("\r\n", " ").Replace("&nbsp&nbsp", "").Trim()); }
                    else { output.Add(rank.Replace("\r\n", " ").Replace("&nbsp&nbsp", "").Trim()); }
                }

                counter += 1;
            }


            return (output[0], output[2]);
        }

        public static async Task<string> GetPlayerCountryRank(string name)
        {
            var playerInfo = (List<List<string>>)null;
            var playerInfo2 = (List<List<string>>)null;
            var playerId = await GetPlayerId(name);
            var url = "https://scoresaber.com" + playerId.First();
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//div[@class='columns']");

                playerInfo = table.Descendants("div").Skip(1).Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList()).ToList();

                playerInfo2 = table.Descendants("div").Skip(1).Select(tr => tr.Descendants("li").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()).ToList();
            }

            return playerInfo.First()[2].Replace("/global?country=", "") + playerInfo2.First()[0].Replace("\r\n", "").Trim();
        }

        public static async Task<EmbedBuilder> AddRole(SocketMessage message)
        {
            var name = message.Content.Substring(12);
            var id = await GetPlayerId(name);

            var guild = message.Channel as SocketGuildChannel;
            if (string.IsNullOrEmpty(id.First())) { return EmbedBuilderExtension.NullEmbed("Soryy", "This name does not exist", null, null); }

            var (playerName, songName) = await GetRecentSongInfoWithId(id.First());

            if (songName == "Hi - Hi Easy")
            {
                var user = message.Author;
                var guildUser = user as IGuildUser;

                string rankcountry = await GetPlayerCountryRank(name);
                var rankcountryList = rankcountry.Split("-");
                rankcountry = rankcountryList[1].Replace("(", "").Replace(")", "").Replace("#", "").Trim();

                var rank = int.Parse(rankcountry);
                if (rank == 1)
                {
                    await guildUser.AddRoleAsync(GetRole("Nummer 1"));
                    return EmbedBuilderExtension.NullEmbed("Completed", "Your role is now top 1", null, null);
                }
                else if (rank <= 3)
                {
                    await guildUser.AddRoleAsync(GetRole("Top 3"));
                    return EmbedBuilderExtension.NullEmbed("Completed", "Your role is now top 3", null, null);
                }
                else if (rank <= 10)
                {
                    await guildUser.AddRoleAsync(GetRole("Top 10"));
                    return EmbedBuilderExtension.NullEmbed("Completed", "Your role is now top 10", null, null);
                }
                else if (rank <= 25)
                {
                    await guildUser.AddRoleAsync(GetRole("Top 25"));
                    return EmbedBuilderExtension.NullEmbed("Completed", "Your role is now top 25", null, null);
                }
                else if (rank <= 50)
                {
                    await guildUser.AddRoleAsync(GetRole("Top 50"));
                    return EmbedBuilderExtension.NullEmbed("Completed", "Your role is now top 50", null, null);
                }
                else if (rank <= 100)
                {
                    await guildUser.AddRoleAsync(GetRole("Top 100"));
                    return EmbedBuilderExtension.NullEmbed("Completed", "Your role is now top 100", null, null);
                }
                else if (rank > 100)
                {
                    await guildUser.AddRoleAsync(GetRole("Top 501+"));
                    return EmbedBuilderExtension.NullEmbed("Completed", "Your role is now top 501 +", null, null);
                }

                SocketRole GetRole(string botname)
                {
                    var role = guild.Guild.Roles.FirstOrDefault(x => x.Name == botname);
                    return role;
                }
            }

            return EmbedBuilderExtension.NullEmbed("Soryy", "Discord can not be linked because the recent song is not [Hi - Hi Easy] \n Please play this song with 0 points to register", null, null);
        }

        public static async Task<List<Player>> GetPlayerInfo(string playerName, int recursionLoop = 0)
        {
            var recursionCounter = recursionLoop;

            var players = new List<Player>();

            var playerInfo = (List<List<string>>)null;
            var playerInfo2 = (List<List<string>>)null;
            var playerImg = (List<List<string>>)null;
            var url = "https://scoresaber.com/global?search=" + playerName.Replace(" ", "+");
            var ids = await GetPlayerId(playerName);
            var playerId = ids;

            var counter = 0;
            foreach (var id in ids)
            {
                var player = new Player(playerName);

                url = "https://scoresaber.com" + playerId[counter];
                using (var client = new HttpClient())
                {
                    var html = await client.GetStringAsync(url);
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    var table = doc.DocumentNode.SelectSingleNode("//div[@class='columns']");

                    playerInfo = table.Descendants("div").Skip(1).Select(tr => tr.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList()).ToList();
                    playerInfo2 = table.Descendants("div").Skip(1).Select(tr => tr.Descendants("li").Select(a => WebUtility.HtmlDecode(a.InnerText)).ToList()).ToList();
                    playerImg = table.Descendants("div").Select(tr => tr.Descendants("img").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("src", ""))).ToList()).ToList();
                }

                var rank = playerInfo2.First()[0].Replace("\r\n", "").Trim();
                var ranks = rank.Split('-');

                player.rank = int.Parse(ranks[0].Replace("Player Ranking: #", "").Replace(",", "").Trim());
                player.steamLink = playerInfo.First()[0];
                player.countryName = playerInfo.First()[2].Replace("/global?country=", "").ToUpper();
                player.countryRank = int.Parse(ranks[1].Replace("(", "").Replace(")", "").Replace("#", "").Replace(",", ""));
                var pp = playerInfo2.First()[1].Replace("\r\n", "").Replace("Performance Points: ", "").Replace(",", "").Replace("pp", "").Trim();
                player.pp = pp.Split('.')[0];
                player.playCount = int.Parse(playerInfo2.First()[2].Replace("\r\n", "").Replace(",", "").Replace("Play Count: ", "").Trim());
                player.totalScore = playerInfo2.First()[3].Replace("\r\n", "").Replace("Total Score: ", "").Trim();
                player.countryIcon = ":flag_" + player.countryName + ":";
                player.imgLink = playerImg.First().First();
                player.name = player.name;

                if (!(recursionCounter >= 1))
                {
                    var nextAndBefore = await RankedNeighbours(playerName, 1);
                    var playerNext = await GetPlayerInfo(nextAndBefore.Item1, 1);
                    var playerBefore = await GetPlayerInfo(nextAndBefore.Item2, 1);
                    try
                    {
                        player.Next = playerNext.First();
                        player.Before = playerBefore.First();
                    }
                    catch
                    {
                        Console.WriteLine(nextAndBefore.Item1 + " or " + nextAndBefore.Item2 + " is not found");
                    }
                }

                players.Add(player);
                counter++;
            }


            return players;
        }

        public static async Task<EmbedBuilder> PlayerComparer(string message)
        {
            var splitting = message.Split(" vs ");

            var player1List = await GetPlayerInfo(splitting[0].Trim());
            var player2List = await GetPlayerInfo(splitting[1].Trim());

            var player1 = player1List.First();
            var player2 = player2List.First();

            var builder = new EmbedBuilder();

            //builder.AddInlineField(splitting[0],
            //    "Rank" + "\n" +
            //    "Country" + "\n" +
            //    "CountryRank" + "\n" +
            //    "PlayCount" + "\n" +
            //    "PP" + "\n" +
            //    "TotalScore" + "\n");

            builder.AddInlineField(splitting[0] + " " + player1.countryIcon.ToLower(), "#" + player1.rank + "\n" + "#" + player1.countryRank + "\n" + player1.playCount + "\n" + player1.pp + "pp" + "\n" + player1.totalScore + "\n");

            var pp1 = int.Parse(player1.pp.Split(".")[0]);
            var pp2 = int.Parse(player2.pp.Split(".")[0]);
            var ppDifference = Math.Abs(pp1 - pp2);

            var totalPointsDifference = Math.Abs((int.Parse(player1.totalScore.Replace(",", "")) - int.Parse(player2.totalScore.Replace(",", ""))));

            builder.AddInlineField(":heavy_minus_sign:", (Math.Abs(player1.rank - player2.rank)) + "\n" + (Math.Abs(player1.countryRank - player2.countryRank)) + "\n" + (Math.Abs(player1.playCount - player2.playCount)) + "\n" + ppDifference + " pp" + "\n" + totalPointsDifference.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("us")));

            builder.AddInlineField(splitting[1] + " " + player2.countryIcon.ToLower(), "#" + player2.rank + "\n" + "#" + player2.countryRank + "\n" + player2.playCount + "\n" + player2.pp + "pp" + "\n" + player2.totalScore + "\n");


            return builder;
        }
    }
}