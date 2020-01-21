using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeDataGenerator
{
    public static class Extensions
    {
        /// <summary>
        /// Get a value
        /// </summary>
        public static String Get<T>(this IEnumerable<T> me, int indx)
        {
            return me.Skip(indx % me.Count()).Take(1).First().ToString();
        }
    }
}
