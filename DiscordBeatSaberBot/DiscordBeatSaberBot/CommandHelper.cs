﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBeatSaberBot
{
    static class CommandHelper
    {
        private static List<string> helpCommands = new List<string>();

        private static string prefix = "prefix = !bs";
        private static string optional = "* = Optional";
        private static string required = "[] = required";
        private static string top10 = "top10 => (Gets the top 10 players)";
        private static string searchPlayer = "Search [username] => (Gets information about [Username])";
        private static string searchSongs = "Songs [songname] => (gives all available songs with information)";
        private static string topSong = "topsong *Username => (gets the current most played song and shows the top 10 players or gives the best played song from a player)";
        private static string ranks = "ranks => (gets all ranks with colors and range)";
        private static string mod = "mod => (gets the BeatSaberModManager.exe for plugins)";
        private static string invitationLink = "invite => (gets the invitationLink for the Beatsaber bot)";
        private static string recentsong = "recentsong [username] => (gets the most recent song that a user has played)";
        private static string addrole = "addrole [username] => (adds role of your rank (if your recent played song is [Tycho - Spectre]))";
        private static string country1 = "country *username => (gets rank list of 3 above and underneath this player)";
        private static string country2 = "country [countryPrefix] [x] => (gets the top x country list)";
        private static string comparer = "comparer [username] vs [username2] => (gets the top x country list)";
        private static string pplist = "pplist (Gives an link to google spreadsheet with all info from ranked songs)";
        private static string feed = "addFeed (Add the current channel in de feedlist so the bot will post beatsaber news)";

        static CommandHelper()
        {
            helpCommands.Add(prefix);
            helpCommands.Add(optional);
            helpCommands.Add(required);
            helpCommands.Add(top10);
            helpCommands.Add(searchPlayer);
            helpCommands.Add(searchSongs);
            helpCommands.Add(topSong);
            helpCommands.Add(recentsong);
            helpCommands.Add(ranks);
            helpCommands.Add(invitationLink);
            //helpCommands.Add(mod);
            helpCommands.Add(addrole);
            helpCommands.Add(country1);
            helpCommands.Add(country2);
            helpCommands.Add(comparer);
            helpCommands.Add(pplist);
            helpCommands.Add(feed);
        }

        public static List<string> Help()
        {
            return helpCommands;
        }
    }
}
