using Impostor.Api.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpostorChatPlugin
{
    public class GameData
    {
        public List<IClientPlayer> Players { get; set; }
        public List<IClientPlayer> Impostors { get; set; }

        public void AddPlayer(IClientPlayer player, bool isImpostor)
        {
            Players.Add(player);
            if (isImpostor)
            {
                Impostors.Add(player);
            }
        }

        public void RemovePlayer(IClientPlayer player)
        {
            Players.Remove(player);
            Impostors.Remove(player);
        }

        public void ResetGame()
        {
            Players = new List<IClientPlayer>();
            Impostors = new List<IClientPlayer>();
        }

        public GameData()
        {
            Players = new List<IClientPlayer>();
            Impostors = new List<IClientPlayer>();
        }
    }
    
}