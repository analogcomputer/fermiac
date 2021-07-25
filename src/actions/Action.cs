using System;
using System.Threading;

namespace fermiac.actions
{
    public abstract class Action
    {
        public abstract string Name { get; }
        public abstract void Enact(BotManager f);
    }
}