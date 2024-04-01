using Impostor.Api.Events.Managers;
using Impostor.Api.Plugins;
using Microsoft.Extensions.Logging;

namespace MatchLog
{
    [ImpostorPlugin(id: "xer.auc.matchlog")]
    public class MatchLog : PluginBase
    {


        public readonly ILogger<MatchLog> _logger;
        public readonly IEventManager _eventmanager;
        private IDisposable _unregister;
        private readonly double version = 1.0;

        public MatchLog(ILogger<MatchLog> logger, IEventManager eventManager) {
            _logger = logger;
            _eventmanager = eventManager;
        }

        public override ValueTask EnableAsync()
        {

            string dataDirectory = Environment.CurrentDirectory;
            string directoryPath = Path.Combine(dataDirectory, "plugins", "MatchLog");
            bool directoryExists = Directory.Exists(directoryPath);

            if (!directoryExists) {
                Directory.CreateDirectory(directoryPath);
            }

            _logger.LogInformation($"MatchLog {version} enabled!");
            _unregister = _eventmanager.RegisterListener(new MatchListener(_logger, _eventmanager));
            return default;
        }

        public override ValueTask DisableAsync()
        {
            _logger.LogInformation($"MatchLog {version} disabled");
            _unregister.Dispose();
            return default;
        }

    }
}