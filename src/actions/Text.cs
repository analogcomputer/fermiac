using System;
using System.Threading;

namespace fermiac.actions
{
    public class Text : Action
    {
        private string msg { get; set; }
        public Text(string message) {
            msg = message;
        }

        public override string Name { get { return("text"); } }
        
        public override void Enact(BotManager f)
        {
            f.Text("analogcomputer", msg);
        }
    }
}