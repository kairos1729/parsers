namespace parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class Utilities
    {
        public static IEnumerable<T> Yield<T>(this T value)
        {
            yield return value;
        }

        public static string ToCommaString<T>(this IEnumerable<T> s)
        {
            return string.Join(", ", s);
        }

        public static Program.Parser<B> Bind<A, B>(
            this Program.Parser<A> p, Func<A, Program.Parser<B>> f)
        {
            return s =>
            {
                var x = p(s);

                if (!x.Any())
                {
                    return Program.ResultForFailedParse<B>();
                }

                var y = x.First();

                return f(y.Item1)(y.Item2);
            };
        }

        public static Program.Parser<C> SelectMany<A, B, C>(
          this Program.Parser<A> parser,
          Func<A, Program.Parser<B>> function,
          Func<A, B, C> projection)
        {
            return parser.Bind(
              outer => function(outer).Bind(
                inner => projection(outer, inner).Unit()));
        }

        public static Program.Parser<B> Select<A, B>(
          this Program.Parser<A> parser, Func<A, B> function)
        {
            return parser.Bind(a => function(a).Unit());
        }

        public static Program.Parser<T> Unit<T>(this T item)
        {
            return Program.Return(item);
        }
    }
}
