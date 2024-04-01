using Impostor.Api.Events.Managers;
using Impostor.Api.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace ImpostorChatPlugin
{
    [ImpostorPlugin(
        id:"xer.ImpostorChatPlugin"
        )]
    public class ImpostorChatPlugin : IPlugin
    {

        public readonly ILogger<ImpostorChatPlugin> _logger;
        public readonly IEventManager _eventManager;

        private IDisposable _unregister;

        public ImpostorChatPlugin(ILogger<ImpostorChatPlugin> logger, IEventManager eventManager)
        {
            _logger = logger;
            _eventManager = eventManager;
        }

        public ValueTask EnableAsync()
        {
            _logger.LogInformation("ImpostorChatPlugin enabled!");
            _unregister = _eventManager.RegisterListener(new ImpostorChatListener(_logger));
            return default;
        }

        public ValueTask DisableAsync()
        {
            _logger.LogInformation("ImpostorChatPlugin disabled!");
            _unregister.Dispose();
            return default;
        }

        public ValueTask ReloadAsync()
        {
            throw new NotImplementedException();
        }
    }
}
