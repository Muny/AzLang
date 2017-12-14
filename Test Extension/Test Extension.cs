using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzLang;
using AzLang.Extensions;
using NCalc;

namespace Test_Extension
{
    [Export(typeof(AzLang.Extensions.IExtension))]
    [ExportMetadata("Identifier", "Test Extension")]
    public class Test_Extension : IExtension
    {
        public Dictionary<string, Action<LanguageInterpreter, FunctionArgs>> GetStockFunctions()
        {
            return new Dictionary<string, Action<LanguageInterpreter, FunctionArgs>>() { {"test_write", (LanguageInterpreter sender, FunctionArgs args) =>
                {
                    Console.WriteLine("this is test write");
                }} };
        }

        public void OnDisabled()
        {

        }

        public void OnEnabled()
        {

        }
    }
}
