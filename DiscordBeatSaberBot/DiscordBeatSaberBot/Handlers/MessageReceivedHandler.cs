﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBeatSaberBot.Commands;
using DiscordBeatSaberBot.Extensions;

namespace DiscordBeatSaberBot.Handlers
{
    public class MessageReceivedHandler
    {
        public async Task HandleMessage(DiscordSocketClient discordSocketClient, SocketMessage message, Program program)
        {
            if (message.Author.Username == "BeatSaber Bot") return;

            MessageDelete.DeleteMessageCheck(message, discordSocketClient);

            if (message.Content.Length <= 3)
                return;

            if (message.Content.Substring(0, 3).Contains("!bs"))
            {
                var messageCommand = message.Content.ToLower();

                var typingState = message.Channel.EnterTypingState(new RequestOptions
                {
                    Timeout = GlobalConfiguration.TypingTimeOut,
                });

                Console.WriteLine(message.Content);

                if (messageCommand.Contains(" helplistraw"))
                {
                    GenericCommands.HelpListRaw(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" help"))
                {
                    GenericCommands.Help(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" rolecolor"))
                {
                    DutchServerCommands.RoleColor(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" seal"))
                {
                    GlobalScoresaberCommands.Seal(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" randomgif"))
                {
                    GlobalScoresaberCommands.RandomGif(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" topsongs"))
                {
                    GlobalScoresaberCommands.TopSongs(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" topsong"))
                {
                    GlobalScoresaberCommands.NewTopSong(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" search"))
                {
                    GlobalScoresaberCommands.NewSearch(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" improve"))
                {
                    GlobalScoresaberCommands.Improve(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" updateroles"))
                {
                    DutchServerCommands.UpdateRoles(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" recentsongs"))
                {
                    GlobalScoresaberCommands.Recentsongs(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" recentsong"))
                {
                    GlobalScoresaberCommands.NewRecentSong(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" changecolor"))
                {
                    DutchServerCommands.ChangeColor(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" poll"))
                {
                    GenericCommands.Poll(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" playing"))
                {
                    GenericCommands.Playing(discordSocketClient, message);
                }

                else if (messageCommand.Contains(" invite"))
                {
                    GenericCommands.Invite(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" comparetext"))
                {
                    GlobalScoresaberCommands.Compare(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" compare"))
                {
                    GlobalScoresaberCommands.CompareNew(discordSocketClient, message);
                }                      
                else if (messageCommand.Contains(" unlink"))
                {
                    DutchServerCommands.UnLinkScoresaberFromDiscord(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" link"))
                {
                    DutchServerCommands.LinkScoresaberWithDiscord(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" profile"))
                {
                    GlobalScoresaberCommands.Profile(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" interviewtest"))
                {
                    new WelcomeInterviewHandler(discordSocketClient, message.Channel, message.Author.Id).AskForInterview();
                }
                else if (messageCommand.Contains(" number"))
                {
                    GenericCommands.Number(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" createachievement"))
                {
                    GenericCommands.CreateAchievementFeed(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" achievement"))
                {
                    GenericCommands.AchievementFeed(discordSocketClient, message);
                }                
                else if (messageCommand.Contains(" typing"))
                {
                    GlobalScoresaberCommands.Typing(discordSocketClient, message);
                    message.DeleteAsync();
                }
                else if (messageCommand.Contains(" createrankmapfeed"))
                {
                    GlobalScoresaberCommands.CreateRankMapFeed(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" ranks"))
                {
                    var builderList = await BeatSaberInfoExtension.GetRanks();
                    foreach (var builder in builderList)
                        await message.Channel.SendMessageAsync("", false, builder.Build());
                }
                else if (messageCommand.Contains(" songs"))
                {
                    await message.Channel.SendMessageAsync(null, false, EmbedBuilderExtension.NullEmbed("Ewh..", "This command is outdated. Blame silverhaze to remake it").Build());
                    //GlobalScoresaberCommands.Songs(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" irlevent create"))
                {
                    DutchServerCommands.IRLevent(discordSocketClient, message);
                }
                else if (messageCommand.Contains(" randomevent create"))
                {
                    DutchServerCommands.RandomEvent(discordSocketClient, message);
                }
                else
                {
                    var embedBuilder = EmbedBuilderExtension.NullEmbed("Oops", "There is no command like that, try something else",
                        null, null);
                    await message.Channel.SendMessageAsync(null, false, embedBuilder.Build());
                }

                typingState.Dispose();
            }
        }
    }
}