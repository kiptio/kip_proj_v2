using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kip.Utils.Core
{
    public static class EnumerableUtils
    {
        private static Random _random;
        static EnumerableUtils()
        {
            _random = new Random();
        }

        public static T GetRandom<T>(this IEnumerable<T> superset)
        {
            return superset.ToArray()[_random.Next(0, superset.Count())];
        }

        public static T GetRandomOrDefault<T>(this IEnumerable<T> superset)
        {
            if (null == superset || superset.Count() == 0) return default(T);

            return superset.ToArray()[_random.Next(0, superset.Count())];
        }
    }
}
