using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AzLang.Extensions;
using NCalc;

namespace AzLang
{
    public class LanguageInterpreter
    {
        private CompositionContainer _container;

        [Import(typeof(IExtensionHandler))]
        public IExtensionHandler ExtensionHandler;

        public AzBaseInterpreter Interpreter;

        private Action<string> HandleWrite;
        private Action<string> HandleDebugMsg;
        private Action<Exception> HandleException;
        private Action<string> HandleNonEvaluatedLine;
        private Action HandleDoneExecuting;
        private Action<AnonymousVariable> HandleReturn = null;

        public LanguageInterpreter(string[] ExtensionPaths, Action<string> HandleWrite, Action<string> HandleDebugMsg, Action<Exception> HandleException, Action<string> HandleNonEvaluatedLine, Action HandleDoneExecuting, Action<AnonymousVariable> HandleReturn = null)
        {
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();
            //Adds all the parts found in the same assembly as the ApplicationForm class
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));

            // add extensions from base interpreter
            //catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));

            foreach (string ExtensionPath in ExtensionPaths)
            {
                if (File.Exists(ExtensionPath))
                {
                    Console.WriteLine("Loading extension: " + ExtensionPath + "...");
                    catalog.Catalogs.Add(new AssemblyCatalog(Assembly.LoadFile(ExtensionPath)));
                }
                else
                    Console.WriteLine("Error loading extension:\n  " + ExtensionPath + "\nExtension DLL nonexistent.");
            }

            _container = new CompositionContainer(catalog);

            _container.ComposeParts(this);

            this.HandleWrite = HandleWrite;
            this.HandleDebugMsg = HandleDebugMsg;
            this.HandleException = HandleException;
            this.HandleNonEvaluatedLine = HandleNonEvaluatedLine;
            this.HandleDoneExecuting = HandleDoneExecuting;
            this.HandleReturn = HandleReturn;

            RegenInterpreter();
        }

        public void RegenInterpreter()
        {
            if (Interpreter != null)
                Interpreter.Dispose();

            Interpreter = new AzBaseInterpreter(ExtensionHandler.GetExtensions(), this, false) { HandleWrite = HandleWrite, HandleDebugMsg = HandleDebugMsg, HandleException = HandleException, HandleNonEvaluatedLine = HandleNonEvaluatedLine, HandleDoneExecuting = HandleDoneExecuting, HandleReturn = HandleReturn };
        }

        public void Interpret(string Code, bool Debug, string File="VM000")
        {
            Interpreter.Interpret(Code, Debug, true, File);
        }

        public void ExecuteUserFunction(string name, FunctionArgs args)
        {
            Interpreter.ExecuteUserFunction(name, args);
        }

        public void ExecuteUserFunction(UserFunction function, FunctionArgs args)
        {
            Interpreter.ExecuteUserFunction(function, args);
        }
    }
}
