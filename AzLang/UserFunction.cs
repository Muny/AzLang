using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzLang
{
    public class UserFunction : AnonymousVariable
    {
        public string Code;

        public string[] VariableKeys;

        public string Name;

        public UserFunction(string Code, string[] VariableKeys, string Name)
        {
            this.Code = Code;
            this.VariableKeys = VariableKeys;
            this.Name = Name;
        }
    }
}
