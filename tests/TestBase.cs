using System;
using System.Linq;

namespace fermiac.tests
{
    public class TestBase
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ;':\",./<>?abcdefghijklmnopqrstuvwzyx!@#$%^&*()_+0123456789-=[]\\{}|";

        protected Random Rng { get; private set; }
        
        public TestBase()
        {
            Rng = new Random(Convert.ToInt32(DateTime.Now.Ticks % int.MaxValue));
        }

        internal string RandomString(int length)
        {
            return new string(Enumerable.Repeat(chars, length).Select(s => s[Rng.Next(s.Length)]).ToArray());
        }

        internal string RandomString(int minSize, int maxSize) {
            return(RandomString(Rng.Next(minSize, maxSize)));
        }

        internal int RandomNumber(int minSize, int maxSize) {
            return(Rng.Next(minSize, maxSize));
        }

        internal bool RandomBit()
        {
            var bit = Rng.Next(0, 2);
            return((bit % 2 == 0));
        }
    }
}