using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCalc;

namespace AzLang.Extensions
{
    public static class EUtils
    {
        public static bool CheckArgs(int requiredAmount, FunctionArgs args)
        {
            if (args.Parameters.Length > requiredAmount || args.Parameters.Length < requiredAmount)
                return false;
            else
                return true;
        }

        public static bool CheckArgType(object arg, Type requiredType)
        {
            if (arg.GetType() != requiredType)
                return false;
            else
                return true;
        }
    }
}
