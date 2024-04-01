using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpostorChatPlugin
{
    public interface IImpostorChat
    {
        void onGameStarted(IGameStartedEvent e) { }
        void onPlayerChat(IPlayerChatEvent e) { }
        void onGameEnd(IGameEndedEvent e) { }
    }
}
