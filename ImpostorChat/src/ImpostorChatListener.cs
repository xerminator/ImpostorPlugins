using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Microsoft.Extensions.Logging;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Net;
using Impostor.Hazel.Abstractions;
using Impostor.Api.Net.Messages.Rpcs;
using Impostor.Api.Innersloth;
using Impostor.Api.Games;
using Impostor.Api.Events.Client;

namespace ImpostorChatPlugin
{
    public class ImpostorChatListener : IEventListener, IImpostorChat
    {
        private readonly ILogger<ImpostorChatPlugin> _logger;
        private Dictionary<GameCode, GameData> gameDataMap = new Dictionary<GameCode, GameData>();


        public ImpostorChatListener(ILogger<ImpostorChatPlugin> logger)
        {
            _logger = logger;
        }

        [EventListener]
        public void onGameStarted(IGameStartedEvent e)
        {
            var gameData = new GameData();
            foreach (var player in e.Game.Players)
            {
                gameData.AddPlayer(player, player.Character.PlayerInfo.IsImpostor);
            }
            gameDataMap.Add(e.Game.Code, gameData);
        }

        [EventListener]
        public void onGameEnd(IGameEndedEvent e) 
        {
            gameDataMap[e.Game.Code].ResetGame();
            gameDataMap.Remove(e.Game.Code);
        }

        [EventListener]
        public void onPlayerChat(IPlayerChatEvent e)
        {
            
            if (e.Game.GameState == GameStates.NotStarted) return;
            if (e.ClientPlayer == null || e.ClientPlayer.Character == null) return;
            //string prefix = "/say";
            //if (!e.Message.StartsWith(prefix)) return;
            if (!e.Message.Split(" ")[0].Equals("/say")) return;
            //e.IsCancelled = true;
            var impostors = gameDataMap[e.Game.Code].Impostors;

            //TODO: Make the sender_msg and reciever_msg configurable, for now hardcoded.
            if (!impostors.Contains(e.ClientPlayer)) {
                //string sender_msg = "Impostor Chat is not available for Crewmates.";
                //e.ClientPlayer.Character.SendChatToPlayerAsync(sender_msg, e.ClientPlayer.Character);
                return;
            }

            //if (exclude_cmds.Any(x => x == e.Message)) return;
            foreach (var player in impostors)
            {
                if (player == null) return;
                if (player != e.ClientPlayer)
                {
                    string reciever_msg = "<#c51111>" + e.Message.Remove(0, 5);
                    //player.Character.SendChatToPlayerAsync(e.Message, player.Character);
                    if (e.ClientPlayer.Character.PlayerInfo.IsDead)
                    {
                        player.Character.SendChatToPlayerAsync(reciever_msg, player.Character);
                    }
                    else
                    {
                        e.ClientPlayer.Character.SendChatToPlayerAsync(reciever_msg, player.Character);
                    }
                }
            }
            

        }
    }

}
