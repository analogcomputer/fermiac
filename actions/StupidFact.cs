using System;
using System.Threading;

namespace fermiac.actions
{
    public class StupidFact : Action
    {
        private static string[] stupidFacts { get; set; }

        public StupidFact() {
            if(stupidFacts == null) {
                try {
                    stupidFacts = System.IO.File.ReadAllLines("stupidfacts.txt");
                } catch {
                    stupidFacts = new string[0];
                }
            }
        }

        public override string Name { get { return("stupidfact"); } }
        
        public override void Enact(BotManager f)
        {
            f.Speak("fermiac", $"Hey analog?", 750);
            var r = new Random((int)DateTime.Now.Ticks % int.MaxValue);
            f.Speak("fermiac", stupidFacts[r.Next(stupidFacts.Length)], 6750);
        }
    }
}