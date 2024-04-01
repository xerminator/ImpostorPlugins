using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAnnouncePlugin
{
    public class Config
    {

        public string helpMessage { get; set; } = "type / before your message to use the ImpostorChat, This Text will display in Red.";
        public string rulesMessage { get; set; } = "Rules will be updated into the Game soon.";
        public string timerMessage { get; set; } = "type ?timer to see the timer during a match.";
        public string AnnouncementMessage { get; set; } = "Please ensure all Players are Linked to AutoMuteUs and that the match is started.";
        public string wrongCommandMessage { get; set; } = "Wrong Command, please use one of the following commands: ?timer | ?help | ?rules "; 

    }
}
