using System;
using System.Collections.Generic;

namespace fermiac.actions
{
    public class RandomLine : Action
    {
        // poor man's cache
        private static Dictionary<string, string[]> lines { get; set; }
        private string fn { get; set; }
        private int pause1 { get; set; }
        private int pause2 { get; set; }

        public RandomLine(dynamic options)
        {
            if(lines == null) lines = new Dictionary<string, string[]>();
            fn = options["source"].Value.ToString();
            if(!lines.ContainsKey(fn)) lines.Add(fn, System.IO.File.ReadAllLines(fn));
            pause1 = Convert.ToInt32(options["pauses"][0].Value);
            pause2 = Convert.ToInt32(options["pauses"][1].Value);
        }

        public override string Name { get { return ("randomline"); } }

        public override void Enact(BotManager f)
        {
            f.Speak("fermiac", $"Hey analog?", pause1);
            var r = new Random((int)DateTime.Now.Ticks % int.MaxValue);
            f.Speak("fermiac", lines[fn][r.Next(lines[fn].Length)], pause2);
        }
    }
}