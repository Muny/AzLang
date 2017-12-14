using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzLang
{
    public class KeyedVariable : AnonymousVariable
    {
        public string Key;
        public new object Value;
        public new Type Type;

        public KeyedVariable(string Key, object val, Type type) : base(val, type)
        {
            this.Key = Key;
            this.Value = val;
            this.Type = type;
        }

        public KeyedVariable(string Key, AnonymousVariable anonvar) : base(anonvar.Value, anonvar.Type)
        {
            this.Key = Key;
            this.Value = anonvar.Value;
            this.Type = anonvar.Type;
        }

        public static KeyedVariable Serialize(string Key, string rawVar, AzBaseInterpreter interpreter, string file, int line, bool debugEnabled)
        {
            AnonymousVariable anonvar = AnonymousVariable.Serialize(rawVar, interpreter, file, line, debugEnabled);

            return new KeyedVariable(Key, anonvar.Value, anonvar.Type);
        }

        public override string ToString()
        {
            if (Value != null)
            {
                if (Value.GetType() == typeof(Undefined))
                    return "Undefined";
                else
                    return Value.ToString();
            }
            else
                return "undefined";
        }
    }
}
