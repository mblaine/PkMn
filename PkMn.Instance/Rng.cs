using System;

namespace PkMn.Instance
{
    public static class Rng
    {
        private static int Seed = 0;

        private static Random instance;

        public static int Next()
        {
            if(instance == null)
                instance = Seed > 0 ? new Random(Seed) : new Random();

            return instance.Next();
        }

        public static int Next(int maxValue)
        {
            if (instance == null)
                instance = Seed > 0 ? new Random(Seed) : new Random();

            return instance.Next(maxValue);
        }

        public static int Next(int minValue, int maxValue)
        {
            if (instance == null)
                instance = Seed > 0 ? new Random(Seed) : new Random();

            return instance.Next(minValue, maxValue);
        }
    }
}
