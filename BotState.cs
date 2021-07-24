using System.Collections.Generic;
using System.Linq;

namespace fermiac {

    public class BotTrigger {
        public bool fired { get; set; }
        public string triggerOn { get; set; }
        public string triggerFor { get; set; }
        public string action { get; set; }
        public string options { get; set; }
        public string frequency { get; set; }
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
            return new BotTriggerSet(this.Where(tx => tx.triggerOn.ToLower() == name.ToLower()));
        }
        public BotTriggerSet TriggerFor(string value) {
            var rset = this.Where(tx => tx.triggerFor.ToLower() == value.ToLower());
            if(rset.Count() == 0) {
                this.Where(tx => tx.triggerFor == "*");
            }
            return new BotTriggerSet(rset);
        }

        public BotTriggerSet Fired() {
            return new BotTriggerSet(this.Where(tx => tx.fired));
        }

        public BotTriggerSet NotFired() {
            return new BotTriggerSet(this.Where(tx => !tx.fired));
        }
    }

    public class BotState {
        public BotTriggerSet triggers { get; set; }
    }
}