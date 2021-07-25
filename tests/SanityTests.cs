using NUnit.Framework;
using System;

namespace fermiac.tests
{
    public class SanityTests : TestBase
    {

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void RandomBitIsFairEnough()
        {
            var samples = 1000000;
            var percentage = 1;
            var tcount = 0;
            var fcount = 0;
            for(int x = 0; x < samples; x++) {
                if(RandomBit()) {
                    tcount++;
                } else {
                    fcount++;
                }
            }

            var delta = Math.Abs(tcount - fcount);
            var maxDelta = samples * (percentage / 100.0);
            Assert.True(delta < maxDelta, $"Highest variance should be {maxDelta}, tested at {delta}");
        }
        
        [Test]
        public void RandomNumberIsFairEnough()
        {
            var samples = 100000000;
            var percentage = 0.5;
            var setSize = 50;

            var counts = new int[setSize];
            for(int x = 0; x < samples; x++) {
                var idx = RandomNumber(0, setSize);
                counts[idx]++;
            }

            var expected = samples / setSize;
            var maxDelta = expected * (percentage / 100.0);

            for(int x = 0; x < setSize; x++) {
                var delta = Math.Abs(expected - counts[x]);
                Assert.True(delta < maxDelta, $"Highest variance for index {x} should be {maxDelta}, tested at {delta}");
            }
        }

        [Test]
        public void RandomStringIsProperLength()
        {
            var samples = 100000;
            for(int x = 0; x < samples; x++) {
                var upperBound = RandomNumber(1,500);
                var str = RandomString(1, upperBound);
                Assert.True(1 <= str.Length);
                Assert.True(str.Length <= upperBound);
            }

            for(int x = 0; x < samples; x++) {
                var upperBound = RandomNumber(11,500);
                var str = RandomString(10, upperBound);
                Assert.True(10 <= str.Length);
                Assert.True(str.Length <= upperBound);
            }

        }
    }
}