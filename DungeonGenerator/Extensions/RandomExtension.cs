using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonGenerator.Extensions
{
    public static class RandomExtensions
    {
        public static void Shuffle<T>(this Random rng, T[] array)
        {
            var n = array.Length;
            while (n > 1)
            {
                var k = rng.Next(n--);
                (array[n], array[k]) = (array[k], array[n]);
            }
        }

        public static int[] CreateShuffleInt(this Random rng, int n)
        {
            var array = Enumerable.Range(0, n).ToArray();
            while (n > 1)
            {
                var k = rng.Next(n--);
                (array[n], array[k]) = (array[k], array[n]);
            }

            return array;
        }

        public static int GetRand(this Random _rand, int   min, int   max) => min <= max ? _rand.Next(min,      max) : _rand.Next(max,           min);
        public static int GetRand(this Random _rand, float min, float max) => min <= max ? _rand.Next((int)min, (int)max) : _rand.Next((int)max, (int)min);
    }
}
