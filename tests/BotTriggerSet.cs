using NUnit.Framework;

namespace fermiac.tests
{
    public class BotTriggerSet : TestBase
    {

        fermiac.BotTriggerSet dataSet;

        [SetUp]
        public void Setup()
        {
            dataSet = new fermiac.BotTriggerSet();
            dataSet.Add(new BotTrigger() {
                triggerOn = "",
                fired = true
            });
            dataSet.Add(new BotTrigger() {
                triggerOn = "firstchat",
                fired = true
            });
            dataSet.Add(new BotTrigger() {
                triggerOn = "firstchat",
                fired = false
            });
            dataSet.Add(new BotTrigger() {
                triggerOn = "racedone",
                fired = false
            });
        }

        [Test]
        public void FiredQueryWorks()
        {
            Assert.AreEqual(2, dataSet.Fired().Count);
        }

        [Test]
        public void NotFiredQueryWorks()
        {
            Assert.AreEqual(2, dataSet.NotFired().Count);
        }

        [Test]
        public void TriggerOnWorks() {
            Assert.AreEqual(2, dataSet.TriggerOn("firstchat").Count);
            Assert.AreEqual(1, dataSet.TriggerOn("racedone").Count);
            Assert.AreEqual(1, dataSet.TriggerOn("").Count);

            for(int x = 0; x < 10000; x++) {
                var nt = new BotTrigger() {
                    triggerOn = RandomString(10, 1000)
                };
                dataSet.Add(nt);
                Assert.AreEqual(1, dataSet.TriggerOn(nt.triggerOn).Count);
            }
        }

        [Test]
        public void TriggerOnCaseInsensitive() {
            Assert.AreEqual(2, dataSet.TriggerOn("FiRsTcHaT").Count);
            Assert.AreEqual(2, dataSet.TriggerOn("FIRSTCHAT").Count);
            Assert.AreEqual(2, dataSet.TriggerOn("FIRStCHAt").Count);
            Assert.AreEqual(1, dataSet.TriggerOn("racedone").Count);
            Assert.AreEqual(1, dataSet.TriggerOn("RaceDone").Count);
            Assert.AreEqual(1, dataSet.TriggerOn("RACEDONE").Count);
            Assert.AreEqual(1, dataSet.TriggerOn("rAcEdOnE").Count);

            for(int x = 0; x < 10000; x++) {
                var nt = new BotTrigger() {
                    triggerOn = RandomString(10, 1000)
                };
                dataSet.Add(nt);
                Assert.AreEqual(1, dataSet.TriggerOn(nt.triggerOn).Count);
                Assert.AreEqual(1, dataSet.TriggerOn(nt.triggerOn.ToUpper()).Count);
                Assert.AreEqual(1, dataSet.TriggerOn(nt.triggerOn.ToLower()).Count);
                Assert.AreEqual(1, dataSet.TriggerOn(nt.triggerOn.ToUpperInvariant()).Count);
                Assert.AreEqual(1, dataSet.TriggerOn(nt.triggerOn.ToLowerInvariant()).Count);
            }
        }
    }
}