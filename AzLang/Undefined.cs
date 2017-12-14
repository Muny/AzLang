using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzLang
{
    public class Undefined : AnonymousVariable
    {
        public Undefined() : base("Undefined", typeof(Undefined)) {
            this.RawVariable = "";
            this.Type = typeof(Undefined);
            this.Value = "Undefined";
        }

        public override string ToString()
        {
            return "Undefined";
        }
    }
}
