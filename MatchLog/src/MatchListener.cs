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
using Impostor.Api.Net.Inner.Objects.ShipStatus;
using Impostor.Api.Net.Messages.Rpcs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MatchLog
{
    public class MatchListener : IEventListener
    {

        private readonly ILogger<MatchLog> _logger;
        private IEventManager _eventManager;
        private Dictionary<GameCode, GameData> gameDataMap = new();
        public MatchListener(ILogger<MatchLog> logger, IEventManager eventManager) {
            _logger = logger;
            _eventManager = eventManager;

        }

        [EventListener]
        public void onGameStarted(IGameStartedEvent e) {
            var gameData = new GameData(e.Game.Code);
            foreach (var player in e.Game.Players) {
                if (player.Character == null) return;
                gameData.AddPlayer(player, player.Character.PlayerInfo.IsImpostor);
            }
            gameData.gameStartTimeSet();
            gameDataMap.Add(e.Game.Code, gameData);
        }

        [EventListener]
        public void onGameEnd(IGameEndedEvent e)
        {
            if (e.Game == null) return;
            GameCode gamecode = e.Game.Code;

            if (gameDataMap.ContainsKey(gamecode))
            {


                //get all data

                var gameData = gameDataMap[gamecode];
                var dbEntry = gameData.stringifyData();
                var rawData = gameData.jsonifyRawData();
                var resultString = gameData.canceled ? "Canceled" : getResult(e.GameOverReason);
                string randomString = $"{GenerateRandomAlphanumericString(16)}";
                _logger.LogInformation(dbEntry.ToString());
                //Write logfile to directory
                string workingDirectory = Environment.CurrentDirectory;
                string directoryPath = Path.Combine(workingDirectory, "plugins", "MatchLog");
                //Directory.CreateDirectory(directoryPath);
                string filePath = Path.Combine(directoryPath, $"{randomString}_events.json");
                string pattern = "*_match.json";
                string[] matchFiles = Directory.GetFiles(directoryPath, pattern);

                //string dbEntryTemp = Path.Combine(directoryPath, $"{randomString}dbEntry");
                File.WriteAllText(filePath, rawData);

                var newMatch = new Match {
                    MatchID = matchFiles.Length - 1,
                    gameStarted = dbEntry[0],
                    players = dbEntry[1],
                    impostors = dbEntry[2],
                    eventsLogFile = $"{randomString}_events.json",
                    result = resultString,
                    reason = e.GameOverReason.ToString()
                };

                string matchJson = JsonSerializer.Serialize<Match>(newMatch);
                string matchFilePath = Path.Combine(directoryPath, $"{randomString}_match.json");
                File.WriteAllText(matchFilePath, matchJson);



                gameDataMap[gamecode].ResetGame();
                gameDataMap.Remove(gamecode);

                //newMatch.Process();


            }

        }

        public string getResult(GameOverReason reason)
        {
            List<GameOverReason> crew = new List<GameOverReason> { GameOverReason.HumansByVote, GameOverReason.HumansByTask};
            List<GameOverReason> imp = new List<GameOverReason> { GameOverReason.ImpostorByKill, GameOverReason.ImpostorBySabotage, GameOverReason.ImpostorByVote};
            //List<GameOverReason> cancel = new List<GameOverReason> { GameOverReason.ImpostorDisconnect, GameOverReason.HumansDisconnect};
            string result = "Unknown";
            if(crew.Contains(reason))
            {
                result = "Crewmates Win";
            } else if(imp.Contains(reason))
            {
                result = "Impostors Win";
            }
            return result;
        }

        [EventListener]
        public void onReport(IPlayerStartMeetingEvent e)
        {
            if(gameDataMap.ContainsKey(e.Game.Code))
            {
                gameDataMap[e.Game.Code].inMeeting = true;
                bool bodyreport = false;
                if(e.ClientPlayer == null)
                {
                    //Should never happen
                    return;
                }
                if (e.Body != null) {
                    bodyreport = true;
                }
                if (bodyreport) {
                    if(e.ClientPlayer.Character != null && e.Body != null)
                    gameDataMap[e.Game.Code].onReport(e.ClientPlayer.Character.PlayerInfo.PlayerName, e.Body.PlayerInfo.PlayerName, DateTime.Now);
                } else if(e.ClientPlayer.Character != null)
                {
                    gameDataMap[e.Game.Code].startMeeting(e.ClientPlayer.Character.PlayerInfo.PlayerName, DateTime.Now);
                } else
                {
                    //Error
                    return;
                }
            }
        }

        //[EventListener]
        //public void onMeetingStart(IMeetingStartedEvent e) {
        //    if (gameDataMap.ContainsKey(e.Game.Code)) {
        //        gameDataMap[e.Game.Code].startMeeting(DateTime.Now);
        //    }
        //}

        [EventListener]
        public void onMeetingEnd(IMeetingEndedEvent e) {
            if (gameDataMap.ContainsKey(e.Game.Code)) {
                gameDataMap[e.Game.Code].inMeeting = false;
                gameDataMap[e.Game.Code].endMeeting(DateTime.Now);
            }
        }

        [EventListener]
        public async void onPlayerChat(IPlayerChatEvent e)
        {
            if (e.Game.GameState != GameStates.Started) return;
            if (e.Message.Equals("/cancel")) {
                e.IsCancelled = true;
                if (!e.ClientPlayer.IsHost) return;
                //_logger.LogInformation("Command to Cancel game recieved");
                if (gameDataMap.ContainsKey(e.Game.Code))
                {
                    //_logger.LogInformation("GameData contains game");

                    //_logger.LogInformation("Setting the game to canceled in gameData");
                    gameDataMap[e.Game.Code].canceled = true;
                    gameDataMap[e.Game.Code].onCancel(e.PlayerControl.PlayerInfo.PlayerName, DateTime.Now);
                    await e.PlayerControl.SendChatToPlayerAsync("Game has been logged as a cancel, type /end to end the game");
                    //_logger.LogInformation("Attempting to Exile all players to force the game to end and exclude the host");
                    //var imp = gameDataMap[e.Game.Code].Impostors[0];
                    


                }
            }
            if(e.Message.Equals("/end"))
            {
                e.IsCancelled = true;
                if (!gameDataMap[e.Game.Code].canceled) return;
                gameDataMap[e.Game.Code].onEnd(e.ClientPlayer, DateTime.Now);
                if (gameDataMap.ContainsKey(e.Game.Code))
                {
                    if (gameDataMap[e.Game.Code].Impostors.Count > 0)
                    {
                        foreach(var player in gameDataMap[e.Game.Code].Players) 
                        {
                            if (!player.Character.PlayerInfo.IsDead && !gameDataMap[e.Game.Code].Impostors.Contains(player)) {
                                await gameDataMap[e.Game.Code].Impostors[0].Character.MurderPlayerAsync(player.Character);
                            }
                        }
                    } 
                }
            }

            else
            {
                return;
            }
        }

        [EventListener]
        public void onVote(IPlayerVotedEvent e) {
            string voted = "none";
            string player = "none";
            if (e.VotedFor != null) {
                voted = e.VotedFor.PlayerInfo.PlayerName;
            }
            if(e.ClientPlayer.Character != null)
            {
                player = e.ClientPlayer.Character.PlayerInfo.PlayerName;
            }
            if (gameDataMap.ContainsKey(e.Game.Code)) {
                gameDataMap[e.Game.Code].addVote(player,DateTime.Now, voted, e.VoteType);
            }
        }

        [EventListener]
        public void onPlayerMurder(IPlayerMurderEvent e) {
            var playerKilled = e.Victim;
            var currentGame = e.Game.Code;
            _logger.LogInformation($"{playerKilled.PlayerInfo.PlayerName} killed");
            var killedClient = e.Game.Players.FirstOrDefault(p => p.Character == playerKilled);

            DateTime dateTime = DateTime.Now;

            if (killedClient != null && gameDataMap.ContainsKey(currentGame))
            {
                gameDataMap[currentGame].onDeathEvent(killedClient, dateTime);
            }
        }

        
        [EventListener]
        public void onTaskCompletion(IPlayerCompletedTaskEvent e) {
            if (e.ClientPlayer.Character == null) return;
            var player = e.ClientPlayer;
            var task = e.Task;
            var timeOfcompletion = DateTime.UtcNow;
            _logger.LogInformation($"{player.Character.PlayerInfo.PlayerName} did task action, completed: {(task.Complete ? ("yes") : ("no"))}");
            if (task.Complete) {
                gameDataMap[player.Game.Code].addTaskCompletion(player, timeOfcompletion, task.Task.Category, task.Task.Type);
            }
        }


        [EventListener]
        public void onDisconnection(IGamePlayerLeftEvent e)
        {
            DateTime dateTime = DateTime.Now;
            if (gameDataMap.ContainsKey(e.Game.Code))
            {
                gameDataMap[e.Game.Code].disconnectedPlayer(dateTime, e.Player);
            }
        }

        private static string GenerateRandomAlphanumericString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }

    }
}
