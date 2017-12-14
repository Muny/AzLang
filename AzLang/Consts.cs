using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzLang
{

    public enum Operator
    {
        Set,
        Add,
        Subtract,
        Multiply,
        Divide
    }

    public static class Consts
    {
        public static class Exceptions
        {
            public static Exception TooManyArgs(string File, int Line)
            {
                return new Exception(File, Line, "Too many arguments", "The method called requires a certain amount of arguments, the number of arguments provided exceeded the number of arguments required.", "Remove unnecessary arguments.");
            }

            public static Exception NotEnoughArgs(string File, int Line)
            {
                return new Exception(File, Line, "Not enough arguments", "The method called requires a certain amount of arguments, the number of arguments provided was less than the number of arguments required.", "Add required arguments.");
            }

            public static Exception ParameterCountMismatch(string File, int Line, int requiredAmount)
            {
                return new Exception(File, Line, "Parameter count mismatch.", "The method called requires " + requiredAmount + " parameters.", "Add or remove parameters as necessary.");
            }

            public static Exception NoScopeEnd(string File, int Line)
            {
                return new Exception(File, Line, "Scope end expected.", "A scope end ('}') was expected, but not found.", "Add '}' to end scope.");
            }
        }

        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this IEnumerable<Dictionary<TKey, TValue>> enumerable)
        {
            return enumerable.SelectMany(x => x).ToDictionary(x => x.Key, y => y.Value);
        }
    }
}
