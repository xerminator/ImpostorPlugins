using Impostor.Api.Events;
using Impostor.Api.Events.Client;
using Impostor.Api.Events.Managers;
using Impostor.Api.Events.Meeting;
using Impostor.Api.Events.Player;
using Impostor.Api.Games;
using Impostor.Api.Innersloth;
using Impostor.Api.Net;
using Impostor.Api.Net.Custom;
using Impostor.Api.Net.Inner;
using Impostor.Api.Net.Inner.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace BotListener
{
    public class DiscordBotListener : IEventListener
    {

        private readonly ILogger<DiscordBot> _logger;
        private IEventManager _eventManager;
        private Dictionary<GameCode, GameData> gameDataMap { get; set; }
        private Discord _discord;
        public Dictionary<string, string> guidPlayers { get; set; }

        public DiscordBotListener(ILogger<DiscordBot> logger, IEventManager eventManager, Discord discord)
        {
            _logger = logger;
            _eventManager = eventManager;
            _discord = discord;

        }


        [EventListener]
        public bool onStarting(IGameStartingEvent e)
        {
            _logger.LogInformation("Game is starting...");
            return false;
        }

        [EventListener]
        public async void onJoin(IGamePlayerJoinedEvent e)
        {
            //_discord.SendMessage("A Player has joined!");
            await _discord.SendMessageAsync("A Player has joined");
        }




        [EventListener]
        public void onLink(IPlayerChatEvent e)
        {
            //e.PlayerControl.PlayerInfo.
            string cmd = e.Message.Split(" ")[0].Trim();
            string playername = e.PlayerControl.PlayerInfo.PlayerName;
            if (!cmd.Equals("/link")) return;
            //AddDiscordCommand(("command","get_guid_players"));
            //_discord.SendMessage();

            _logger.LogInformation("Generating new GUID for: " + e.PlayerControl.PlayerInfo.PlayerName);
            Guid guid = Guid.NewGuid();
            Dictionary<Guid, string> guid_players = new Dictionary<Guid, string>();
            //AddDiscordCommand(("command", "link"),("guid", guid.ToString()));
            //_discord.SendMessage();
            


            //guid_players.Add(guid,playername);
            //string json_string = JsonSerializer.Serialize(guid_players);
            

        }

        public void AddDiscordCommand(params(string Key, object Value)[] fields)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            foreach (var field in fields) 
            {
                dict.Add(field.Key, field.Value);
            }
            //_discord.addData(dict);
        }

    }
}
