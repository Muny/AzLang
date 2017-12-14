using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzLang
{
    public class Exception
    {
        public string File;
        public int Line;
        public string Message;
        public string Detail;
        public string Suggestions;

        public Exception(string File, int Line, string Message, string Detail, string Suggestions)
        {
            this.File = File;
            this.Line = Line;
            this.Message = Message;
            this.Detail = Detail;
            this.Suggestions = Suggestions;
        }
    }
}
