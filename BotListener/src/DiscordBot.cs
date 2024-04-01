using BotListener;
using Impostor.Api.Events.Managers;
using Impostor.Api.Plugins;
using Microsoft.Extensions.Logging;

namespace BotListener
{
    [ImpostorPlugin(id: "xer.res.discordbot")]
    public class DiscordBot : PluginBase
    {


        private readonly ILogger<DiscordBot> _logger;
        private readonly IEventManager _eventmanager;
        private IDisposable _unregister;
        private readonly double version = 1.0;
        private Discord discord;

        public DiscordBot(ILogger<DiscordBot> logger, IEventManager eventManager)
        {
            _logger = logger;
            _eventmanager = eventManager;
            discord = new Discord();
        }

        public override ValueTask EnableAsync()
        {
            _logger.LogInformation($"DiscordBot {version} enabled!");
            _unregister = _eventmanager.RegisterListener(new DiscordBotListener(_logger, _eventmanager, discord));
            //discord.Connect();
            return default;
        }

        public override ValueTask DisableAsync()
        {
            _logger.LogInformation($"DiscordBot {version} disabled");
            _unregister.Dispose();
            //discord.Close();
            return default;
        }



    }
}