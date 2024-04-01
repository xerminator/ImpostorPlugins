using Impostor.Api.Events.Player;
using Impostor.Api.Events;
using Microsoft.Extensions.Logging;
using Impostor.Api.Net.Inner.Objects;
using System.Text;
using System.Text.Json;
using Impostor.Api.Innersloth;
using Impostor.Hazel.Abstractions;
using Impostor.Api.Innersloth.Customization;
using System;

namespace ChatAnnouncePlugin
{
    public class ChatAnnounceEventListener : IEventListener
    {
        private readonly ILogger<ChatAnnouncePlugin> _logger;
        private Config _config;
        private List<IInnerPlayerControl> list_announced = new List<IInnerPlayerControl>();

        public ChatAnnounceEventListener(ILogger<ChatAnnouncePlugin> logger,Config config)
        {
            _logger = logger;
            _config = config;
        }

        [EventListener]
        public void OnPlayerSpawned(IPlayerSpawnedEvent e)
        {
            _logger.LogInformation("Player {player} > spawned", e.PlayerControl.PlayerInfo.PlayerName);

            var clientPlayer = e.ClientPlayer;
            var playerControl = e.PlayerControl;

            Task.Run(async () =>
            {
                // Give the player time to load.
                await Task.Delay(TimeSpan.FromSeconds(3));

                bool messageSend = false;
                while (clientPlayer.Client.Connection != null && clientPlayer.Client.Connection.IsConnected && !messageSend)
                {
                    if (!list_announced.Contains(playerControl))
                    {
                        await playerControl.SendChatToPlayerAsync(_config.AnnouncementMessage);
                    }
                    await Task.Delay(TimeSpan.FromMilliseconds(3000));
                    messageSend = true;
                    list_announced.Append(playerControl);
                }
            });
        }

        [EventListener]
        public void onDisconnect(IPlayerDestroyedEvent e) {
            if (list_announced.Contains(e.PlayerControl)) {
                list_announced.Remove(e.PlayerControl);
            }
        }

        [EventListener]
        public void OnPlayerChat(IPlayerChatEvent e)
        {
            string input = e.Message;
            if (!e.Message.StartsWith("/")) return;
            List<string> commands = new List<string>{ "/timer", "/help", "/rules","/cancel","/end","/say"};
            if (!(commands.Contains(input.Split(" ")[0].Trim()))) {
                e.PlayerControl.SendChatToPlayerAsync(_config.wrongCommandMessage);
                return;
            }
            //e.IsCancelled = true;
            string message = "";
            if (input.Split(" ").Length > 1)
            {
                if (input.Split(" ")[0].Trim().Equals("/say"))
                {
                    return;
                }
                switch (e.Message.Split(" ")[1].Trim())
                {
                    case "timer":
                        message = _config.timerMessage;
                        break;
                    case "help":
                        message = _config.helpMessage;
                        break;
                    case "rules":
                        message = _config.rulesMessage;
                        break;
                    case "cancel":
                        message = "The host can cancel the game at any time with /cancel after a game has started";
                        break;
                    default:
                        message = _config.wrongCommandMessage;
                        break;
                } 
            } else
            {
                switch (e.Message.Split(" ")[0].Trim())
                {
                    case "/timer":
                        message = _config.timerMessage;
                        break;
                    case "/help":
                        message = _config.helpMessage;
                        break;
                    case "/rules":
                        message = _config.rulesMessage;
                        break;
                    case "/cancel":
                        message = "";
                        break;
                    case "/say":
                        message = "";
                        break;
                    case "/end":
                        message = "";
                        break;
                    default:
                        message = _config.wrongCommandMessage;
                        break;
                }
            }
            if (message == "") return;
            sendMessage(e.PlayerControl, message);
            
        }

        private void sendMessage(IInnerPlayerControl player, string message = "") 
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(message);
            player.SendChatToPlayerAsync(builder.ToString());
        }

    }
}