namespace parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Program
    {
        private static readonly Dictionary<string, int> RegisterValues = 
            new Dictionary<string, int>
            {
                {"ONE", 1},
                {"TWO", 2},
            };

        public delegate IEnumerable<Tuple<T, string>> Parser<T>(string s);

        public static readonly Parser<char> item =
            s => string.IsNullOrEmpty(s)
                ? ResultForFailedParse<char>()
                : new Tuple<char, string>(s[0], s.Substring(1)).Yield();

        public static IEnumerable<Tuple<T, string>> ResultForFailedParse<T>()
        {
            return Enumerable.Empty<Tuple<T, string>>();
        }

        public static Parser<T> Failure<T>()
        {
            return _ => Enumerable.Empty<Tuple<T, string>>();
        }

        public static Parser<T> Return<T>(T value)
        {
            return s => new Tuple<T, string>(value, s).Yield();
        }

        public static Parser<T> Choice<T>(Parser<T> choice1, Parser<T> choice2)
        {
            return s =>
                {
                    var result = choice1(s);

                    if (!result.Any())
                    {
                        result = choice2(s);
                    }

                    return result;
                };
        }

        public static Parser<char> Satisfies(Predicate<char> predicate)
        {
            return from x in item
                   from result in predicate(x) ? x.Unit() : Failure<char>()
                   select result;
        }

        public static Parser<char> Digit()
        {
            return Satisfies(c => char.IsDigit(c));
        }

        public static Parser<char> Char(char c)
        {
            return Satisfies(x => x == c);
        }

        public static Parser<string> String(string s)
        {
            return from c in Char(s[0])
                   from rest in String(s.Substring(1))
                   select c + rest;
        }

        public static Parser<T> Token<T>(Parser<T> parser)
        {
            return from _ in Whitespaces()
                   from result in parser
                   from __ in Whitespaces()
                   select result;
        }

        public static Parser<char> Whitespace()
        {
            return Satisfies(char.IsWhiteSpace);
        }

        public static Parser<IEnumerable<char>> Whitespaces()
        {
            return Many(Whitespace());
        }

        public static Parser<IEnumerable<T>> Many<T>(Parser<T> parser)
        {
            return Choice(Many1(parser), Return(Enumerable.Empty<T>()));
        }

        public static Parser<IEnumerable<T>> Many1<T>(Parser<T> parser)
        {
            return from first in parser
                   from rest in Many(parser)
                   select first.Yield().Concat(rest);
        }

        public static Parser<int> NaturalNumber()
        {
            int value;
            return from digits in Many(Digit())
                   from result in int.TryParse(
                        new string(digits.ToArray()), out value)
                            ? value.Unit()
                            : Failure<int>()
                   select result;
        }

        public static Parser<int> Expr()
        {
            return Choice(
                    from left in Token(Fact())
                    from _ in Token(Char('+'))
                    from right in Expr()
                    select left + right,
                    Fact());
        }

        public static Parser<int> Fact()
        {
            return Choice(
                from left in Atom()
                from _ in Token(Char('*'))
                from right in Fact()
                select left * right,
                Atom());
        }

        public static Parser<int> Atom()
        {
            return Choice(
                Choice(
                    from _ in Token(Char('('))
                    from e in Expr()
                    from __ in Token(Char(')'))
                    select e,
                    Token(NaturalNumber())),
                    Token(Register()));
        }

        public static Parser<int> Register()
        {
            return from r in Many1(Satisfies(char.IsUpper))
                   let rs = new string(r.ToArray())
                   from result in RegisterValues.ContainsKey(rs)
                   ? Return(RegisterValues[rs])
                   : Failure<int>()
                   select result;
        }

        static void Main(string[] args)
        {
            var y = Expr()("(1 + 2) * 3 * ((4 + 50 + ONEX) * (TWO + ONE))");

            if (y.Any())
            {
                Console.WriteLine("Value=" + y.First().Item1 + " unparsed=" + y.First().Item2);
            }
            else
            {
                Console.WriteLine("Error");
            }

            Console.ReadLine();
        }
    }
}
