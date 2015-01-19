using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    internal static partial class ExtensionMethods
    {
        public static int Count<T>(this IEnumerable<T> source)
        {
            ICollection<T> c = source as ICollection<T>;

            if (c != null)
            {
                return c.Count;
            }

            UtilProject.WriteLine("Count<" + source + "> is not an ICollection: must GetEnumerator()");

            using (IEnumerator<T> enumerator = source.GetEnumerator())
            {
                int result = 0;

                while (enumerator.MoveNext())
                {
                    result++;
                }

                return result;
            }
        }

        public static void FirstOnly<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
                break;
            }
        }

        public static void FirstOnlyAssert<T>(this IEnumerable<T> source, Action<T> action)
        {
            MBox.Assert(0, source.Count() <= 1);
            FirstOnly(source, action);
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }
    }

    internal static partial class ExtensionMethods
    {
        public static string ToPrintString(this object source)
        {
            if (source == null) return null;

            string s = string.Join("", source.ToString().Cast<char>().Where(c => Char.IsControl(c) == false)).Trim();

            if (s.Length == 0) return null;                             // Returns null if empty

            return s;
        }
    }
}
