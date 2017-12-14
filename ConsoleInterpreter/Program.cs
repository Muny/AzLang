using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleInterpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            AzLang.AzBaseInterpreter Interpreter = new AzLang.AzBaseInterpreter()
            {
                HandleDebugMsg = (string msg) =>
                {
                    Console.WriteLine("[DEBUG] " + msg);
                },
                HandleDoneExecuting = () =>
                {

                },
                HandleException = (AzLang.Exception exception) =>
                {
                    Console.WriteLine("[EXCEPTION] " + exception.File + ":" + exception.Line + "\n  Message: " + exception.Message + "\n  Detail: " + exception.Detail + "\n  Suggestions: " + exception.Suggestions);

                    Console.WriteLine(">> Press any key to exit.");

                    Console.ReadKey();

                    Environment.Exit(0);
                },
                HandleNonEvaluatedLine = (string line) =>
                {

                },
                HandleWrite = (string msg) =>
                {
                    Console.WriteLine(msg);
                }
            };

            string file = "testScript.az";

            Interpreter.Interpret(File.ReadAllText(file), false, file);

            Console.WriteLine(">> Press any key to exit.");

            Console.ReadKey();
        }
    }
}
