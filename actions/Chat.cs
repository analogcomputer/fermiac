using System;
using System.Threading;

namespace fermiac.actions
{
    public class Chat : Action
    {
        private string msg { get; set; }
        public Chat(string message) {
            msg = message;
        }

        public override string Name { get { return("chat"); } }
        
        public override void Enact(BotManager f)
        {
            var msgs = msg.Split('|');
            foreach(var txt in msgs) {
                if(txt.Contains(";")) {
                    var parts = txt.Split(';');
                    var waitMS = Convert.ToInt32(parts[0]);
                    var toSay = parts[1];
                    f.Speak("fermiac", toSay, waitMS);
                } else {
                    f.Speak("fermiac", txt, 0);
                }
            }
        }
    }
}