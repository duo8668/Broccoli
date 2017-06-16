using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Extensions
{
    public static class ExtensionMethods
    {
        /**
        * Give any Enumerable a ForEach Method.
        */
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> handler)
        {
            if (enumerable == null) return;

            foreach (T value in enumerable)
            {
                handler(value);
            }
        }

        /**
         * A Linq`ish way of breaking out of a ForEach.
         *
         * ```cs
         * 	var fooList = new List<string>{ "abc", "xyz" };
         *
         * 	fooList.ForEach(value =>
         *  {
         *  	if (value == "abc")
         *  	{
         *  		// break out of the foreach
         *  		return false;
         *  	}
         *
         * 		// if null or true is returned, the loop will continue;
         * 	});
         * ```
         */
        public static void ForEach<T>(this IEnumerable<T> enumerable, Func<T, bool> handler)
        {
            if (enumerable == null) return;

            foreach (T value in enumerable)
            {
                var result = handler(value);

                if (result == false)
                {
                    break;
                }
            }
        }

        /**
         * A Linq`ish way of iterating over an Enumerable with an index.
         *
         * ```cs
         * 	var fooList = new List<string>{ "abc", "xyz" };
         *
         * 	fooList.ForEach((key, value) =>
         *  {
         *  	Console.WriteLine(key + ": " + value);
         * 	});
         * ```
         *
         * _Credit: http://stackoverflow.com/questions/43021_
         */
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<int, T> handler)
        {
            if (enumerable == null) return;

            int key = 0;
            foreach (T value in enumerable)
            {
                handler(key++, value);
            }
        }

        /**
         * A Linq`ish way of breaking out of a ForEach.
         *
         * ```cs
         * 	var fooList = new List<string>{ "abc", "xyz" };
         *
         * 	fooList.ForEach((key, value) =>
         *  {
         *  	if (value == "abc")
         *  	{
         *  		// break out of the foreach
         *  		return false;
         *  	}
         *
         * 		// if null or true is returned, the loop will continue;
         * 	});
         * ```
         */
        public static void ForEach<T>(this IEnumerable<T> enumerable, Func<int, T, bool> handler)
        {
            if (enumerable == null) return;

            int key = 0;
            foreach (T value in enumerable)
            {
                var result = handler(key++, value);

                if (result == false)
                {
                    break;
                }
            }
        }


        /**
    * Give any Enumerable a ForEach Method.
    */

    }

    public static class NumberExtensions
    {
        public static bool IsNumber(this object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }
    }
}
