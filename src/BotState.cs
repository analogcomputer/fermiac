using System.Collections.Generic;
using System.Linq;

namespace fermiac {

    public class BotTrigger {
        public BotTrigger() {
            triggerExceptions = new string[0];
        }

        public fermiac.actions.Action ToAction()
        {
            switch(this.action.ToLower()) {
                case "chat": return new actions.Chat(this.options);
                case "randomline": return new actions.RandomLine(this.options);
                case "text": {
                    // acting on a bang command carries a timeout
                    this.AvailableOn = System.DateTime.Now.AddSeconds(37);
                    return new actions.Text(this.options);
                } 
                default: return null; // unsupported action
            }
        }

        public bool fired { get; set; }
        public string triggerOn { get; set; }
        public string triggerFor { get; set; }
        public string action { get; set; }
        public dynamic options { get; set; }
        public string frequency { get; set; }
        public string[] triggerExceptions { get; set; }
        public System.DateTime AvailableOn { get; set; }
    }

    public class BotTriggerSet : List<BotTrigger>
    {
        public BotTriggerSet() : base()
        {
        }

        public BotTriggerSet(IEnumerable<BotTrigger> set) : base(set)
        {
            
        }

        public BotTriggerSet TriggerOn(string name) {
            return new BotTriggerSet(this.Where(tx => tx.triggerOn.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)));
        }
        
        public BotTriggerSet TriggerFor(string value) {
            var rset = this.Where(tx => tx.triggerFor.Equals(value, System.StringComparison.InvariantCultureIgnoreCase));
            if(rset.Count() == 0) {
                this.Where(tx => tx.triggerFor == "*");
            }
            rset = rset.Where(tx => !tx.triggerExceptions.Any(tex => tex.Equals(value, System.StringComparison.InvariantCultureIgnoreCase)));
            return new BotTriggerSet(rset);
        }

        public BotTriggerSet Fired() {
            return new BotTriggerSet(this.Where(tx => tx.fired));
        }

        public BotTriggerSet NotFired() {
            return new BotTriggerSet(this.Where(tx => !tx.fired && tx.AvailableOn <= System.DateTime.Now ));
        }
    }

    public class BotState {
        public BotTriggerSet triggers { get; set; }
    }
}