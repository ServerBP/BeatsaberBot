﻿using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DiscordBeatSaberBot
{
    class BeatSaberHourCounter
    {
        public BeatSaberHourCounter()
        {

        }

        public void TurnOnCounterForPlayer(SocketGuildUser userOld, SocketGuildUser userNew)
        {
            var userOldName = "";
            var userNewName = "";

            if (userOld.Game == null)
            {
                userOldName = "null";
            }
            else
            {
                userOldName = userOld.Game.Value.ToString();
            }

            if (userNew.Game == null)
            {
                userNewName = "null";
            }
            else
            {
                userNewName = userNew.Game.Value.ToString();
            }

            if (userOldName == "Beat Saber" && userNewName != "Beat Saber") //Finished Gaming
            {
                var data = getData("../../../DiscordIDBeatsaberHourCounter.txt");
                if (data.Count == 0) { // first input
                    var t = new string[]{ "0", DateTime.Now.ToString() };
                    data = new Dictionary<ulong, string[]>();
                    data.Add(123456, t);
                }

                var newData = new Dictionary<ulong, string[]>();
                foreach (var discordId in data)
                {
                    if (discordId.Key == userOld.Id)
                    {
                        var dateTime = discordId.Value[1];
                        var totalHoursnew = DateTime.Now - DateTime.ParseExact(dateTime, "yyyy-MM-dd hh:mm:ss.fff", CultureInfo.InvariantCulture);

                        var currentHours = int.Parse(discordId.Value[0]);
                        var totalHours = (int) totalHoursnew.TotalMinutes + currentHours;

                        var value = new string[] { totalHours.ToString(), ""};

                        newData.Add(discordId.Key, value);
                    }
                    else
                    {
                        newData.Add(discordId.Key, discordId.Value);
                    }


                }
                setData("../../../DiscordIDBeatsaberHourCounter.txt", newData);
            }

            if (userOldName != "Beat Saber" && userNewName == "Beat Saber") //Starting Gaming
            {
                var data = getData("../../../DiscordIDBeatsaberHourCounter.txt");
                if (data.Count == 0)
                { // first input
                    var t = new string[] { "0", DateTime.Now.ToString() };
                    data = new Dictionary<ulong, string[]>();
                    data.Add(123456, t);
                }
                var newData = new Dictionary<ulong, string[]>();
                foreach (var discordId in data)
                {
                    if (discordId.Key == userOld.Id)
                    {
                        var hourCount = discordId.Value[0];
                        var dateTime = DateTime.Now;

                        var value = new string[] { hourCount, dateTime.ToString("yyyy-MM-dd hh:mm:ss.fff") };

                        newData.Add(discordId.Key, value);

                    }
                    else
                    {
                        newData.Add(discordId.Key, discordId.Value);
                    }
                    
                }

                setData("../../../DiscordIDBeatsaberHourCounter.txt", newData);



            }

            Dictionary<ulong, string[]> getData(string filePath)
            {
                using (var r = new StreamReader(filePath))
                {
                    var json = r.ReadToEnd();
                    var data = JsonConvert.DeserializeObject<Dictionary<ulong, string[]>>(json);
                    if (data == null) return new Dictionary<ulong, string[]>();
                    return data;
                }
            }

            void setData(string filePath, Dictionary<ulong, string[]> newData)
            {
                using (var file = File.CreateText(filePath))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(file, newData);
                }
            }

            
        }

        public void InsertAndResetAllDutchMembers(DiscordSocketClient discord)
        {
            var guild = discord.GetGuild(505485680344956928);

            var newList = new Dictionary<ulong, string[]>();

            foreach (var user in guild.Users)
            {
                newList.Add(user.Id, new string[] { "0", "" });
            }

            newList.Add(0000, new string[] { "Start date", DateTime.Now.ToShortDateString()});

            using (var file = File.CreateText("../../../DiscordIDBeatsaberHourCounter.txt"))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, newList);
            }

        }

        public Embed GetTop25BeatSaberHours(SocketMessage message)
        {
            var channel = message.Channel;
            var topdata = getData("../../../DiscordIDBeatsaberHourCounter.txt");

            var embedBuilder = new EmbedBuilder
            {
                Title = "Top 25 Dutch Beat saber hours",
                Footer = new EmbedFooterBuilder { Text = "Start Date: " + topdata.GetValueOrDefault((ulong)0000)[1]}
            };
            var counter = 0;
            var sortedList = topdata.OrderByDescending(key => key.Value[0]);
            foreach (var top in sortedList)
            {
                if (counter >= 25 || top.Value[0] == "Start date") continue;
                embedBuilder.Description += "Discord ID: " + top.Key + "    Minutes played: " + top.Value[0] + "\n";
                counter++;
            }

            return embedBuilder.Build();


            Dictionary<ulong, string[]> getData(string filePath)
            {
                using (var r = new StreamReader(filePath))
                {
                    var json = r.ReadToEnd();
                    var data = JsonConvert.DeserializeObject<Dictionary<ulong, string[]>>(json);
                    if (data == null) return new Dictionary<ulong, string[]>();
                    return data;
                }
            }

        }
    }
}